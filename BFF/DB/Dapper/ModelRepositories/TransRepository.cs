using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.DB.PersistenceModels;
using BFF.MVVM.Models.Native.Structure;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public interface ITransRepository : 
        IRepositoryBase<ITransBase>, 
        ISpecifiedPagedAccess<ITransBase, Domain.IAccount>,
        ISpecifiedPagedAccessAsync<ITransBase, Domain.IAccount>
    {
    }


    public sealed class TransRepository : RepositoryBase<ITransBase, Trans>, ITransRepository
    {
        private readonly IRepository<Domain.ITransaction> _transactionRepository;
        private readonly IRepository<Domain.ITransfer> _transferRepository;
        private readonly IRepository<Domain.IParentTransaction> _parentTransactionRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly ICategoryBaseRepository _categoryBaseRepository;
        private readonly IPayeeRepository _payeeRepository;
        private readonly ISubTransactionRepository _subTransactionsRepository;
        private readonly ITransOrm _transOrm;
        private readonly IFlagRepository _flagRepository;

        public TransRepository(
            IProvideConnection provideConnection, 
            IRepository<Domain.ITransaction> transactionRepository, 
            IRepository<Domain.ITransfer> transferRepository, 
            IRepository<Domain.IParentTransaction> parentTransactionRepository,
            IAccountRepository accountRepository,
            ICategoryBaseRepository categoryBaseRepository,
            IPayeeRepository payeeRepository,
            ISubTransactionRepository subTransactionsRepository, 
            ICrudOrm crudOrm,
            ITransOrm transOrm,
            IFlagRepository flagRepository)
            : base(provideConnection, crudOrm)
        {
            _transactionRepository = transactionRepository;
            _transferRepository = transferRepository;
            _parentTransactionRepository = parentTransactionRepository;
            _accountRepository = accountRepository;
            _categoryBaseRepository = categoryBaseRepository;
            _payeeRepository = payeeRepository;
            _subTransactionsRepository = subTransactionsRepository;
            _transOrm = transOrm;
            _flagRepository = flagRepository;
        }

        protected override Converter<Trans, ITransBase> ConvertToDomain => trans =>
        {
            Enum.TryParse(trans.Type, true, out TransType type);
            ITransBase ret;
            switch(type)
            {
                case TransType.Transaction:
                    ret = new Domain.Transaction(
                        _transactionRepository, 
                        trans.Date,
                        trans.Id,
                        trans.FlagId == null ? null : _flagRepository.Find((long)trans.FlagId),
                        trans.CheckNumber,
                        _accountRepository.Find(trans.AccountId),
                        _payeeRepository.Find(trans.PayeeId),
                        trans.CategoryId == null ? null : _categoryBaseRepository.Find((long)trans.CategoryId), 
                        trans.Memo, 
                        trans.Sum,
                        trans.Cleared == 1L);
                    break;
                case TransType.Transfer:
                    ret = new Domain.Transfer(
                        _transferRepository,
                        trans.Date,
                        trans.Id,
                        trans.FlagId == null ? null : _flagRepository.Find((long)trans.FlagId),
                        trans.CheckNumber,
                        _accountRepository.Find(trans.PayeeId),
                        _accountRepository.Find(trans.CategoryId ?? -1), // This CategoryId should never be a null, because it comes from a transfer
                        trans.Memo,
                        trans.Sum, 
                        trans.Cleared == 1L);
                    break;
                case TransType.ParentTransaction:
                    ret = new Domain.ParentTransaction(
                        _parentTransactionRepository,
                        _subTransactionsRepository.GetChildrenOf(trans.Id), 
                        trans.Date,
                        trans.Id,
                        trans.FlagId == null ? null : _flagRepository.Find((long)trans.FlagId),
                        trans.CheckNumber,
                        _accountRepository.Find(trans.AccountId),
                        _payeeRepository.Find(trans.PayeeId),
                        trans.Memo, 
                        trans.Cleared == 1L);
                    break;
                default:
                    ret = new Domain.Transaction(_transactionRepository, DateTime.Today)
                        {Memo = "ERROR ERROR In the custom mapping ERROR ERROR ERROR ERROR"};
                    break;
            }

            return ret;
        };

        protected override Converter<ITransBase, Trans> ConvertToPersistence => domainTransBase =>
        {
            long accountId = -69;
            long? categoryId = -69;
            long payeeId = -1;
            long sum = -69;
            string type = "";
            switch (domainTransBase)
            {
                case Domain.ITransaction transaction:
                    accountId = transaction.Account.Id;
                    categoryId = transaction.Category?.Id;
                    payeeId = transaction.Payee.Id;
                    sum = transaction.Sum;
                    type = TransType.Transaction.ToString();
                    break;
                case Domain.IParentTransaction parentTransaction:
                    accountId = parentTransaction.Account.Id;
                    payeeId = parentTransaction.Payee.Id;
                    type = TransType.ParentTransaction.ToString();
                    break;
                case Domain.ITransfer transfer:
                    categoryId = transfer.ToAccount.Id;
                    payeeId = transfer.FromAccount.Id;
                    sum = transfer.Sum;
                    type = TransType.Transfer.ToString();
                    break;
            }

            return new Trans
            {
                Id = domainTransBase.Id,
                AccountId = accountId,
                FlagId = domainTransBase.Flag == null || domainTransBase.Flag == Domain.Flag.Default
                    ? (long?) null
                    : domainTransBase.Flag.Id,
                CheckNumber = domainTransBase.CheckNumber,
                CategoryId = categoryId,
                PayeeId = payeeId,
                Date = domainTransBase.Date,
                Memo = domainTransBase.Memo,
                Sum = sum,
                Cleared = domainTransBase.Cleared ? 1L : 0L,
                Type = type
            };
        };

        public IEnumerable<ITransBase> GetPage(int offset, int pageSize, Domain.IAccount specifyingObject)
        {
            return (specifyingObject is Domain.IAccount account
                ? _transOrm.GetPageFromSpecificAccount(offset, pageSize, account.Id)
                : _transOrm.GetPageFromSummaryAccount(offset, pageSize))
                .Select(t => ConvertToDomain(t));
        }

        public long GetCount(Domain.IAccount specifyingObject)
        {
            return specifyingObject is Domain.IAccount account
                    ? _transOrm.GetCountFromSpecificAccount(account.Id)
                    : _transOrm.GetCountFromSummaryAccount();
        }

        public async Task<IEnumerable<ITransBase>> GetPageAsync(int offset, int pageSize, Domain.IAccount specifyingObject)
        {
            return (await (specifyingObject is Domain.IAccount account
                ? _transOrm.GetPageFromSpecificAccountAsync(offset, pageSize, account.Id)
                : _transOrm.GetPageFromSummaryAccountAsync(offset, pageSize)).ConfigureAwait(false))
                .Select(t => ConvertToDomain(t));
        }

        public async Task<long> GetCountAsync(Domain.IAccount specifyingObject)
        {
            return await (specifyingObject is Domain.IAccount account
                ? _transOrm.GetCountFromSpecificAccountAsync(account.Id)
                : _transOrm.GetCountFromSummaryAccountAsync()).ConfigureAwait(false);
        }
    }
}