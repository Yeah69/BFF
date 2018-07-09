using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.DB.PersistenceModels;
using BFF.Helper;
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
            : base(provideConnection, crudOrm, new CategoryComparer())
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _mergeOrm = mergeOrm;
            _categoryOrm = categoryOrm;
            var groupedByParent = All.GroupBy(c => c.Parent).Where(grouping => grouping.Key != null);
            foreach (var parentSubCategoryGrouping in groupedByParent)
            {
                var parent = parentSubCategoryGrouping.Key;
                foreach (var subCategory in parentSubCategoryGrouping)
                {
                    parent.AddCategory(subCategory);
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
            await _mergeOrm.MergeCategoryAsync(ConvertToPersistence(from), ConvertToPersistence(to)).ConfigureAwait(false);
            ResortChildren();
            RemoveFromObservableCollection(from);
            RemoveFromCache(from);

            void ResortChildren()
            {
                foreach (var child in from.Categories)
                {
                    var firstOrDefault = to.Categories.Reverse()
                        .FirstOrDefault(c => Comparer<string>.Default.Compare(child.Name, c.Name) > 1);
                    int insertIndex = 0;
                    if (firstOrDefault != null && All.Contains(firstOrDefault))
                        insertIndex = All.IndexOf(firstOrDefault) + 1;
                    else if (All.Contains(to))
                        insertIndex = All.IndexOf(to) + 1;

                    RemoveFromObservableCollection(child);
                    All.Insert(insertIndex++, child);
                    
                    // Resort subcategories of the child
                    Stack<Domain.ICategory> treeIteration = new Stack<Domain.ICategory>(child.Categories.Reverse());
                    while (treeIteration.Count > 0)
                    {
                        var iterated = treeIteration.Pop();
                        RemoveFromObservableCollection(iterated);
                        All.Insert(insertIndex++, iterated);
                        iterated.Categories.Reverse().ForEach(c => treeIteration.Push(c));
                    }

                    // Rename if new parent has subcategory with same name
                    if (to.Categories.Any(c => c.Name == child.Name))
                    {
                        int i = 1;
                        while (to.Categories.Any(c => c.Name == $"{child.Name}{i}"))
                            i++;
                        child.Name = $"{child.Name}{i}";
                    }

                    // Set new relations
                    child.Parent = to;
                    to.AddCategory(child);
                }
            }
        }
    }
}