using System;
using System.Threading.Tasks;
using BFF.DB.PersistenceModels;
using BFF.Helper;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public interface IBudgetEntryRepository : IWriteOnlyRepositoryBase<Domain.IBudgetEntry>
    {
        Task<Domain.IBudgetEntry> Convert(BudgetEntry budgetEntry, long outflow, long balance);
    }


    public sealed class BudgetEntryRepository : WriteOnlyRepositoryBase<Domain.IBudgetEntry, BudgetEntry>, IBudgetEntryRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICategoryRepository _categoryRepository;
        private readonly INotifyBudgetOverviewRelevantChange _notifyBudgetOverviewRelevantChange;

        public BudgetEntryRepository(
            IProvideConnection provideConnection,
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm,
            ICategoryRepository categoryRepository,
            INotifyBudgetOverviewRelevantChange notifyBudgetOverviewRelevantChange) : base(provideConnection, crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _categoryRepository = categoryRepository;
            _notifyBudgetOverviewRelevantChange = notifyBudgetOverviewRelevantChange;
        }
        
        protected override Converter<Domain.IBudgetEntry, BudgetEntry> ConvertToPersistence => domainBudgetEntry => 
            new BudgetEntry
            {
                Id = domainBudgetEntry.Id,
                CategoryId = domainBudgetEntry.Category?.Id,
                Month = domainBudgetEntry.Month,
                Budget = domainBudgetEntry.Budget
            };

        public async Task<Domain.IBudgetEntry> Convert(BudgetEntry budgetEntry, long outflow, long balance)
        {
            return new Domain.BudgetEntry(
                this,
                _notifyBudgetOverviewRelevantChange,
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