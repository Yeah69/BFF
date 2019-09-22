using System;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class SubTransaction : Model.Models.SubTransaction, IRealmModel<ISubTransactionRealm>
    {
        private readonly RealmObjectWrap<ISubTransactionRealm> _realmObjectWrap;

        public SubTransaction(
            ICrudOrm<ISubTransactionRealm> crudOrm,
            IRxSchedulerProvider rxSchedulerProvider,
            ISubTransactionRealm realmObject,
            ICategoryBase category,
            string memo, 
            long sum) : base(rxSchedulerProvider, category, memo, sum)
        {
            _realmObjectWrap = new RealmObjectWrap<ISubTransactionRealm>(
                realmObject,
                realm =>
                {
                    var id = realm.All<Persistence.SubTransaction>().Count();
                    var ro = new Persistence.SubTransaction{ Id = id };
                    UpdateRealmObject(ro);
                    return ro;
                },
                UpdateRealmObject,
                crudOrm);
            
            void UpdateRealmObject(ISubTransactionRealm ro)
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
                          ?? throw new ArgumentException("Model objects from different backends shouldn't be mixed");
                ro.Memo = Memo;
                ro.Sum = Sum;
            }
        }

        public override bool IsInserted => _realmObjectWrap.IsInserted;

        public ISubTransactionRealm RealmObject => _realmObjectWrap.RealmObject;

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
