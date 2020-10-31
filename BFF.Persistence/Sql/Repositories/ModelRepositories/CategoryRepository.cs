using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;

namespace BFF.Persistence.Sql.Repositories.ModelRepositories
{
    internal interface ISqliteCategoryRepositoryInternal : ICategoryRepository, ISqliteObservableRepositoryBaseInternal<ICategory>
    {
        void InitializeAll();
    }

    internal sealed class SqliteCategoryRepository : SqliteObservableRepositoryBase<ICategory, ICategorySql>, ISqliteCategoryRepositoryInternal
    {
        private readonly ICrudOrm<ICategorySql> _crudOrm;
        private readonly Lazy<IMergeOrm> _mergeOrm;
        private readonly Lazy<ICategoryOrm> _categoryOrm;

        public SqliteCategoryRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<ICategorySql> crudOrm,
            Lazy<IMergeOrm> mergeOrm,
            Lazy<ICategoryOrm> categoryOrm) 
            : base(rxSchedulerProvider, crudOrm, new CategoryComparer())
        {
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
                    parent?.AddCategory(subCategory);
                    subCategory.Parent = parent;
                }
            }
        }

        protected override async Task<ICategory> ConvertToDomainAsync(ICategorySql persistenceModel)
        {
            return 
                new Models.Domain.Category(
                    _crudOrm, 
                    _mergeOrm.Value,
                    this,
                    persistenceModel.Id,
                    persistenceModel.Name,
                    persistenceModel.ParentId != null ? await FindAsync((long)persistenceModel.ParentId).ConfigureAwait(false) : null);
        }

        protected override Task<IEnumerable<ICategorySql>> FindAllInnerAsync() => _categoryOrm.Value.ReadCategoriesAsync();
    }
}