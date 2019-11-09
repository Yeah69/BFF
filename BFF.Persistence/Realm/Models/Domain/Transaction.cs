using System;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Models.Structure;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class Transaction : Model.Models.Transaction, IRealmModel<Trans>
    {
        private readonly IUpdateBudgetCache _updateBudgetCache;
        private readonly RealmObjectWrap<Trans> _realmObjectWrap;

        public Transaction(
            ICrudOrm<Trans> crudOrm,
            IUpdateBudgetCache updateBudgetCache,
            IRxSchedulerProvider rxSchedulerProvider,
            Trans realmObject,
            DateTime date,
            IFlag flag,
            string checkNumber,
            IAccount account,
            IPayee payee, 
            ICategoryBase category, 
            string memo, 
            long sum, 
            bool cleared) 
            : base(
                rxSchedulerProvider,
                date,
                flag,
                checkNumber,
                account,
                payee,
                category,
                memo,
                sum,
                cleared)
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
                ro.Account = 
                    Account is null
                        ? null
                        : (Account as Account)?.RealmObject 
                          ?? throw new ArgumentException("Model objects from different backends shouldn't be mixed");
                ro.Date = new DateTimeOffset(Date, TimeSpan.Zero);
                ro.Payee =
                    Payee is null
                        ? null
                        : (Payee as Payee)?.RealmObject
                          ?? throw new ArgumentException("Model objects from different backends shouldn't be mixed");
                ro.Category =
                    Category is null
                        ? null
                        : (Category as Category)?.RealmObject
                          ?? (Category as IncomeCategory)?.RealmObject
                          ?? throw new ArgumentException("Model objects from different backends shouldn't be mixed");
                ro.CheckNumber = CheckNumber;
                ro.Flag =
                    Flag is null
                        ? null
                        : (Flag as Flag)?.RealmObject
                          ?? throw new ArgumentException("Model objects from different backends shouldn't be mixed");
                ro.Memo = Memo;
                ro.Sum = Sum;
                ro.Cleared = Cleared;
                ro.TypeIndex = (int) TransType.Transaction;

                ro.FromAccount = null;
                ro.ToAccount = null;
            }
        }

        public override bool IsInserted => _realmObjectWrap.IsInserted;

        public Trans RealmObject => _realmObjectWrap.RealmObject;

        public override async Task InsertAsync()
        {
            await _realmObjectWrap.InsertAsync().ConfigureAwait(false);
            if (_realmObjectWrap.RealmObject != null)
                await _updateBudgetCache.OnTransactionInsertOrDeleteAsync(
                    _realmObjectWrap.RealmObject.Category,
                    _realmObjectWrap.RealmObject.Date).ConfigureAwait(false);
        }

        public override async Task DeleteAsync()
        {
            var tempCategory = _realmObjectWrap.RealmObject?.Category;
            var tempDate = _realmObjectWrap.RealmObject?.Date;
            await _realmObjectWrap.DeleteAsync().ConfigureAwait(false);
            if (tempCategory != null)
                await _updateBudgetCache.OnTransactionInsertOrDeleteAsync(
                    tempCategory, 
                    tempDate.Value).ConfigureAwait(false);

        }

        protected override async Task UpdateAsync()
        {
            var beforeCategory = _realmObjectWrap.RealmObject?.Category;
            var beforeDate = _realmObjectWrap.RealmObject?.Date;
            var beforeSum = _realmObjectWrap.RealmObject?.Sum;
            // The change already occured for the domain model but wasn't yet updated in the realm object
            var afterCategory = (Category as Category)?.RealmObject;
            var afterDate = new DateTimeOffset(Date, TimeSpan.Zero);
            var afterSum = Sum;
            await _realmObjectWrap.UpdateAsync().ConfigureAwait(false);
            if (beforeDate is null) return;
            await _updateBudgetCache 
                .OnTransactionUpdateAsync(beforeCategory, beforeDate.Value, beforeSum.Value, afterCategory, afterDate, afterSum)
                .ConfigureAwait(false);
        }
    }
}
