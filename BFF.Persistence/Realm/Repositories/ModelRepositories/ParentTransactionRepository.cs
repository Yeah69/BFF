using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Repositories.ModelRepositories
{
    internal sealed class RealmParentTransactionRepository : RealmRepositoryBase<IParentTransaction, Trans>
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICrudOrm<Trans> _crudOrm;
        private readonly IRealmOperations _realmOperations;
        private readonly Lazy<IRealmAccountRepositoryInternal> _accountRepository;
        private readonly Lazy<IRealmPayeeRepositoryInternal> _payeeRepository;
        private readonly Lazy<IRealmSubTransactionRepository> _subTransactionRepository;
        private readonly Lazy<IRealmFlagRepositoryInternal> _flagRepository;

        public RealmParentTransactionRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<Trans> crudOrm,
            IRealmOperations realmOperations,
            Lazy<IRealmAccountRepositoryInternal> accountRepository,
            Lazy<IRealmPayeeRepositoryInternal> payeeRepository,
            Lazy<IRealmSubTransactionRepository> subTransactionRepository,
            Lazy<IRealmFlagRepositoryInternal> flagRepository) : base(crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
            _realmOperations = realmOperations;
            _accountRepository = accountRepository;
            _payeeRepository = payeeRepository;
            _subTransactionRepository = subTransactionRepository;
            _flagRepository = flagRepository;
        }

        protected override Task<IParentTransaction> ConvertToDomainAsync(Trans persistenceModel)
        {
            return _realmOperations.RunFuncAsync(InnerAsync);

            async Task<IParentTransaction> InnerAsync(Realms.Realm _)
            {
                var parentTransaction = new Models.Domain.ParentTransaction(
                    _crudOrm,
                    _subTransactionRepository.Value,
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
                    persistenceModel.Memo,
                    persistenceModel.Cleared);

                foreach (var subTransaction in parentTransaction.SubTransactions)
                {
                    subTransaction.Parent = parentTransaction;
                }

                return parentTransaction;
            }
            
        }
    }
}