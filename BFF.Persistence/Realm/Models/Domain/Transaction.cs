using System;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Models.Structure;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class Transaction : Model.Models.Transaction, IRealmModel<ITransRealm>
    {
        private readonly RealmObjectWrap<ITransRealm> _realmObjectWrap;

        public Transaction(
            ICrudOrm<ITransRealm> crudOrm,
            IRxSchedulerProvider rxSchedulerProvider,
            ITransRealm realmObject,
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
            _realmObjectWrap = new RealmObjectWrap<ITransRealm>(
                realmObject,
                realm =>
                {
                    var id = realm.All<Trans>().Count();
                    var ro = new Trans{ Id = id };
                    UpdateRealmObject(ro);
                    return ro;
                },
                UpdateRealmObject,
                crudOrm);
            
            void UpdateRealmObject(ITransRealm ro)
            {
                ro.Account = 
                    Account is null
                        ? null
                        : (Account as Account)?.RealmObject 
                          ?? throw new ArgumentException("Model objects from different backends shouldn't be mixed");
                ro.Date = Date;
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
                ro.Type = TransType.Transaction;

                ro.FromAccount = null;
                ro.ToAccount = null;
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
