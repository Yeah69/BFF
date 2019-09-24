using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Repositories.ModelRepositories
{
    public interface IRealmBudgetEntryRepository
    {
        Task<IBudgetEntry> Convert(IBudgetEntryRealm budgetEntry, long outflow, long balance);
    }

    internal sealed class RealmBudgetEntryRepository : RealmWriteOnlyRepositoryBase<IBudgetEntry>, IRealmBudgetEntryRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICrudOrm<IBudgetEntryRealm> _crudOrm;
        private readonly IRealmOperations _realmOperations;
        private readonly Lazy<IRealmCategoryRepositoryInternal> _categoryRepository;
        private readonly Lazy<IRealmTransRepository> _transRepository;

        public RealmBudgetEntryRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<IBudgetEntryRealm> crudOrm,
            IRealmOperations realmOperations,
            Lazy<IRealmCategoryRepositoryInternal> categoryRepository,
            Lazy<IRealmTransRepository> transRepository)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
            _realmOperations = realmOperations;
            _categoryRepository = categoryRepository;
            _transRepository = transRepository;
        }

        public Task<IBudgetEntry> Convert(IBudgetEntryRealm persistenceModel, long outflow, long balance)
        {
            return _realmOperations.RunFuncAsync(InnerAsync);

            async Task<IBudgetEntry> InnerAsync(Realms.Realm _)
            {
                return new Models.Domain.BudgetEntry(
                    _crudOrm,
                    _transRepository.Value,
                    _rxSchedulerProvider,
                    persistenceModel,
                    persistenceModel.Month,
                    await _categoryRepository.Value.FindAsync(persistenceModel.Category).ConfigureAwait(false),
                    persistenceModel.Budget,
                    outflow,
                    balance);
            }
        }
    }
}