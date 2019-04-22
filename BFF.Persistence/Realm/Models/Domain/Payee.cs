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
        private readonly ICrudOrm<IPayeeRealm> _crudOrm;
        private readonly IMergeOrm _mergeOrm;
        private readonly IRealmPayeeRepositoryInternal _repository;
        private bool _isInserted;

        public Payee(
            ICrudOrm<IPayeeRealm> crudOrm,
            IMergeOrm mergeOrm,
            IRealmPayeeRepositoryInternal repository,
            IRxSchedulerProvider rxSchedulerProvider,
            IPayeeRealm realmObject,
            bool isInserted,
            string name) : base(rxSchedulerProvider, name)
        {
            RealmObject = realmObject;
            _crudOrm = crudOrm;
            _mergeOrm = mergeOrm;
            _repository = repository;
            _isInserted = isInserted;
        }

        public IPayeeRealm RealmObject { get; }

        public override bool IsInserted => _isInserted;

        public override async Task InsertAsync()
        {
            _isInserted = await _crudOrm.CreateAsync(RealmObject).ConfigureAwait(false);
        }

        public override async Task DeleteAsync()
        {
            await _crudOrm.DeleteAsync(RealmObject).ConfigureAwait(false);
            _isInserted = false;
        }

        protected override async Task UpdateAsync()
        {
            await _crudOrm.UpdateAsync(RealmObject).ConfigureAwait(false);
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
