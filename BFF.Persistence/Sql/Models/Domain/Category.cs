using System;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;
using BFF.Persistence.Sql.Repositories.ModelRepositories;
using MoreLinq;

namespace BFF.Persistence.Sql.Models.Domain
{
    internal class Category : Model.Models.Category, ISqlModel
    {
        private readonly ICrudOrm<ICategorySql> _crudOrm;
        private readonly IMergeOrm _mergeOrm;
        private readonly ICategoryRepositoryInternal _repository;

        public Category(
            ICrudOrm<ICategorySql> crudOrm,
            IMergeOrm mergeOrm,
            ICategoryRepositoryInternal repository,
            IRxSchedulerProvider rxSchedulerProvider,
            long id,
            string name, 
            ICategory parent) : base(rxSchedulerProvider, name, parent)
        {
            Id = id;
            _crudOrm = crudOrm;
            _mergeOrm = mergeOrm;
            _repository = repository;
        }

        public long Id { get; private set; }

        public override bool IsInserted => Id > 0;

        public override async Task InsertAsync()
        {
            Id = await _crudOrm.CreateAsync(CreatePersistenceObject()).ConfigureAwait(false);
        }

        public override async Task DeleteAsync()
        {
            await _crudOrm.DeleteAsync(CreatePersistenceObject()).ConfigureAwait(false);
            await base.DeleteAsync().ConfigureAwait(false);
        }

        protected override async Task UpdateAsync()
        {
            await _crudOrm.UpdateAsync(CreatePersistenceObject()).ConfigureAwait(false);
        }

        public override async Task MergeToAsync(ICategory category)
        {
            if (!(category is Category)) throw new ArgumentException("Cannot merge if other part isn't from same backend", nameof(category));
            
            Categories
                .Join(category.Categories, c => c.Name, c => c.Name, (f, t) => f)
                .ForEach(f =>
                {
                    int i = -1;
                    // ReSharper disable once EmptyEmbeddedStatement => i gets iterated in the condition expression
                    while (category.Categories.Any(t => t.Name == $"{f.Name}{++i}")) ;
                    f.Name = $"{f.Name}{i}";
                });
            await _mergeOrm.MergeCategoryAsync(
                CreatePersistenceObject(),
                ((Category) category).CreatePersistenceObject())
                .ConfigureAwait(false);
            _repository.ClearCache();
            await _repository.ResetAll().ConfigureAwait(false);
            _repository.InitializeAll();
        }

        private ICategorySql CreatePersistenceObject()
        {
            if (Parent != null && !(Parent is Category)) throw new ArgumentException("Cannot create persistence object if parts are from another backend");

            return new Persistence.Category
            {
                Id = Id,
                Name = Name,
                ParentId = (Parent as Category)?.Id,
                IsIncomeRelevant = false,
                MonthOffset = 0
            };
        }
    }
}
