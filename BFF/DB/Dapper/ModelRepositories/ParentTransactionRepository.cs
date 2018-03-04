using System;
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
            ICrudOrm crudOrm,
            IAccountRepository accountRepository,
            IPayeeRepository payeeRepository,
            ISubTransactionRepository subTransactionRepository,
            IFlagRepository flagRepository) : base(provideConnection, crudOrm)
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
                FlagId = domainParentTransaction.Flag is null || domainParentTransaction.Flag == Domain.Flag.Default 
                    ? (long?) null 
                    : domainParentTransaction.Flag.Id,
                CheckNumber = domainParentTransaction.CheckNumber,
                PayeeId = domainParentTransaction.Payee?.Id,
                Date = domainParentTransaction.Date,
                Memo = domainParentTransaction.Memo,
                Cleared = domainParentTransaction.Cleared ? 1L : 0L,
                Sum = -69,
                Type = nameof(TransType.ParentTransaction)
            };

        protected override Converter<Trans, Domain.IParentTransaction> ConvertToDomain => persistenceParentTransaction =>
        {
            var parentTransaction = new Domain.ParentTransaction(
                this,
                _subTransactionRepository.GetChildrenOf(persistenceParentTransaction.Id),
                persistenceParentTransaction.Date,
                persistenceParentTransaction.Id,
                persistenceParentTransaction.FlagId is null 
                    ? null 
                    : _flagRepository.Find((long) persistenceParentTransaction.FlagId),
                persistenceParentTransaction.CheckNumber,
                _accountRepository.Find(persistenceParentTransaction.AccountId),
                persistenceParentTransaction.PayeeId is null
                    ? null 
                    : _payeeRepository.Find((long) persistenceParentTransaction.PayeeId),
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