using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM;
using BFF.Persistence.Realm.ORM.Interfaces;
using MrMeeseeks.Extensions;

namespace BFF.Persistence.Realm.Repositories.ModelRepositories
{
    internal interface IRealmSubTransactionRepository
    {
        Task<IEnumerable<ISubTransaction>> GetChildrenOfAsync(Trans parentTransaction);
    }

    internal sealed class RealmSubTransactionRepository : RealmRepositoryBase<ISubTransaction, Models.Persistence.SubTransaction>, IRealmSubTransactionRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICrudOrm<Models.Persistence.SubTransaction> _crudOrm;
        private readonly IUpdateBudgetCache _updateBudgetCache;
        private readonly IRealmOperations _realmOperations;
        private readonly Lazy<IParentalOrm> _parentalOrm;
        private readonly Lazy<IRealmCategoryBaseRepositoryInternal> _categoryBaseRepository;

        public RealmSubTransactionRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<Models.Persistence.SubTransaction> crudOrm,
            IUpdateBudgetCache updateBudgetCache,
            IRealmOperations realmOperations,
            Lazy<IParentalOrm> parentalOrm,
            Lazy<IRealmCategoryBaseRepositoryInternal> categoryBaseRepository)
            : base(crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
            _updateBudgetCache = updateBudgetCache;
            _realmOperations = realmOperations;
            _parentalOrm = parentalOrm;
            _categoryBaseRepository = categoryBaseRepository;
        }

        public async Task<IEnumerable<ISubTransaction>> GetChildrenOfAsync(Trans parentTransaction) =>
            await (await _parentalOrm.Value.ReadSubTransactionsOfAsync(parentTransaction).ConfigureAwait(false))
                .Select(async sti => await ConvertToDomainAsync(sti).ConfigureAwait(false)).ToAwaitableEnumerable().ConfigureAwait(false);

        protected override Task<ISubTransaction> ConvertToDomainAsync(Models.Persistence.SubTransaction persistenceModel)
        {
            return _realmOperations.RunFuncAsync(InnerAsync);

            async Task<ISubTransaction> InnerAsync(Realms.Realm _)
            {
                return new Models.Domain.SubTransaction(
                    _crudOrm,
                    _updateBudgetCache,
                    _rxSchedulerProvider,
                    persistenceModel,
                    persistenceModel.Category is null
                        ? null
                        : await _categoryBaseRepository.Value.FindAsync(persistenceModel.Category).ConfigureAwait(false),
                    persistenceModel.Memo,
                    persistenceModel.Sum);
            }
        }
    }
}