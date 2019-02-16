using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM.Sqlite;
using BFF.Persistence.ORM.Sqlite.Interfaces;

namespace BFF.Model.Repositories.ModelRepositories
{
    public interface IParentTransactionRepository : IRepositoryBase<IParentTransaction>
    {
    }

    internal sealed class ParentTransactionRepository : RepositoryBase<IParentTransaction, ITransSql>, IParentTransactionRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IAccountRepositoryInternal _accountRepository;
        private readonly IPayeeRepositoryInternal _payeeRepository;
        private readonly ISubTransactionRepository _subTransactionRepository;
        private readonly IFlagRepositoryInternal _flagRepository;
        private readonly Func<ITransSql> _transDtoFactory;

        public ParentTransactionRepository(
            IProvideSqliteConnection provideConnection,
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm,
            IAccountRepositoryInternal accountRepository,
            IPayeeRepositoryInternal payeeRepository,
            ISubTransactionRepository subTransactionRepository,
            IFlagRepositoryInternal flagRepository,
            Func<ITransSql> transDtoFactory) : base(provideConnection, crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _accountRepository = accountRepository;
            _payeeRepository = payeeRepository;
            _subTransactionRepository = subTransactionRepository;
            _flagRepository = flagRepository;
            _transDtoFactory = transDtoFactory;
        }
        
        protected override Converter<IParentTransaction, ITransSql> ConvertToPersistence => domainParentTransaction =>
        {
            var transDto = _transDtoFactory();

            transDto.Id = domainParentTransaction.Id;
            transDto.AccountId = domainParentTransaction.Account.Id;
            transDto.CategoryId = -69;
            transDto.FlagId = domainParentTransaction.Flag is null || domainParentTransaction.Flag == Flag.Default
                ? (long?)null
                : domainParentTransaction.Flag.Id;
            transDto.CheckNumber = domainParentTransaction.CheckNumber;
            transDto.PayeeId = domainParentTransaction.Payee?.Id;
            transDto.Date = domainParentTransaction.Date;
            transDto.Memo = domainParentTransaction.Memo;
            transDto.Cleared = domainParentTransaction.Cleared ? 1L : 0L;
            transDto.Sum = -69;
            transDto.Type = nameof(TransType.ParentTransaction);

            return transDto;
        };

        protected override async Task<IParentTransaction> ConvertToDomainAsync(ITransSql persistenceModel)
        {
            var parentTransaction = new ParentTransaction(
                this,
                _rxSchedulerProvider,
                await _subTransactionRepository.GetChildrenOfAsync(persistenceModel.Id).ConfigureAwait(false),
                persistenceModel.Date,
                persistenceModel.Id,
                persistenceModel.FlagId is null
                    ? null
                    : await _flagRepository.FindAsync((long)persistenceModel.FlagId).ConfigureAwait(false),
                persistenceModel.CheckNumber,
                await _accountRepository.FindAsync(persistenceModel.AccountId).ConfigureAwait(false),
                persistenceModel.PayeeId is null
                    ? null
                    : await _payeeRepository.FindAsync((long)persistenceModel.PayeeId).ConfigureAwait(false),
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