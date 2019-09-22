using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class Payee : Model.Models.Payee, IRealmModel<IPayeeRealm>
    {
        private readonly IMergeOrm _mergeOrm;
        private readonly IRealmPayeeRepositoryInternal _repository;
        private readonly RealmObjectWrap<IPayeeRealm> _realmObjectWrap;

        public Payee(
            ICrudOrm<IPayeeRealm> crudOrm,
            IMergeOrm mergeOrm,
            IRealmPayeeRepositoryInternal repository,
            IRxSchedulerProvider rxSchedulerProvider,
            IPayeeRealm realmObject,
            string name) : base(rxSchedulerProvider, name)
        {
            _realmObjectWrap = new RealmObjectWrap<IPayeeRealm>(
                realmObject,
                realm =>
                {
                    var ro = new Persistence.Payee();
                    UpdateRealmObject(ro);
                    return ro;
                },
                UpdateRealmObject,
                crudOrm);
            
            _mergeOrm = mergeOrm;
            _repository = repository;
            
            void UpdateRealmObject(IPayeeRealm ro)
            {
                ro.Name = Name;
            }
        }

        public override bool IsInserted => _realmObjectWrap.IsInserted;

        public IPayeeRealm RealmObject => _realmObjectWrap.RealmObject;

        public override async Task InsertAsync()
        {
            await _realmObjectWrap.InsertAsync().ConfigureAwait(false);
            await _repository.AddAsync(this).ConfigureAwait(false);
        }

        public override async Task DeleteAsync()
        {
            await _realmObjectWrap.DeleteAsync().ConfigureAwait(false);
            _repository.RemoveFromObservableCollection(this);
            _repository.RemoveFromCache(this);
        }

        protected override Task UpdateAsync()
        {
            return _realmObjectWrap.UpdateAsync();
        }

        public override async Task MergeToAsync(IPayee payee)
        {
            if (!(payee is Payee)) throw new ArgumentException("Cannot merge if other part isn't from same backend", nameof(payee));

            await _mergeOrm.MergePayeeAsync(
                    RealmObject,
                    ((Payee)payee).RealmObject)
                .ConfigureAwait(false);
            _repository.RemoveFromObservableCollection(this);
            _repository.RemoveFromCache(this);
        }
    }
}
