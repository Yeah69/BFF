using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Core.Persistence;
using BFF.Model.Models;
using BFF.Persistence.Models;
using BFF.Persistence.ORM;
using BFF.Persistence.ORM.Interfaces;

namespace BFF.Model.Repositories.ModelRepositories
{
    public interface IBudgetEntryRepository : IWriteOnlyRepositoryBase<IBudgetEntry>
    {
        Task<IBudgetEntry> Convert(IBudgetEntryDto budgetEntry, long outflow, long balance);
    }


    internal sealed class BudgetEntryRepository : WriteOnlyRepositoryBase<IBudgetEntry, IBudgetEntryDto>, IBudgetEntryRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICategoryRepository _categoryRepository;
        private readonly Func<IBudgetEntryDto> _budgetEntryDtoFactory;

        public BudgetEntryRepository(
            IProvideConnection provideConnection,
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm,
            ICategoryRepository categoryRepository,
            Func<IBudgetEntryDto> budgetEntryDtoFactory) : base(provideConnection, crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _categoryRepository = categoryRepository;
            _budgetEntryDtoFactory = budgetEntryDtoFactory;
        }
        
        protected override Converter<IBudgetEntry, IBudgetEntryDto> ConvertToPersistence => domainBudgetEntry =>
        {
            var budgetEntryDto = _budgetEntryDtoFactory();
            budgetEntryDto.Id = domainBudgetEntry.Id;
            budgetEntryDto.CategoryId = domainBudgetEntry.Category?.Id;
            budgetEntryDto.Month = domainBudgetEntry.Month;
            budgetEntryDto.Budget = domainBudgetEntry.Budget;
            return budgetEntryDto;
        };

        public async Task<IBudgetEntry> Convert(IBudgetEntryDto budgetEntry, long outflow, long balance)
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