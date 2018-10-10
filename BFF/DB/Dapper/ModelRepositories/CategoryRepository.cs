using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core;
using BFF.Persistence;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;
using MoreLinq;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CategoryComparer : Comparer<Domain.ICategory>
    {
        public override int Compare(Domain.ICategory x, Domain.ICategory y)
        {
            IList<Domain.ICategory> GetParentalPathList(Domain.ICategory category)
            {
                IList<Domain.ICategory> list = new List<Domain.ICategory> {category};
                Domain.ICategory current = category;
                while(current.Parent != null)
                {
                    current = current.Parent;
                    list.Add(current);
                }

                return list.Reverse().ToList();
            }
            
            IList<Domain.ICategory> xList = GetParentalPathList(x);
            IList<Domain.ICategory> yList = GetParentalPathList(y);

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

    public interface ICategoryRepository : IObservableRepositoryBase<Domain.ICategory>, IMergingRepository<Domain.ICategory>
    {
    }

    public sealed class CategoryRepository : ObservableRepositoryBase<Domain.ICategory, Category>, ICategoryRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IMergeOrm _mergeOrm;
        private readonly ICategoryOrm _categoryOrm;

        public CategoryRepository(
            IProvideConnection provideConnection,
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm, 
            IMergeOrm mergeOrm,
            ICategoryOrm categoryOrm) 
            : base(provideConnection, rxSchedulerProvider, crudOrm, new CategoryComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _mergeOrm = mergeOrm;
            _categoryOrm = categoryOrm;
            InitializeAll();
        }
        
        public override async Task DeleteAsync(Domain.ICategory dataModel)
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

        protected override async Task<Domain.ICategory> ConvertToDomainAsync(Category persistenceModel)
        {
            return 
                new Domain.Category(this,
                    _rxSchedulerProvider,
                    persistenceModel.Id,
                    persistenceModel.Name,
                    persistenceModel.ParentId != null ? await FindAsync((long)persistenceModel.ParentId).ConfigureAwait(false) : null);
        }

        protected override Task<IEnumerable<Category>> FindAllInnerAsync() => _categoryOrm.ReadCategoriesAsync();

        protected override Converter<Domain.ICategory, Category> ConvertToPersistence => domainCategory => 
            new Category
            {
                Id = domainCategory.Id,
                ParentId = domainCategory.Parent?.Id,
                Name = domainCategory.Name
            };

        public async Task MergeAsync(Domain.ICategory from, Domain.ICategory to)
        {
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
            await _mergeOrm.MergeCategoryAsync(ConvertToPersistence(from), ConvertToPersistence(to)).ConfigureAwait(false);
            ClearCache();
            await ResetAll();
            InitializeAll();
        }
    }
}