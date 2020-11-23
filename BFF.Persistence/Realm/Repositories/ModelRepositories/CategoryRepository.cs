using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Repositories.ModelRepositories
{
    internal interface IRealmCategoryRepositoryInternal : ICategoryRepository, IRealmObservableRepositoryBaseInternal<ICategory, Models.Persistence.Category>
    {
        void InitializeAll();
    }

    internal sealed class RealmCategoryRepository : RealmObservableRepositoryBase<ICategory, Models.Persistence.Category>, IRealmCategoryRepositoryInternal
    {
        private readonly ICrudOrm<Models.Persistence.Category> _crudOrm;
        private readonly IRealmOperations _realmOperations;
        private readonly Lazy<IMergeOrm> _mergeOrm;
        private readonly Lazy<ICategoryOrm> _categoryOrm;

        public RealmCategoryRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<Models.Persistence.Category> crudOrm,
            IRealmOperations realmOperations,
            Lazy<IMergeOrm> mergeOrm,
            Lazy<ICategoryOrm> categoryOrm) 
            : base(rxSchedulerProvider, crudOrm, realmOperations, new CategoryComparer())
        {
            _crudOrm = crudOrm;
            _realmOperations = realmOperations;
            _mergeOrm = mergeOrm;
            _categoryOrm = categoryOrm;
            this.AllAsync.ContinueWith(_ => InitializeAll());
        }

        public void InitializeAll()
        {
            var groupedByParent = All.GroupBy(c => c.Parent).Where(grouping => grouping.Key is not null);
            foreach (var parentSubCategoryGrouping in groupedByParent)
            {
                var parent = parentSubCategoryGrouping.Key;
                foreach (var subCategory in parentSubCategoryGrouping)
                {
                    parent?.AddCategory(subCategory);
                    subCategory.Parent = parent;
                }
            }
        }

        protected override Task<ICategory> ConvertToDomainAsync(Models.Persistence.Category persistenceModel)
        {
            return _realmOperations.RunFuncAsync(InnerAsync);

            async Task<ICategory> InnerAsync(Realms.Realm _)
            {
                return new Models.Domain.Category(
                    persistenceModel,
                    persistenceModel.Name ?? String.Empty,
                    persistenceModel.Parent is null
                        ? null
                        : await FindAsync(persistenceModel.Parent).ConfigureAwait(false),
                    _crudOrm,
                    _mergeOrm.Value,
                    this);
            }
        }

        protected override Task<IEnumerable<Models.Persistence.Category>> FindAllInnerAsync() => _categoryOrm.Value.ReadCategoriesAsync();
    }
}