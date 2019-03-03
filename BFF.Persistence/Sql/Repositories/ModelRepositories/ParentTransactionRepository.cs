using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;

namespace BFF.Persistence.Sql.Repositories.ModelRepositories
{
    public interface IParentTransactionRepository
    {
    }

    internal sealed class ParentTransactionRepository : RepositoryBase<IParentTransaction, ITransSql>, IParentTransactionRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ICrudOrm<ITransSql> _crudOrm;
        private readonly Lazy<IAccountRepositoryInternal> _accountRepository;
        private readonly Lazy<IPayeeRepositoryInternal> _payeeRepository;
        private readonly Lazy<ISubTransactionRepository> _subTransactionRepository;
        private readonly Lazy<IFlagRepositoryInternal> _flagRepository;

        public ParentTransactionRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm<ITransSql> crudOrm,
            Lazy<IAccountRepositoryInternal> accountRepository,
            Lazy<IPayeeRepositoryInternal> payeeRepository,
            Lazy<ISubTransactionRepository> subTransactionRepository,
            Lazy<IFlagRepositoryInternal> flagRepository) : base(crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _crudOrm = crudOrm;
            _accountRepository = accountRepository;
            _payeeRepository = payeeRepository;
            _subTransactionRepository = subTransactionRepository;
            _flagRepository = flagRepository;
        }

        protected override async Task<IParentTransaction> ConvertToDomainAsync(ITransSql persistenceModel)
        {
            var parentTransaction = new Models.Domain.ParentTransaction(
                _crudOrm,
                _subTransactionRepository.Value,
                _rxSchedulerProvider,
                persistenceModel.Id,
                persistenceModel.Date,
                persistenceModel.FlagId is null
                    ? null
                    : await _flagRepository.Value.FindAsync((long)persistenceModel.FlagId).ConfigureAwait(false),
                persistenceModel.CheckNumber,
                await _accountRepository.Value.FindAsync(persistenceModel.AccountId).ConfigureAwait(false),
                persistenceModel.PayeeId is null
                    ? null
                    : await _payeeRepository.Value.FindAsync((long)persistenceModel.PayeeId).ConfigureAwait(false),
                persistenceModel.Memo,
                persistenceModel.Cleared == 1L);

            foreach (var subTransaction in parentTransaction.SubTransactions)
            {
                subTransaction.Parent = parentTransaction;
            }

            return parentTransaction;
        }
    }
}