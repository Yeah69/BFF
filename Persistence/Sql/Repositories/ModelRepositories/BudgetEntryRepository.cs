using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model;
using BFF.Model.Models;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;

namespace BFF.Persistence.Sql.Repositories.ModelRepositories
{
    public interface ISqliteBudgetEntryRepository
    {
        Task<IBudgetEntry> Convert(
            IBudgetEntrySql budgetEntry, 
            long outflow, 
            long balance,
            long aggregatedBudget,
            long aggregatedOutflow,
            long aggregatedBalance);
    }

    internal sealed class SqliteBudgetEntryRepository : SqliteWriteOnlyRepositoryBase<IBudgetEntry>, ISqliteBudgetEntryRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICrudOrm<IBudgetEntrySql> _crudOrm;
        private readonly Lazy<ISqliteCategoryRepositoryInternal> _categoryRepository;
        private readonly Lazy<ISqliteTransRepository> _transRepository;
        private readonly IClearBudgetCache _clearBudgetCache;
        private readonly IUpdateBudgetCategory _updateBudgetCategory;

        public SqliteBudgetEntryRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<IBudgetEntrySql> crudOrm,
            Lazy<ISqliteCategoryRepositoryInternal> categoryRepository,
            Lazy<ISqliteTransRepository> transRepository,
            IClearBudgetCache clearBudgetCache,
            IUpdateBudgetCategory updateBudgetCategory)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
            _categoryRepository = categoryRepository;
            _transRepository = transRepository;
            _clearBudgetCache = clearBudgetCache;
            _updateBudgetCategory = updateBudgetCategory;
        }

        public async Task<IBudgetEntry> Convert(
            IBudgetEntrySql persistenceModel, 
            long outflow,
            long balance,
            long aggregatedBudget,
            long aggregatedOutflow,
            long aggregatedBalance)
        {
            return new Models.Domain.BudgetEntry(
                persistenceModel.Id,
                persistenceModel.Month,
                await _categoryRepository.Value.FindAsync(persistenceModel.CategoryId ?? 0L).ConfigureAwait(false),
                persistenceModel.Budget,
                outflow,
                balance,
                aggregatedBudget,
                aggregatedOutflow,
                aggregatedBalance,
                _crudOrm,
                _transRepository.Value,
                _clearBudgetCache,
                _updateBudgetCategory);
        }
    }
}