using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Repositories.ModelRepositories
{
    internal sealed class RealmParentTransactionRepository : RealmRepositoryBase<IParentTransaction, ITransRealm>
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICrudOrm<ITransRealm> _crudOrm;
        private readonly Lazy<IRealmAccountRepositoryInternal> _accountRepository;
        private readonly Lazy<IRealmPayeeRepositoryInternal> _payeeRepository;
        private readonly Lazy<IRealmSubTransactionRepository> _subTransactionRepository;
        private readonly Lazy<IRealmFlagRepositoryInternal> _flagRepository;

        public RealmParentTransactionRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<ITransRealm> crudOrm,
            Lazy<IRealmAccountRepositoryInternal> accountRepository,
            Lazy<IRealmPayeeRepositoryInternal> payeeRepository,
            Lazy<IRealmSubTransactionRepository> subTransactionRepository,
            Lazy<IRealmFlagRepositoryInternal> flagRepository) : base(crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
            _accountRepository = accountRepository;
            _payeeRepository = payeeRepository;
            _subTransactionRepository = subTransactionRepository;
            _flagRepository = flagRepository;
        }

        protected override async Task<IParentTransaction> ConvertToDomainAsync(ITransRealm persistenceModel)
        {
            var parentTransaction = new Models.Domain.ParentTransaction(
                _crudOrm,
                _subTransactionRepository.Value,
                _rxSchedulerProvider,
                persistenceModel,
                true,
                persistenceModel.Date,
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