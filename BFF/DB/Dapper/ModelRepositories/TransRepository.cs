using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.DB.PersistenceModels;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native.Structure;
using Domain = BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper.ModelRepositories
{
    public interface ITransRepository : 
        IRepositoryBase<ITransBase>, 
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
                FlagId = domainTransBase.Flag is null || domainTransBase.Flag == Domain.Flag.Default
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

        public async Task<IEnumerable<ITransBase>> GetPageAsync(int offset, int pageSize, Domain.IAccount specifyingObject)
        {
            return await (await (specifyingObject is Domain.IAccount account
                ? _transOrm.GetPageFromSpecificAccountAsync(offset, pageSize, account.Id)
                : _transOrm.GetPageFromSummaryAccountAsync(offset, pageSize)).ConfigureAwait(false))
                .Select(async t => await ConvertToDomainAsync(t)).ToAwaitableEnumerable().ConfigureAwait(false);
        }

        public async Task<long> GetCountAsync(Domain.IAccount specifyingObject)
        {
            return await (specifyingObject is Domain.IAccount account
                ? _transOrm.GetCountFromSpecificAccountAsync(account.Id)
                : _transOrm.GetCountFromSummaryAccountAsync()).ConfigureAwait(false);
        }

        protected override async Task<ITransBase> ConvertToDomainAsync(Trans persistenceModel)
        {
            Enum.TryParse(persistenceModel.Type, true, out TransType type);
            ITransBase ret;
            switch (type)
            {
                case TransType.Transaction:
                    ret = new Domain.Transaction(
                        _transactionRepository,
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
                        persistenceModel.CategoryId is null
                            ? null
                            : await _categoryBaseRepository.FindAsync((long)persistenceModel.CategoryId).ConfigureAwait(false),
                        persistenceModel.Memo,
                        persistenceModel.Sum,
                        persistenceModel.Cleared == 1L);
                    break;
                case TransType.Transfer:
                    ret = new Domain.Transfer(
                        _transferRepository,
                        persistenceModel.Date,
                        persistenceModel.Id,
                        persistenceModel.FlagId is null
                            ? null
                            : await _flagRepository.FindAsync((long)persistenceModel.FlagId).ConfigureAwait(false),
                        persistenceModel.CheckNumber,
                        persistenceModel.PayeeId is null
                            ? null
                            : await _accountRepository.FindAsync((long)persistenceModel.PayeeId).ConfigureAwait(false),
                        persistenceModel.CategoryId is null
                            ? null
                            : await _accountRepository.FindAsync((long)persistenceModel.CategoryId).ConfigureAwait(false),
                        persistenceModel.Memo,
                        persistenceModel.Sum,
                        persistenceModel.Cleared == 1L);
                    break;
                case TransType.ParentTransaction:
                    ret = new Domain.ParentTransaction(
                        _parentTransactionRepository,
                        await _subTransactionsRepository.GetChildrenOfAsync(persistenceModel.Id).ConfigureAwait(false),
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
                    break;
                default:
                    ret = new Domain.Transaction(_transactionRepository, DateTime.Today)
                    { Memo = "ERROR ERROR In the custom mapping ERROR ERROR ERROR ERROR" };
                    break;
            }

            return ret;
        }
    }
}