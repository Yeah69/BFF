using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class IncomeCategory : Model.Models.IncomeCategory, IRealmModel<ICategoryRealm>
    {
        private readonly ICrudOrm<ICategoryRealm> _crudOrm;
        private readonly IMergeOrm _mergeOrm;
        private readonly IRealmIncomeCategoryRepositoryInternal _repository;
        private bool _isInserted;

        public IncomeCategory(
            ICrudOrm<ICategoryRealm> crudOrm,
            IMergeOrm mergeOrm,
            IRealmIncomeCategoryRepositoryInternal repository,
            IRxSchedulerProvider rxSchedulerProvider,
            ICategoryRealm realmObject,
            bool isInserted,
            string name, 
            int monthOffset) : base(rxSchedulerProvider, name, monthOffset)
        {
            RealmObject = realmObject;
            _crudOrm = crudOrm;
            _mergeOrm = mergeOrm;
            _repository = repository;
            _isInserted = isInserted;
        }

        public ICategoryRealm RealmObject { get; }

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

        public override async Task MergeToAsync(IIncomeCategory category)
        {
            if (!(category is IncomeCategory)) throw new ArgumentException("Cannot merge if other part isn't from same backend", nameof(category));

            await _mergeOrm.MergeCategoryAsync(
                    RealmObject,
                    ((IncomeCategory)category).RealmObject)
                .ConfigureAwait(false);
            _repository.RemoveFromObservableCollection(this);
            _repository.RemoveFromCache(this);
        }
    }
}
