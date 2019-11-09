using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Repositories.ModelRepositories
{
    internal sealed class RealmTransactionRepository : RealmRepositoryBase<ITransaction, Trans>
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICrudOrm<Trans> _crudOrm;
        private readonly IUpdateBudgetCache _updateBudgetCache;
        private readonly IRealmOperations _realmOperations;
        private readonly Lazy<IRealmAccountRepositoryInternal> _accountRepository;
        private readonly Lazy<IRealmCategoryBaseRepositoryInternal> _categoryBaseRepository;
        private readonly Lazy<IRealmPayeeRepositoryInternal> _payeeRepository;
        private readonly Lazy<IRealmFlagRepositoryInternal> _flagRepository;

        public RealmTransactionRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<Trans> crudOrm,
            IUpdateBudgetCache updateBudgetCache,
            IRealmOperations realmOperations,
            Lazy<IRealmAccountRepositoryInternal> accountRepository,
            Lazy<IRealmCategoryBaseRepositoryInternal> categoryBaseRepository,
            Lazy<IRealmPayeeRepositoryInternal> payeeRepository,
            Lazy<IRealmFlagRepositoryInternal> flagRepository) : base(crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
            _updateBudgetCache = updateBudgetCache;
            _realmOperations = realmOperations;
            _accountRepository = accountRepository;
            _categoryBaseRepository = categoryBaseRepository;
            _payeeRepository = payeeRepository;
            _flagRepository = flagRepository;
        }

        protected override Task<ITransaction> ConvertToDomainAsync(Trans persistenceModel)
        {
            return _realmOperations.RunFuncAsync(InnerAsync);


            async Task<ITransaction> InnerAsync(Realms.Realm _)
            {
                return new Models.Domain.Transaction(
                    _crudOrm,
                    _updateBudgetCache,
                    _rxSchedulerProvider,
                    persistenceModel,
                    persistenceModel.Date.UtcDateTime,
                    persistenceModel.Flag is null
                        ? null
                        : await _flagRepository.Value.FindAsync(persistenceModel.Flag).ConfigureAwait(false),
                    persistenceModel.CheckNumber,
                    await _accountRepository.Value.FindAsync(persistenceModel.Account).ConfigureAwait(false),
                    persistenceModel.Payee is null
                        ? null
                        : await _payeeRepository.Value.FindAsync(persistenceModel.Payee).ConfigureAwait(false),
                    persistenceModel.Category is null
                        ? null
                        : await _categoryBaseRepository.Value.FindAsync(persistenceModel.Category)
                            .ConfigureAwait(false),
                    persistenceModel.Memo,
                    persistenceModel.Sum,
                    persistenceModel.Cleared);
            }
        }
    }
}