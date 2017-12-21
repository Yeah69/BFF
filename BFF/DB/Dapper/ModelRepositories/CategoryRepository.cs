using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using BFF.DB.PersistenceModels;
using Dapper;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateCategoryTable : CreateTableBase
    {
        public CreateCategoryTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(Category)}s](
            {nameof(Category.Id)} INTEGER PRIMARY KEY,
            {nameof(Category.ParentId)} INTEGER,
            {nameof(Category.Name)} VARCHAR(100),
            {nameof(Category.IsIncomeRelevant)} INTEGER,
            {nameof(Category.MonthOffset)} INTEGER,
            FOREIGN KEY({nameof(Category.ParentId)}) REFERENCES {nameof(Category)}s({nameof(Category.Id)}) ON DELETE SET NULL);";
    }

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
        private static readonly string GetAllQuery = $"SELECT * FROM {nameof(Category)}s WHERE {nameof(Category.IsIncomeRelevant)} == 0;";

        public CategoryRepository(IProvideConnection provideConnection) 
            : base(provideConnection, new CategoryComparer())
        {
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

        public override Domain.ICategory Create() =>
            new Domain.Category(this, -1, "", null);

        protected override IEnumerable<Category> FindAllInner(DbConnection connection)
        {
            return connection.Query<Category>(GetAllQuery);
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

    public class IncomeCategoryComparer : Comparer<Domain.IIncomeCategory>
    {
        public override int Compare(Domain.IIncomeCategory x, Domain.IIncomeCategory y) => StringComparer.Create(CultureInfo.InvariantCulture, false).Compare(x.Name, y.Name);
    }

    public interface IIncomeCategoryRepository : IObservableRepositoryBase<Domain.IIncomeCategory>
    {
    }

    public sealed class IncomeCategoryRepository : ObservableRepositoryBase<Domain.IIncomeCategory, Category>, IIncomeCategoryRepository
    {
        private static readonly string GetAllQuery = $"SELECT * FROM {nameof(Category)}s WHERE {nameof(Category.IsIncomeRelevant)} == 1;";

        public IncomeCategoryRepository(IProvideConnection provideConnection)
            : base(provideConnection, new IncomeCategoryComparer())
        {
        }
        
        public override Domain.IIncomeCategory Create() =>
            new Domain.IncomeCategory(this, -1, "", 0);

        protected override IEnumerable<Category> FindAllInner(DbConnection connection)
        {
            return connection.Query<Category>(GetAllQuery);
        }

        protected override Converter<Domain.IIncomeCategory, Category> ConvertToPersistence => domainCategory =>
            new Category
            {
                Id = domainCategory.Id,
                Name = domainCategory.Name,
                ParentId = null,
                IsIncomeRelevant = true,
                MonthOffset = domainCategory.MonthOffset
            };

        protected override Converter<(Category, DbConnection), Domain.IIncomeCategory> ConvertToDomain => tuple =>
        {
            (Category persistenceCategory, DbConnection _) = tuple;
            return new Domain.IncomeCategory(this,
                persistenceCategory.Id,
                persistenceCategory.Name,
                persistenceCategory.MonthOffset);
        };
    }
}