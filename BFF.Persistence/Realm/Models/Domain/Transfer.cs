using System;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class Transfer : Model.Models.Transfer, IRealmModel<Trans>
    {
        private readonly IUpdateBudgetCache _updateBudgetCache;
        private readonly RealmObjectWrap<Trans> _realmObjectWrap;

        public Transfer(
            ICrudOrm<Trans> crudOrm,
            IUpdateBudgetCache updateBudgetCache,
            IRxSchedulerProvider rxSchedulerProvider,
            Trans realmObject,
            DateTime date,
            IFlag flag, 
            string checkNumber, 
            IAccount fromAccount,
            IAccount toAccount,
            string memo, 
            long sum,
            bool cleared) : base(rxSchedulerProvider, date, flag, checkNumber, fromAccount, toAccount, memo, sum, cleared)
        {
            _updateBudgetCache = updateBudgetCache;
            _realmObjectWrap = new RealmObjectWrap<Trans>(
                realmObject,
                realm =>
                {
                    var dbSetting = realm.All<Persistence.DbSetting>().First();
                    var id = dbSetting.NextTransId++;
                    realm.Add(dbSetting, true);
                    var ro = new Trans{ Id = id };
                    UpdateRealmObject(ro);
                    return ro;
                },
                UpdateRealmObject,
                crudOrm);
            
            void UpdateRealmObject(Trans ro)
            {
                ro.FromAccount =
                    FromAccount is null
                        ? null
                        : (FromAccount as Account)?.RealmObject
                          ?? throw new ArgumentException("Model objects from different backends shouldn't be mixed");
                ro.ToAccount =
                    ToAccount is null
                        ? null
                        : (ToAccount as Account)?.RealmObject
                          ?? throw new ArgumentException("Model objects from different backends shouldn't be mixed");
                ro.Date = new DateTimeOffset(Date, TimeSpan.Zero);
                ro.CheckNumber = CheckNumber;
                ro.Flag =
                    Flag is null
                        ? null
                        : (Flag as Flag)?.RealmObject
                          ?? throw new ArgumentException("Model objects from different backends shouldn't be mixed");
                ro.Memo = Memo;
                ro.Sum = Sum;
                ro.Cleared = Cleared;
                ro.TypeIndex = (int) TransType.Transfer;

                ro.Account = null;
                ro.Payee = null;
                ro.Category = null;
            }
        }

        public override bool IsInserted => _realmObjectWrap.IsInserted;

        public Trans RealmObject => _realmObjectWrap.RealmObject;

        public override async Task InsertAsync()
        {
            await _realmObjectWrap.InsertAsync().ConfigureAwait(false);
            var temp = _realmObjectWrap.RealmObject;
            if (temp != null)
            {
                var month = new DateTimeOffset(
                    temp.Date.Year,
                    temp.Date.Month,
                    1, 0, 0, 0, TimeSpan.Zero);
                await _updateBudgetCache.OnTransferInsertOrDeleteAsync(temp.FromAccount, temp.ToAccount, month)
                    .ConfigureAwait(false);
            }
        }

        public override async Task DeleteAsync()
        {
            var temp = _realmObjectWrap.RealmObject;
            await _realmObjectWrap.DeleteAsync().ConfigureAwait(false);
            if (temp != null)
            {
                var month = new DateTimeOffset(
                    temp.Date.Year,
                    temp.Date.Month,
                    1, 0, 0, 0, TimeSpan.Zero);
                await _updateBudgetCache.OnTransferInsertOrDeleteAsync(temp.FromAccount, temp.ToAccount, month)
                    .ConfigureAwait(false);
            }

        }

        protected override async Task UpdateAsync()
        {
            var beforeFromAccount = _realmObjectWrap.RealmObject?.FromAccount;
            var beforeToAccount = _realmObjectWrap.RealmObject?.FromAccount;
            var beforeDate = _realmObjectWrap.RealmObject?.Date;
            var beforeSum = _realmObjectWrap.RealmObject?.Sum;
            // The change already occured for the domain model but wasn't yet updated in the realm object
            var afterFromAccount = (FromAccount as Account)?.RealmObject;
            var afterToAccount = (ToAccount as Account)?.RealmObject;
            var afterDate = new DateTimeOffset(Date, TimeSpan.Zero);
            var afterSum = Sum;
            await _realmObjectWrap.UpdateAsync().ConfigureAwait(false);
            if (beforeDate is null) return;
            await _updateBudgetCache.OnTransferUpdateAsync(
                beforeFromAccount,
                beforeToAccount,
                beforeDate.Value,
                beforeSum.Value,
                afterFromAccount,
                afterToAccount,
                afterDate,
                afterSum)
                .ConfigureAwait(false);
        }
    }
}
