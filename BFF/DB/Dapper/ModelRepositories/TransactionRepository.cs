using System;
using System.Data.Common;
using BFF.DB.PersistenceModels;
using BFF.MVVM.Models.Native.Structure;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public interface ITransactionRepository : IRepositoryBase<Domain.ITransaction>
    {
    }

    public sealed class TransactionRepository : RepositoryBase<Domain.ITransaction, Trans>, ITransactionRepository
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ICategoryBaseRepository _categoryBaseRepository;
        private readonly IPayeeRepository _payeeRepository;
        private readonly IFlagRepository _flagRepository;

        public TransactionRepository(
            IProvideConnection provideConnection,
            ICrudOrm crudOrm,
            IAccountRepository accountRepository,
            ICategoryBaseRepository categoryBaseRepository,
            IPayeeRepository payeeRepository,
            IFlagRepository flagRepository) : base(provideConnection, crudOrm)
        {
            _accountRepository = accountRepository;
            _categoryBaseRepository = categoryBaseRepository;
            _payeeRepository = payeeRepository;
            _flagRepository = flagRepository;
        }
        
        protected override Converter<Domain.ITransaction, Trans> ConvertToPersistence => domainTransaction => 
            new Trans
            {
                Id = domainTransaction.Id,
                AccountId = domainTransaction.Account.Id,
                FlagId = domainTransaction.Flag == null || domainTransaction.Flag == Domain.Flag.Default ? (long?)null : domainTransaction.Flag.Id,
                CheckNumber = domainTransaction.CheckNumber,
                CategoryId = domainTransaction.Category?.Id,
                PayeeId = domainTransaction.Payee.Id,
                Date = domainTransaction.Date,
                Memo = domainTransaction.Memo,
                Sum = domainTransaction.Sum,
                Cleared = domainTransaction.Cleared ? 1L : 0L,
                Type = nameof(TransType.Transaction)
            };

        protected override Converter<(Trans, DbConnection), Domain.ITransaction> ConvertToDomain => tuple =>
        {
            (Trans persistenceTransaction, DbConnection connection) = tuple;
            return new Domain.Transaction(
                this,
                persistenceTransaction.Date,
                persistenceTransaction.Id,
                persistenceTransaction.FlagId == null ? null :_flagRepository.Find((long)persistenceTransaction.FlagId, connection),
                persistenceTransaction.CheckNumber,
                _accountRepository.Find(persistenceTransaction.AccountId, connection),
                _payeeRepository.Find(persistenceTransaction.PayeeId, connection),
                persistenceTransaction.CategoryId == null ? null : _categoryBaseRepository.Find((long)persistenceTransaction.CategoryId, connection),
                persistenceTransaction.Memo,
                persistenceTransaction.Sum,
                persistenceTransaction.Cleared == 1L);
        };
    }
}