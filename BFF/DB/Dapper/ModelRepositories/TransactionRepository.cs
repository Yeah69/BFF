using System;
using System.Threading.Tasks;
using BFF.Core;
using BFF.MVVM.Models.Native;
using BFF.Persistence;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;

namespace BFF.DB.Dapper.ModelRepositories
{
    public interface ITransactionRepository : IRepositoryBase<ITransaction>
    {
    }

    public sealed class TransactionRepository : RepositoryBase<ITransaction, TransDto>, ITransactionRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IAccountRepository _accountRepository;
        private readonly ICategoryBaseRepository _categoryBaseRepository;
        private readonly IPayeeRepository _payeeRepository;
        private readonly IFlagRepository _flagRepository;

        public TransactionRepository(
            IProvideConnection provideConnection,
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm,
            IAccountRepository accountRepository,
            ICategoryBaseRepository categoryBaseRepository,
            IPayeeRepository payeeRepository,
            IFlagRepository flagRepository) : base(provideConnection, crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _accountRepository = accountRepository;
            _categoryBaseRepository = categoryBaseRepository;
            _payeeRepository = payeeRepository;
            _flagRepository = flagRepository;
        }
        
        protected override Converter<ITransaction, TransDto> ConvertToPersistence => domainTransaction => 
            new TransDto
            {
                Id = domainTransaction.Id,
                AccountId = domainTransaction.Account.Id,
                FlagId = domainTransaction.Flag is null || domainTransaction.Flag == Flag.Default 
                    ? (long?) null 
                    : domainTransaction.Flag.Id,
                CheckNumber = domainTransaction.CheckNumber,
                CategoryId = domainTransaction.Category?.Id,
                PayeeId = domainTransaction.Payee?.Id,
                Date = domainTransaction.Date,
                Memo = domainTransaction.Memo,
                Sum = domainTransaction.Sum,
                Cleared = domainTransaction.Cleared ? 1L : 0L,
                Type = nameof(TransType.Transaction)
            };

        protected override async Task<ITransaction> ConvertToDomainAsync(TransDto persistenceModel)
        {
            return new Transaction(
                this,
                _rxSchedulerProvider,
                persistenceModel.Date,
                persistenceModel.Id,
                persistenceModel.FlagId is null
                    ? null
                    : await _flagRepository.FindAsync((long) persistenceModel.FlagId).ConfigureAwait(false),
                persistenceModel.CheckNumber,
                await _accountRepository.FindAsync(persistenceModel.AccountId).ConfigureAwait(false),
                persistenceModel.PayeeId is null
                    ? null
                    : await _payeeRepository.FindAsync((long) persistenceModel.PayeeId).ConfigureAwait(false),
                persistenceModel.CategoryId is null
                    ? null
                    : await _categoryBaseRepository.FindAsync((long) persistenceModel.CategoryId).ConfigureAwait(false),
                persistenceModel.Memo,
                persistenceModel.Sum,
                persistenceModel.Cleared == 1L);
        }
    }
}