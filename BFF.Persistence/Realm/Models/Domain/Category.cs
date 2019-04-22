using System;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;
using MoreLinq;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class Category : Model.Models.Category, IRealmModel<ICategoryRealm>
    {
        private readonly ICrudOrm<ICategoryRealm> _crudOrm;
        private readonly IMergeOrm _mergeOrm;
        private readonly IRealmCategoryRepositoryInternal _repository;
        private bool _isInserted;

        public Category(
            ICrudOrm<ICategoryRealm> crudOrm,
            IMergeOrm mergeOrm,
            IRealmCategoryRepositoryInternal repository,
            IRxSchedulerProvider rxSchedulerProvider,
            ICategoryRealm realmObject,
            bool isInserted,
            string name, 
            ICategory parent) : base(rxSchedulerProvider, name, parent)
        {
            RealmObject = realmObject;
            _crudOrm = crudOrm;
            _mergeOrm = mergeOrm;
            _repository = repository;
            _isInserted = isInserted;
        }

        public override bool IsInserted => _isInserted;

        public ICategoryRealm RealmObject { get; }

        public override async Task InsertAsync()
        {
            _isInserted = await _crudOrm.CreateAsync(RealmObject).ConfigureAwait(false);
        }

        public override async Task DeleteAsync()
        {
            await _crudOrm.DeleteAsync(RealmObject).ConfigureAwait(false);
            await base.DeleteAsync().ConfigureAwait(false);
            _isInserted = false;
        }

        protected override async Task UpdateAsync()
        {
            await _crudOrm.UpdateAsync(RealmObject).ConfigureAwait(false);
        }

        public override async Task MergeToAsync(ICategory category)
        {
            if (!(category is Category)) throw new ArgumentException("Cannot merge if other part isn't from same backend", nameof(category));
            
            Categories
                .Join(category.Categories, c => c.Name, c => c.Name, (f, t) => f)
                .ForEach(f =>
                {
                    int i = -1;
                    // ReSharper disable once EmptyEmbeddedStatement => i gets iterated in the condition expression
                    while (category.Categories.Any(t => t.Name == $"{f.Name}{++i}")) ;
                    f.Name = $"{f.Name}{i}";
                });
            await _mergeOrm.MergeCategoryAsync(
                RealmObject,
                ((Category) category).RealmObject)
                .ConfigureAwait(false);
            _repository.ClearCache();
            await _repository.ResetAll().ConfigureAwait(false);
            _repository.InitializeAll();
        }
    }
}
