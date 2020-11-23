using System;
using System.Linq;
using System.Threading.Tasks;
using BFF.Model.Models;
using BFF.Model.Models.Structure;
using BFF.Persistence.Helper;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using MrMeeseeks.Extensions;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class Transaction : Model.Models.Transaction, IRealmModel<Trans>
    {
        private readonly RealmObjectWrap<Trans> _realmObjectWrap;

        public Transaction(
            // parameters
            Trans? realmObject,
            DateTime date,
            IFlag? flag,
            string checkNumber,
            IAccount? account,
            IPayee? payee, 
            ICategoryBase? category, 
            string memo, 
            long sum, 
            bool cleared,
            
            // dependencies
            ICrudOrm<Trans> crudOrm) 
            : base(
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
                ro.Date = new DateTimeOffset(Date.Year, Date.Month, Date.Day, Date.Hour, Date.Minute, Date.Second, Date.Millisecond, TimeSpan.Zero);
                ro.MonthIndex = ro.Date.ToMonthIndex();
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

        public Trans? RealmObject => _realmObjectWrap.RealmObject;

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
