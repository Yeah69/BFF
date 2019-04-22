using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;
using BFF.Persistence.Sql.Repositories.ModelRepositories;

namespace BFF.Persistence.Sql.Models.Domain
{
    internal class IncomeCategory : Model.Models.IncomeCategory, ISqlModel
    {
        private readonly ICrudOrm<ICategorySql> _crudOrm;
        private readonly IMergeOrm _mergeOrm;
        private readonly ISqliteIncomeCategoryRepositoryInternal _repository;

        public IncomeCategory(
            ICrudOrm<ICategorySql> crudOrm,
            IMergeOrm mergeOrm,
            ISqliteIncomeCategoryRepositoryInternal repository,
            IRxSchedulerProvider rxSchedulerProvider,
            long id,
            string name, 
            int monthOffset) : base(rxSchedulerProvider, name, monthOffset)
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
        }

        protected override async Task UpdateAsync()
        {
            await _crudOrm.UpdateAsync(CreatePersistenceObject()).ConfigureAwait(false);
        }

        public override async Task MergeToAsync(IIncomeCategory category)
        {
            if (!(category is IncomeCategory)) throw new ArgumentException("Cannot merge if other part isn't from same backend", nameof(category));

            await _mergeOrm.MergeCategoryAsync(
                    CreatePersistenceObject(),
                    ((IncomeCategory)category).CreatePersistenceObject())
                .ConfigureAwait(false);
            _repository.RemoveFromObservableCollection(this);
            _repository.RemoveFromCache(this);
        }

        private ICategorySql CreatePersistenceObject()
        {
            return new Persistence.Category
            {
                Id = Id,
                Name = Name,
                ParentId = null,
                IsIncomeRelevant = true,
                MonthOffset = MonthOffset
            };
        }
    }
}
