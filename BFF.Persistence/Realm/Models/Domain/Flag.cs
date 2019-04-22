using System;
using System.Drawing;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class Flag : Model.Models.Flag, IRealmModel<IFlagRealm>
    {
        private readonly ICrudOrm<IFlagRealm> _crudOrm;
        private readonly IMergeOrm _mergeOrm;
        private readonly IRealmFlagRepositoryInternal _repository;
        private bool _isInserted;

        public Flag(
            ICrudOrm<IFlagRealm> crudOrm,
            IMergeOrm mergeOrm,
            IRealmFlagRepositoryInternal repository,
            IRxSchedulerProvider rxSchedulerProvider,
            IFlagRealm realmObject,
            bool isInserted,
            Color color, 
            string name) : base(rxSchedulerProvider, color, name)
        {
            RealmObject = realmObject;
            _crudOrm = crudOrm;
            _mergeOrm = mergeOrm;
            _repository = repository;
            _isInserted = isInserted;
        }

        public IFlagRealm RealmObject { get; }

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

        public override async Task MergeToAsync(IFlag flag)
        {
            if (!(flag is Flag)) throw new ArgumentException("Cannot merge if other part isn't from same backend", nameof(flag));

            await _mergeOrm.MergeFlagAsync(
                    RealmObject,
                    ((Flag)flag).RealmObject)
                .ConfigureAwait(false);
            _repository.RemoveFromObservableCollection(this);
            _repository.RemoveFromCache(this);
        }
    }
}
