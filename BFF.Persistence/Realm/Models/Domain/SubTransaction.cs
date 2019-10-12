using System;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class SubTransaction : Model.Models.SubTransaction, IRealmModel<Persistence.SubTransaction>
    {
        private readonly RealmObjectWrap<Persistence.SubTransaction> _realmObjectWrap;

        public SubTransaction(
            ICrudOrm<Persistence.SubTransaction> crudOrm,
            IRxSchedulerProvider rxSchedulerProvider,
            Persistence.SubTransaction realmObject,
            ICategoryBase category,
            string memo, 
            long sum) : base(rxSchedulerProvider, category, memo, sum)
        {
            _realmObjectWrap = new RealmObjectWrap<Persistence.SubTransaction>(
                realmObject,
                realm =>
                {
                    var dbSetting = realm.All<Persistence.DbSetting>().First();
                    var id = dbSetting.NextSubTransactionId++;
                    realm.Add(dbSetting, true);
                    var ro = new Persistence.SubTransaction{ Id = id };
                    UpdateRealmObject(ro);
                    return ro;
                },
                UpdateRealmObject,
                crudOrm);
            
            void UpdateRealmObject(Persistence.SubTransaction ro)
            {
                ro.Parent = 
                    Parent is null
                        ? null
                        : (Parent as ParentTransaction)?.RealmObject
                          ?? throw new ArgumentException("Model objects from different backends shouldn't be mixed");
                ro.Category =
                    Category is null
                        ? null
                        : (Category as Category)?.RealmObject
                          ?? (Category as IncomeCategory)?.RealmObject
                          ?? throw new ArgumentException("Model objects from different backends shouldn't be mixed");
                ro.Memo = Memo;
                ro.Sum = Sum;
            }
        }

        public override bool IsInserted => _realmObjectWrap.IsInserted;

        public Persistence.SubTransaction RealmObject => _realmObjectWrap.RealmObject;

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
