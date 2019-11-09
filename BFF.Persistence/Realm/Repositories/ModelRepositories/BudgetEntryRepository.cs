using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Realm.ORM;
using BFF.Persistence.Realm.ORM.Interfaces;
using JetBrains.Annotations;

namespace BFF.Persistence.Realm.Repositories.ModelRepositories
{
    internal interface IRealmBudgetEntryRepository
    {
        Task<IBudgetEntry> Convert([CanBeNull] Models.Persistence.BudgetEntry budgetEntry, Models.Persistence.Category category, DateTime month, long budget, long outflow, long balance);
    }

    internal sealed class RealmBudgetEntryRepository : RealmWriteOnlyRepositoryBase<IBudgetEntry>, IRealmBudgetEntryRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICrudOrm<Models.Persistence.BudgetEntry> _crudOrm;
        private readonly IUpdateBudgetCache _updateBudgetCache;
        private readonly IRealmOperations _realmOperations;
        private readonly Lazy<IRealmCategoryRepositoryInternal> _categoryRepository;
        private readonly Lazy<IRealmTransRepository> _transRepository;

        public RealmBudgetEntryRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<Models.Persistence.BudgetEntry> crudOrm,
            IUpdateBudgetCache updateBudgetCache,
            IRealmOperations realmOperations,
            Lazy<IRealmCategoryRepositoryInternal> categoryRepository,
            Lazy<IRealmTransRepository> transRepository)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
            _updateBudgetCache = updateBudgetCache;
            _realmOperations = realmOperations;
            _categoryRepository = categoryRepository;
            _transRepository = transRepository;
        }

        public Task<IBudgetEntry> Convert(Models.Persistence.BudgetEntry budgetEntry, Models.Persistence.Category category, DateTime month, long budget, long outflow, long balance)
        {
            return _realmOperations.RunFuncAsync(InnerAsync);

            async Task<IBudgetEntry> InnerAsync(Realms.Realm _)
            {
                return new Models.Domain.BudgetEntry(
                    _crudOrm,
                    _updateBudgetCache,
                    _transRepository.Value,
                    _rxSchedulerProvider,
                    budgetEntry,
                    month,
                    await _categoryRepository.Value.FindAsync(category).ConfigureAwait(false),
                    budget,
                    outflow,
                    balance);
            }
        }
    }
}