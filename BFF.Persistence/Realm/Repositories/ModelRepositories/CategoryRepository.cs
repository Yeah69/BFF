using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Repositories.ModelRepositories
{
    internal interface IRealmCategoryRepositoryInternal : ICategoryRepository, IRealmObservableRepositoryBaseInternal<ICategory, ICategoryRealm>
    {
        void InitializeAll();
    }

    internal sealed class RealmCategoryRepository : RealmObservableRepositoryBase<ICategory, ICategoryRealm>, IRealmCategoryRepositoryInternal
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICrudOrm<ICategoryRealm> _crudOrm;
        private readonly Lazy<IMergeOrm> _mergeOrm;
        private readonly Lazy<ICategoryOrm> _categoryOrm;

        public RealmCategoryRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<ICategoryRealm> crudOrm,
            Lazy<IMergeOrm> mergeOrm,
            Lazy<ICategoryOrm> categoryOrm) 
            : base(rxSchedulerProvider, crudOrm, new CategoryComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
            _mergeOrm = mergeOrm;
            _categoryOrm = categoryOrm;
            InitializeAll();
        }

        public void InitializeAll()
        {
            var groupedByParent = All.GroupBy(c => c.Parent).Where(grouping => grouping.Key != null);
            foreach (var parentSubCategoryGrouping in groupedByParent)
            {
                var parent = parentSubCategoryGrouping.Key;
                foreach (var subCategory in parentSubCategoryGrouping)
                {
                    parent.AddCategory(subCategory);
                    subCategory.Parent = parent;
                }
            }
        }

        protected override async Task<ICategory> ConvertToDomainAsync(ICategoryRealm persistenceModel)
        {
            return 
                new Models.Domain.Category(
                    _crudOrm, 
                    _mergeOrm.Value,
                    this,
                    _rxSchedulerProvider,
                    persistenceModel,
                    persistenceModel.Name,
                    persistenceModel.Parent is null 
                        ? null
                        : await FindAsync(persistenceModel.Parent).ConfigureAwait(false));
        }

        protected override Task<IEnumerable<ICategoryRealm>> FindAllInnerAsync() => _categoryOrm.Value.ReadCategoriesAsync();
    }
}