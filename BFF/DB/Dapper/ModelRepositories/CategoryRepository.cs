using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using BFF.DB.PersistenceModels;
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

    public interface ICategoryRepository : IObservableRepositoryBase<Domain.ICategory>
    {
    }

    public sealed class CategoryRepository : ObservableRepositoryBase<Domain.ICategory, Category>, ICategoryRepository
    {
        private readonly ICategoryOrm _categoryOrm;

        public CategoryRepository(IProvideConnection provideConnection, ICrudOrm crudOrm, ICategoryOrm categoryOrm) 
            : base(provideConnection, crudOrm, new CategoryComparer())
        {
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

        protected override IEnumerable<Category> FindAllInner(DbConnection connection)
        {
            return _categoryOrm.ReadCategories();
        }

        protected override Converter<Domain.ICategory, Category> ConvertToPersistence => domainCategory => 
            new Category
            {
                Id = domainCategory.Id,
                ParentId = domainCategory.Parent?.Id,
                Name = domainCategory.Name
            };
        
        protected override Converter<(Category, DbConnection), Domain.ICategory> ConvertToDomain => tuple =>
        {
            (Category persistenceCategory, DbConnection connection) = tuple;
            return new Domain.Category(this,
                persistenceCategory.Id,
                persistenceCategory.Name,
                persistenceCategory.ParentId != null ? Find((long)persistenceCategory.ParentId, connection) : null);
        };
    }
}