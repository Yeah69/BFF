using System;
using System.Threading.Tasks;
using BFF.Core;
using BFF.MVVM.Models.Native;
using BFF.Persistence;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;

namespace BFF.DB.Dapper.ModelRepositories
{
    public interface IBudgetEntryRepository : IWriteOnlyRepositoryBase<IBudgetEntry>
    {
        Task<IBudgetEntry> Convert(BudgetEntryDto budgetEntry, long outflow, long balance);
    }


    public sealed class BudgetEntryRepository : WriteOnlyRepositoryBase<IBudgetEntry, BudgetEntryDto>, IBudgetEntryRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICategoryRepository _categoryRepository;

        public BudgetEntryRepository(
            IProvideConnection provideConnection,
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm,
            ICategoryRepository categoryRepository) : base(provideConnection, crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _categoryRepository = categoryRepository;
        }
        
        protected override Converter<IBudgetEntry, BudgetEntryDto> ConvertToPersistence => domainBudgetEntry => 
            new BudgetEntryDto
            {
                Id = domainBudgetEntry.Id,
                CategoryId = domainBudgetEntry.Category?.Id,
                Month = domainBudgetEntry.Month,
                Budget = domainBudgetEntry.Budget
            };

        public async Task<IBudgetEntry> Convert(BudgetEntryDto budgetEntry, long outflow, long balance)
        {
            return new BudgetEntry(
                this,
                _rxSchedulerProvider,
                budgetEntry.Id, 
                budgetEntry.Month,
                await _categoryRepository.FindAsync(budgetEntry.CategoryId ?? 0L).ConfigureAwait(false), 
                budgetEntry.Budget, 
                outflow, 
                balance);
        }
    }
}