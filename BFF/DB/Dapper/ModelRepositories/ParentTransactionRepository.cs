using System;
using System.Data.Common;
using BFF.DB.PersistenceModels;
using BFF.MVVM.Models.Native.Structure;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public interface IParentTransactionRepository : IRepositoryBase<Domain.IParentTransaction>
    {
    }

    public sealed class ParentTransactionRepository : RepositoryBase<Domain.IParentTransaction, Trans>, IParentTransactionRepository
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IPayeeRepository _payeeRepository;
        private readonly ISubTransactionRepository _subTransactionRepository;
        private readonly IFlagRepository _flagRepository;

        public ParentTransactionRepository(
            IProvideConnection provideConnection,
            IAccountRepository accountRepository,
            IPayeeRepository payeeRepository,
            ISubTransactionRepository subTransactionRepository,
            IFlagRepository flagRepository) : base(provideConnection)
        {
            _accountRepository = accountRepository;
            _payeeRepository = payeeRepository;
            _subTransactionRepository = subTransactionRepository;
            _flagRepository = flagRepository;
        }
        
        protected override Converter<Domain.IParentTransaction, Trans> ConvertToPersistence => domainParentTransaction => 
            new Trans
            {
                Id = domainParentTransaction.Id,
                AccountId = domainParentTransaction.Account.Id,
                CategoryId = -69,
                FlagId = domainParentTransaction.Flag == null || domainParentTransaction.Flag == Domain.Flag.Default ? (long?) null : domainParentTransaction.Flag.Id,
                CheckNumber = domainParentTransaction.CheckNumber,
                PayeeId = domainParentTransaction.Payee.Id,
                Date = domainParentTransaction.Date,
                Memo = domainParentTransaction.Memo,
                Cleared = domainParentTransaction.Cleared ? 1L : 0L,
                Sum = -69,
                Type = nameof(TransType.ParentTransaction)
            };

        protected override Converter<(Trans, DbConnection), Domain.IParentTransaction> ConvertToDomain => tuple =>
        {
            (Trans persistenceParentTransaction, DbConnection connection) = tuple;

            var parentTransaction = new Domain.ParentTransaction(
                this,
                _subTransactionRepository.GetChildrenOf(persistenceParentTransaction.Id, connection),
                persistenceParentTransaction.Date,
                persistenceParentTransaction.Id,
                persistenceParentTransaction.FlagId == null ? null : _flagRepository.Find((long)persistenceParentTransaction.FlagId, connection),
                persistenceParentTransaction.CheckNumber,
                _accountRepository.Find(persistenceParentTransaction.AccountId, connection),
                _payeeRepository.Find(persistenceParentTransaction.PayeeId, connection),
                persistenceParentTransaction.Memo,
                persistenceParentTransaction.Cleared == 1L);

            foreach (var subTransaction in parentTransaction.SubTransactions)
            {
                subTransaction.Parent = parentTransaction;
            }

            return parentTransaction;
        };
    }
}