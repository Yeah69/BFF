using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Models.Structure;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM;
using BFF.Persistence.ORM.Sqlite.Interfaces;
using MoreLinq;

namespace BFF.Model.Repositories.ModelRepositories
{
    internal class CategoryComparer : Comparer<ICategory>
    {
        public override int Compare(ICategory x, ICategory y)
        {
            IList<ICategory> GetParentalPathList(ICategory category)
            {
                IList<ICategory> list = new List<ICategory> {category};
                ICategory current = category;
                while(current.Parent != null)
                {
                    current = current.Parent;
                    list.Add(current);
                }

                return list.Reverse().ToList();
            }
            
            IList<ICategory> xList = GetParentalPathList(x);
            IList<ICategory> yList = GetParentalPathList(y);

            int i = 0;
            int value = 0;
            while(value == 0)
            {
                if(i >= xList.Count && i >= yList.Count) return 0;
                if(i >= xList.Count) return -1;
                if(i >= yList.Count) return 1;

                value = Comparer<string>.Default.Compare(xList[i].Name, yList[i].Name);
                i++;
            } 

            return value;
        }
    }

    public interface ICategoryRepository : IObservableRepositoryBase<ICategory>
    {
    }

    internal interface ICategoryRepositoryInternal : ICategoryRepository, IMergingRepository<ICategory>, IReadOnlyRepository<ICategory>
    {
    }

    internal sealed class CategoryRepository : ObservableRepositoryBase<ICategory, ICategorySql>, ICategoryRepositoryInternal
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IMergeOrm _mergeOrm;
        private readonly ICategoryOrm _categoryOrm;

        public CategoryRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<ICategorySql> crudOrm, 
            IMergeOrm mergeOrm,
            ICategoryOrm categoryOrm) 
            : base(rxSchedulerProvider, crudOrm, new CategoryComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _mergeOrm = mergeOrm;
            _categoryOrm = categoryOrm;
            InitializeAll();
        }
        
        public override async Task DeleteAsync(ICategory dataModel)
        {
            await base.DeleteAsync(dataModel).ConfigureAwait(false);
            dataModel.Parent?.RemoveCategory(dataModel);
        }

        private void InitializeAll()
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

        protected override async Task<ICategory> ConvertToDomainAsync(ICategorySql persistenceModel)
        {
            return 
                new Category<ICategorySql>(
                    persistenceModel,
                    this,
                    this,
                    _rxSchedulerProvider,
                    persistenceModel.Id > 0,
                    persistenceModel.Name,
                    persistenceModel.ParentId != null ? await FindAsync((long)persistenceModel.ParentId).ConfigureAwait(false) : null);
        }

        protected override Task<IEnumerable<ICategorySql>> FindAllInnerAsync() => _categoryOrm.ReadCategoriesAsync();
        
        public async Task MergeAsync(ICategory from, ICategory to)
        {
            var fromPersistenceModel = (@from as IDataModelInternal<ICategorySql>)?.BackingPersistenceModel;
            var toPersistenceModel = (to as IDataModelInternal<ICategorySql>)?.BackingPersistenceModel;
            if (fromPersistenceModel is null || toPersistenceModel is null) return;
            from
                .Categories
                .Join(to.Categories, c => c.Name, c => c.Name, (f, t) => f)
                .ForEach(f =>
                {
                    int i = -1;
                    // ReSharper disable once EmptyEmbeddedStatement => i gets iterated in the condition expression
                    while (to.Categories.Any(t => t.Name == $"{f.Name}{++i}")) ;
                    f.Name = $"{f.Name}{i}";
                });
            await _mergeOrm.MergeCategoryAsync(
                fromPersistenceModel,
                toPersistenceModel)
                .ConfigureAwait(false);
            ClearCache();
            await ResetAll();
            InitializeAll();
        }
    }
}