using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Domain = BFF.MVVM.Models.Native;
using Persistance = BFF.DB.PersistanceModels;

namespace BFF.DB.Dapper.ModelRepositories
{
    public class CreateCategoryTable : CreateTableBase
    {
        public CreateCategoryTable(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override string CreateTableStatement =>
            $@"CREATE TABLE [{nameof(Persistance.Category)}s](
            {nameof(Persistance.Category.Id)} INTEGER PRIMARY KEY,
            {nameof(Persistance.Category.ParentId)} INTEGER,
            {nameof(Persistance.Category.Name)} VARCHAR(100),
            FOREIGN KEY({nameof(Persistance.Category.ParentId)}) REFERENCES {nameof(Persistance.Category)}s({nameof(Persistance.Category.Id)}) ON DELETE SET NULL);";
    }
    
    public class CategoryComparer : Comparer<Domain.ICategory>
    {
        public override int Compare(Domain.ICategory x, Domain.ICategory y)
        {
            IList<Domain.ICategory> getParentalPathList(Domain.ICategory category)
            {
                IList<Domain.ICategory> list = new List<Domain.ICategory>{category};
                Domain.ICategory current = category;
                while(current.Parent != null)
                {
                    current = current.Parent;
                    list.Add(current);
                }

                return list.Reverse().ToList();
            }
            
            IList<Domain.ICategory> xList = getParentalPathList(x);
            IList<Domain.ICategory> yList = getParentalPathList(y);

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
    
    public sealed class CategoryRepository : ObservableRepositoryBase<Domain.ICategory, Persistance.Category>
    {
        public CategoryRepository(IProvideConnection provideConnection) 
            : base(provideConnection, new CategoryComparer())
        {
        }

        public override Domain.ICategory Create() =>
            new Domain.Category(this, -1, "", null);
        
        protected override Converter<Domain.ICategory, Persistance.Category> ConvertToPersistance => domainCategory => 
            new Persistance.Category
            {
                Id = domainCategory.Id,
                ParentId = domainCategory.Parent?.Id,
                Name = domainCategory.Name
            };
        
        protected override Converter<(Persistance.Category, DbConnection), Domain.ICategory> ConvertToDomain => tuple =>
        {
            (Persistance.Category persistenceCategory, DbConnection connection) = tuple;
            return new Domain.Category(this,
                persistenceCategory.Id,
                persistenceCategory.Name,
                persistenceCategory.ParentId != null ? Find((long)persistenceCategory.ParentId, connection) : null);
        };
    }
}