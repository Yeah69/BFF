using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;

namespace BFF.Persistence.Sql.Repositories.ModelRepositories
{
    public interface IBudgetEntryRepository
    {
        Task<IBudgetEntry> Convert(IBudgetEntrySql budgetEntry, long outflow, long balance);
    }

    internal sealed class BudgetEntryRepository : WriteOnlyRepositoryBase<IBudgetEntry>, IBudgetEntryRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICrudOrm<IBudgetEntrySql> _crudOrm;
        private readonly Lazy<ICategoryRepositoryInternal> _categoryRepository;
        private readonly Lazy<ITransRepository> _transRepository;

        public BudgetEntryRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<IBudgetEntrySql> crudOrm,
            Lazy<ICategoryRepositoryInternal> categoryRepository,
            Lazy<ITransRepository> transRepository)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
            _categoryRepository = categoryRepository;
            _transRepository = transRepository;
        }

        public async Task<IBudgetEntry> Convert(IBudgetEntrySql persistenceModel, long outflow, long balance)
        {
            return new Models.Domain.BudgetEntry(
                _crudOrm,
                _transRepository.Value,
                _rxSchedulerProvider,
                persistenceModel.Id,
                persistenceModel.Month,
                await _categoryRepository.Value.FindAsync(persistenceModel.CategoryId ?? 0L).ConfigureAwait(false),
                persistenceModel.Budget,
                outflow,
                balance);
        }
    }
}