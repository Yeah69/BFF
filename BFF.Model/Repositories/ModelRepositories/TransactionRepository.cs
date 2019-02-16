using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM.Sqlite;
using BFF.Persistence.ORM.Sqlite.Interfaces;

namespace BFF.Model.Repositories.ModelRepositories
{
    public interface ITransactionRepository : IRepositoryBase<ITransaction>
    {
    }

    internal sealed class TransactionRepository : RepositoryBase<ITransaction, ITransSql>, ITransactionRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IAccountRepositoryInternal _accountRepository;
        private readonly ICategoryBaseRepositoryInternal _categoryBaseRepository;
        private readonly IPayeeRepositoryInternal _payeeRepository;
        private readonly IFlagRepositoryInternal _flagRepository;
        private readonly Func<ITransSql> _transDtoFactory;

        public TransactionRepository(
            IProvideSqliteConnection provideConnection,
            IRxSchedulerProvider rxSchedulerProvider,
            ICrudOrm crudOrm,
            IAccountRepositoryInternal accountRepository,
            ICategoryBaseRepositoryInternal categoryBaseRepository,
            IPayeeRepositoryInternal payeeRepository,
            IFlagRepositoryInternal flagRepository,
            Func<ITransSql> transDtoFactory) : base(provideConnection, crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _accountRepository = accountRepository;
            _categoryBaseRepository = categoryBaseRepository;
            _payeeRepository = payeeRepository;
            _flagRepository = flagRepository;
            _transDtoFactory = transDtoFactory;
        }
        
        protected override Converter<ITransaction, ITransSql> ConvertToPersistence => domainTransaction =>
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

        protected override async Task<ITransaction> ConvertToDomainAsync(ITransSql persistenceModel)
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