using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM.Sqlite.Interfaces;

namespace BFF.Model.Repositories.ModelRepositories
{
    public interface IBudgetEntryRepository
    {
        Task<IBudgetEntry> Convert(IBudgetEntrySql budgetEntry, long outflow, long balance);
    }


    internal sealed class BudgetEntryRepository : WriteOnlyRepositoryBase<IBudgetEntry, IBudgetEntrySql>, IBudgetEntryRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICategoryRepositoryInternal _categoryRepository;

        public BudgetEntryRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<IBudgetEntrySql> crudOrm,
            ICategoryRepositoryInternal categoryRepository)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _categoryRepository = categoryRepository;
        }

        public async Task<IBudgetEntry> Convert(IBudgetEntrySql budgetEntry, long outflow, long balance)
        {
            return new BudgetEntry<IBudgetEntrySql>(
                budgetEntry,
                this,
                _rxSchedulerProvider,
                budgetEntry.Id > 0,
                budgetEntry.Month,
                await _categoryRepository.FindAsync(budgetEntry.CategoryId ?? 0L).ConfigureAwait(false),
                budgetEntry.Budget,
                outflow,
                balance);
        }
    }
}