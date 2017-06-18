using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    
    public sealed class CategoryRepository : CachingRepositoryBase<Domain.Category, Persistance.Category>
    {
        public ObservableCollection<Domain.ICategory> All { get; }

        public CategoryRepository(IProvideConnection provideConnection) : base(provideConnection)
        {
            All = new ObservableCollection<Domain.ICategory>();
            
            IList<Domain.Category> allCategories = FindAll().ToList();
            var rootCategories = allCategories.Where(c => c.Parent == null).OrderBy(c => c.Name);
            var stack = new Stack<Domain.ICategory>(rootCategories.Reverse());
            while(stack.Count > 0)
            {
                Domain.ICategory current = stack.Pop();
                All.Add(current);
                var children = allCategories.Where(c => c.Parent == current);
                foreach(var child in children.Reverse())
                {
                    stack.Push(child);
                }
            }
        }

        public override Domain.Category Create() =>
            new Domain.Category(this, -1, "", null);
        
        protected override Converter<Domain.Category, Persistance.Category> ConvertToPersistance => domainCategory => 
            new Persistance.Category
            {
                Id = domainCategory.Id,
                ParentId = domainCategory.Parent.Id,
                Name = domainCategory.Name
            };
        
        protected override Converter<Persistance.Category, Domain.Category> ConvertToDomain => persistanceCategory =>
            new Domain.Category(this,
                                persistanceCategory.Id,
                                persistanceCategory.Name,
                                persistanceCategory.ParentId != null ? Find((long)persistanceCategory.ParentId) : null);
    }
}