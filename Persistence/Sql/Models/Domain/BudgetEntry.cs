using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Model;
using BFF.Model.Models;
using BFF.Model.Models.Structure;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;
using BFF.Persistence.Sql.Repositories.ModelRepositories;
using MrMeeseeks.Extensions;

namespace BFF.Persistence.Sql.Models.Domain
{
    internal class BudgetEntry : Model.Models.BudgetEntry, ISqlModel
    {
        private readonly ICrudOrm<IBudgetEntrySql> _crudOrm;
        private readonly ISqliteTransRepository _transRepository;

        public BudgetEntry(
            long id,
            DateTime month,
            ICategory category, 
            long budget, 
            long outflow, 
            long balance,
            long aggregatedBudget,
            long aggregatedOutflow,
            long aggregatedBalance,
            ICrudOrm<IBudgetEntrySql> crudOrm, 
            ISqliteTransRepository transRepository,
            IClearBudgetCache clearBudgetCache,
            IUpdateBudgetCategory updateBudgetCategory) 
            : base(
                month, 
                category, 
                budget, 
                outflow, 
                balance,
                aggregatedBudget,
                aggregatedOutflow, 
                aggregatedBalance, 
                clearBudgetCache,
                updateBudgetCategory)
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
            if (Category is not null && !(Category is Category)) throw new ArgumentException("Cannot create persistence object if parts are from another backend");

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
            return _transRepository.GetFromMonthAndCategoriesAsync(Month, Category.IterateTreeBreadthFirst(c => c.Categories));
        }

        protected override Task<long> AverageBudgetOfLastMonths(int monthCount)
        {
            throw new NotSupportedException("Not supported yet or ever!");
        }

        protected override Task<long> AverageOutflowOfLastMonths(int monthCount)
        {
            throw new NotSupportedException("Not supported yet or ever!");
        }
    }
}
