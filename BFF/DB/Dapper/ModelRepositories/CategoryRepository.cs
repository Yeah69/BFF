using System;
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
    
    public class CategoryRepository : RepositoryBase<Domain.Category, Persistance.Category>
    {
        public CategoryRepository(IProvideConnection provideConnection) : base(provideConnection) { }
        
        protected override Converter<Domain.Category, Persistance.Category> ConvertToPersistance => domainCategory => 
            new Persistance.Category
            {
                Id = domainCategory.Id,
                ParentId = domainCategory.ParentId,
                Name = domainCategory.Name
            };
        
        protected override Converter<Persistance.Category, Domain.Category> ConvertToDomain => persistanceCategory =>
            new Domain.Category
            {
                Id = persistanceCategory.Id,
                ParentId = persistanceCategory.ParentId,
                Name = persistanceCategory.Name
            };
    }
}