using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Models.Structure;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;
using BFF.Persistence.Sql.Repositories.ModelRepositories;

namespace BFF.Persistence.Sql.Models.Domain
{
    internal class BudgetEntry : Model.Models.BudgetEntry, ISqlModel
    {
        private readonly ICrudOrm<IBudgetEntrySql> _crudOrm;
        private readonly ISqliteTransRepository _transRepository;

        public BudgetEntry(
            ICrudOrm<IBudgetEntrySql> crudOrm, 
            ISqliteTransRepository transRepository,
            IRxSchedulerProvider rxSchedulerProvider,
            long id,
            DateTime month,
            ICategory category, 
            long budget, 
            long outflow, 
            long balance) : base(rxSchedulerProvider, month, category, budget, outflow, balance)
        {
            Id = id;
            _crudOrm = crudOrm;
            _transRepository = transRepository;
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

        private IBudgetEntrySql CreatePersistenceObject()
        {
            if (Category != null && !(Category is Category)) throw new ArgumentException("Cannot create persistence object if parts are from another backend");

            return new Persistence.BudgetEntry
            {
                Id = Id,
                Budget = Budget,
                Month = Month,
                CategoryId = (Category as Category)?.Id
            };
        }

        public override Task<IEnumerable<ITransBase>> GetAssociatedTransAsync()
        {
            return _transRepository.GetFromMonthAndCategoryAsync(Month, Category);
        }

        public override Task<IEnumerable<ITransBase>> GetAssociatedTransIncludingSubCategoriesAsync()
        {
            return _transRepository.GetFromMonthAndCategoriesAsync(Month, Category.IterateRootBreadthFirst(c => c.Categories));
        }
    }
}
