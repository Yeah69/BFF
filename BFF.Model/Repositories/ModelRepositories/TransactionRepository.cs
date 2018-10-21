using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Core.Persistence;
using BFF.Model.Models;
using BFF.Persistence.Models;
using BFF.Persistence.ORM;
using BFF.Persistence.ORM.Interfaces;

namespace BFF.Model.Repositories.ModelRepositories
{
    public interface ITransactionRepository : IRepositoryBase<ITransaction>
    {
    }

    internal sealed class TransactionRepository : RepositoryBase<ITransaction, ITransDto>, ITransactionRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IAccountRepository _accountRepository;
        private readonly ICategoryBaseRepository _categoryBaseRepository;
        private readonly IPayeeRepository _payeeRepository;
        private readonly IFlagRepository _flagRepository;
        private readonly Func<ITransDto> _transDtoFactory;

        public TransactionRepository(
            IProvideConnection provideConnection,
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm,
            IAccountRepository accountRepository,
            ICategoryBaseRepository categoryBaseRepository,
            IPayeeRepository payeeRepository,
            IFlagRepository flagRepository,
            Func<ITransDto> transDtoFactory) : base(provideConnection, crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _accountRepository = accountRepository;
            _categoryBaseRepository = categoryBaseRepository;
            _payeeRepository = payeeRepository;
            _flagRepository = flagRepository;
            _transDtoFactory = transDtoFactory;
        }
        
        protected override Converter<ITransaction, ITransDto> ConvertToPersistence => domainTransaction =>
        {
            var transDto = _transDtoFactory();

            transDto.Id = domainTransaction.Id;
            transDto.AccountId = domainTransaction.Account.Id;
            transDto.FlagId = domainTransaction.Flag is null || domainTransaction.Flag == Flag.Default
                ? (long?)null
                : domainTransaction.Flag.Id;
            transDto.CheckNumber = domainTransaction.CheckNumber;
            transDto.CategoryId = domainTransaction.Category?.Id;
            transDto.PayeeId = domainTransaction.Payee?.Id;
            transDto.Date = domainTransaction.Date;
            transDto.Memo = domainTransaction.Memo;
            transDto.Sum = domainTransaction.Sum;
            transDto.Cleared = domainTransaction.Cleared ? 1L : 0L;
            transDto.Type = nameof(TransType.Transaction);

            return transDto;
        };

        protected override async Task<ITransaction> ConvertToDomainAsync(ITransDto persistenceModel)
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