using System;
using System.Threading.Tasks;
using BFF.Model;
using BFF.Model.Models;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Repositories.ModelRepositories
{
    internal interface IRealmBudgetEntryRepository
    {
        Task<IBudgetEntry> Convert(
            Models.Persistence.BudgetEntry? budgetEntry,
            Models.Persistence.Category category, 
            DateTime month, 
            long budget, 
            long outflow, 
            long balance,
            long aggregatedBudget,
            long aggregatedOutflow,
            long aggregatedBalance);
    }

    internal sealed class RealmBudgetEntryRepository : RealmWriteOnlyRepositoryBase<IBudgetEntry>, IRealmBudgetEntryRepository
    {
        private readonly ICrudOrm<Models.Persistence.BudgetEntry> _crudOrm;
        private readonly IRealmOperations _realmOperations;
        private readonly Lazy<IRealmCategoryRepositoryInternal> _categoryRepository;
        private readonly Lazy<IRealmTransRepository> _transRepository;
        private readonly Lazy<IBudgetOrm> _budgetOrm;
        private readonly IClearBudgetCache _clearBudgetCache;
        private readonly IUpdateBudgetCategory _updateBudgetCategory;

        public RealmBudgetEntryRepository(
            ICrudOrm<Models.Persistence.BudgetEntry> crudOrm,
            IRealmOperations realmOperations,
            Lazy<IRealmCategoryRepositoryInternal> categoryRepository,
            Lazy<IRealmTransRepository> transRepository,
            Lazy<IBudgetOrm> budgetOrm,
            IClearBudgetCache clearBudgetCache,
            IUpdateBudgetCategory updateBudgetCategory)
        {
            _crudOrm = crudOrm;
            _realmOperations = realmOperations;
            _categoryRepository = categoryRepository;
            _transRepository = transRepository;
            _budgetOrm = budgetOrm;
            _clearBudgetCache = clearBudgetCache;
            _updateBudgetCategory = updateBudgetCategory;
        }

        public Task<IBudgetEntry> Convert(
            Models.Persistence.BudgetEntry? budgetEntry, 
            Models.Persistence.Category category,
            DateTime month,
            long budget, 
            long outflow, 
            long balance,
            long aggregatedBudget,
            long aggregatedOutflow,
            long aggregatedBalance)
        {
            return _realmOperations.RunFuncAsync(InnerAsync);

            async Task<IBudgetEntry> InnerAsync(Realms.Realm _)
            {
                return new Models.Domain.BudgetEntry(
                    budgetEntry,
                    month,
                    await _categoryRepository.Value.FindAsync(category).ConfigureAwait(false),
                    budget,
                    outflow,
                    balance,
                    aggregatedBudget,
                    aggregatedOutflow,
                    aggregatedBalance,
                    _crudOrm,
                    _budgetOrm.Value,
                    _transRepository.Value,
                    _clearBudgetCache,
                    _updateBudgetCategory);
            }
        }
    }
}