using System;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class Transfer : Model.Models.Transfer, IRealmModel<ITransRealm>
    {
        private readonly RealmObjectWrap<ITransRealm> _realmObjectWrap;

        public Transfer(
            ICrudOrm<ITransRealm> crudOrm,
            IRxSchedulerProvider rxSchedulerProvider,
            ITransRealm realmObject,
            DateTime date,
            IFlag flag, 
            string checkNumber, 
            IAccount fromAccount,
            IAccount toAccount,
            string memo, 
            long sum,
            bool cleared) : base(rxSchedulerProvider, date, flag, checkNumber, fromAccount, toAccount, memo, sum, cleared)
        {
            _realmObjectWrap = new RealmObjectWrap<ITransRealm>(
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
            
            void UpdateRealmObject(ITransRealm ro)
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
                ro.Date = Date;
                ro.CheckNumber = CheckNumber;
                ro.Flag =
                    Flag is null
                        ? null
                        : (Flag as Flag)?.RealmObject
                          ?? throw new ArgumentException("Model objects from different backends shouldn't be mixed");
                ro.Memo = Memo;
                ro.Sum = Sum;
                ro.Cleared = Cleared;
                ro.Type = TransType.Transfer;

                ro.Account = null;
                ro.Payee = null;
                ro.Category = null;
            }
        }

        public override bool IsInserted => _realmObjectWrap.IsInserted;

        public ITransRealm RealmObject => _realmObjectWrap.RealmObject;

        public override Task InsertAsync()
        {
            return _realmObjectWrap.InsertAsync();
        }

        public override Task DeleteAsync()
        {
            return _realmObjectWrap.DeleteAsync();
        }

        protected override Task UpdateAsync()
        {
            return _realmObjectWrap.UpdateAsync();
        }
    }
}
