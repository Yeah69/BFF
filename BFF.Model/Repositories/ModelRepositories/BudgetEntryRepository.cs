using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM.Sqlite;
using BFF.Persistence.ORM.Sqlite.Interfaces;

namespace BFF.Model.Repositories.ModelRepositories
{
    public interface IBudgetEntryRepository : IWriteOnlyRepositoryBase<IBudgetEntry>
    {
        Task<IBudgetEntry> Convert(IBudgetEntrySql budgetEntry, long outflow, long balance);
    }


    internal sealed class BudgetEntryRepository : WriteOnlyRepositoryBase<IBudgetEntry, IBudgetEntrySql>, IBudgetEntryRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICategoryRepositoryInternal _categoryRepository;
        private readonly Func<IBudgetEntrySql> _budgetEntryDtoFactory;

        public BudgetEntryRepository(
            IProvideSqliteConnection provideConnection,
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm,
            ICategoryRepositoryInternal categoryRepository,
            Func<IBudgetEntrySql> budgetEntryDtoFactory) : base(provideConnection, crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _categoryRepository = categoryRepository;
            _budgetEntryDtoFactory = budgetEntryDtoFactory;
        }
        
        protected override Converter<IBudgetEntry, IBudgetEntrySql> ConvertToPersistence => domainBudgetEntry =>
        {
            var budgetEntryDto = _budgetEntryDtoFactory();
            budgetEntryDto.Id = domainBudgetEntry.Id;
            budgetEntryDto.CategoryId = domainBudgetEntry.Category?.Id;
            budgetEntryDto.Month = domainBudgetEntry.Month;
            budgetEntryDto.Budget = domainBudgetEntry.Budget;
            return budgetEntryDto;
        };

        public async Task<IBudgetEntry> Convert(IBudgetEntrySql budgetEntry, long outflow, long balance)
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