using System;
using System.Drawing;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Extensions;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class Flag : Model.Models.Flag, IRealmModel<IFlagRealm>
    {
        private readonly IMergeOrm _mergeOrm;
        private readonly IRealmFlagRepositoryInternal _repository;
        private readonly RealmObjectWrap<IFlagRealm> _realmObjectWrap;

        public Flag(
            ICrudOrm<IFlagRealm> crudOrm,
            IMergeOrm mergeOrm,
            IRealmFlagRepositoryInternal repository,
            IRxSchedulerProvider rxSchedulerProvider,
            IFlagRealm realmObject,
            Color color, 
            string name) : base(rxSchedulerProvider, color, name)
        {
            _realmObjectWrap = new RealmObjectWrap<IFlagRealm>(
                realmObject,
                realm =>
                {
                    var ro = new Persistence.Flag();
                    UpdateRealmObject(ro);
                    return ro;
                },
                UpdateRealmObject,
                crudOrm);
            _mergeOrm = mergeOrm;
            _repository = repository;
            
            void UpdateRealmObject(IFlagRealm ro)
            {
                ro.Name = Name;
                ro.Color = Color.ToLong();
            }
        }

        public override bool IsInserted => _realmObjectWrap.IsInserted;

        public IFlagRealm RealmObject => _realmObjectWrap.RealmObject;

        public override async Task InsertAsync()
        {
            await _realmObjectWrap.InsertAsync().ConfigureAwait(false);
            await _repository.AddAsync(this).ConfigureAwait(false);
        }

        public override Task DeleteAsync()
        {
            return _realmObjectWrap.DeleteAsync();
        }

        protected override async Task UpdateAsync()
        {
            await _realmObjectWrap.UpdateAsync().ConfigureAwait(false);
            _repository.RemoveFromObservableCollection(this);
            _repository.RemoveFromCache(this);
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
