using System;
using System.Drawing;
using System.Threading.Tasks;
using BFF.Model.Models;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;
using MrMeeseeks.Extensions;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class Flag : Model.Models.Flag, IRealmModel<Persistence.Flag>
    {
        private readonly IMergeOrm _mergeOrm;
        private readonly IRealmFlagRepositoryInternal _repository;
        private readonly RealmObjectWrap<Persistence.Flag> _realmObjectWrap;

        public Flag(
            // parameter
            Persistence.Flag? realmObject,
            Color color, 
            string name,
            
            // dependencies
            ICrudOrm<Persistence.Flag> crudOrm,
            IMergeOrm mergeOrm,
            IRealmFlagRepositoryInternal repository) : base(color, name)
        {
            _realmObjectWrap = new RealmObjectWrap<Persistence.Flag>(
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
            
            void UpdateRealmObject(Persistence.Flag ro)
            {
                ro.Name = Name;
                ro.Color = Color.ToLong();
            }
        }

        public override bool IsInserted => _realmObjectWrap.IsInserted;

        public Persistence.Flag? RealmObject => _realmObjectWrap.RealmObject;

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
                    RealmObject ?? throw new NullReferenceException("Shouldn't be null at that point"),
                    ((Flag)flag).RealmObject ?? throw new NullReferenceException("Shouldn't be null at that point"))
                .ConfigureAwait(false);
            _repository.RemoveFromObservableCollection(this);
            _repository.RemoveFromCache(this);
        }
    }
}
