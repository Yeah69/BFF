using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Models.Structure;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM.Sqlite;
using BFF.Persistence.ORM.Sqlite.Interfaces;

namespace BFF.Model.Repositories.ModelRepositories
{
    public interface ITransRepository : 
        IRepositoryBase<ITransBase>, 
        ISpecifiedPagedAccessAsync<ITransBase, IAccount>
    {
        Task<IEnumerable<ITransBase>> GetFromMonthAsync(DateTime month);
        Task<IEnumerable<ITransBase>> GetFromMonthAndCategoryAsync(DateTime month, ICategoryBase category);
        Task<IEnumerable<ITransBase>> GetFromMonthAndCategoriesAsync(DateTime month, IEnumerable<ICategoryBase> categories);
    }


    internal sealed class TransRepository : RepositoryBase<ITransBase, ITransSql>, ITransRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IRepository<ITransaction> _transactionRepository;
        private readonly IRepository<ITransfer> _transferRepository;
        private readonly IRepository<IParentTransaction> _parentTransactionRepository;
        private readonly IAccountRepositoryInternal _accountRepository;
        private readonly ICategoryBaseRepositoryInternal _categoryBaseRepository;
        private readonly IPayeeRepositoryInternal _payeeRepository;
        private readonly ISubTransactionRepository _subTransactionsRepository;
        private readonly ITransOrm _transOrm;
        private readonly IFlagRepositoryInternal _flagRepository;
        private readonly Func<ITransSql> _transDtoFactory;

        public TransRepository(
            IProvideSqliteConnection provideConnection, 
            IRxSchedulerProvider rxSchedulerProvider,
            IRepository<ITransaction> transactionRepository, 
            IRepository<ITransfer> transferRepository, 
            IRepository<IParentTransaction> parentTransactionRepository,
            IAccountRepositoryInternal accountRepository,
            ICategoryBaseRepositoryInternal categoryBaseRepository,
            IPayeeRepositoryInternal payeeRepository,
            ISubTransactionRepository subTransactionsRepository, 
            ICrudOrm crudOrm,
            ITransOrm transOrm,
            IFlagRepositoryInternal flagRepository,
            Func<ITransSql> transDtoFactory)
            : base(provideConnection, crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _transactionRepository = transactionRepository;
            _transferRepository = transferRepository;
            _parentTransactionRepository = parentTransactionRepository;
            _accountRepository = accountRepository;
            _categoryBaseRepository = categoryBaseRepository;
            _payeeRepository = payeeRepository;
            _subTransactionsRepository = subTransactionsRepository;
            _transOrm = transOrm;
            _flagRepository = flagRepository;
            _transDtoFactory = transDtoFactory;
        }

        protected override Converter<ITransBase, ITransSql> ConvertToPersistence => domainTransBase =>
        {
            long accountId = -69;
            long? categoryId = -69;
            long payeeId = -1;
            long sum = -69;
            string type = "";
            switch (domainTransBase)
            {
                case ITransaction transaction:
                    accountId = transaction.Account.Id;
                    categoryId = transaction.Category?.Id;
                    payeeId = transaction.Payee.Id;
                    sum = transaction.Sum;
                    type = TransType.Transaction.ToString();
                    break;
                case IParentTransaction parentTransaction:
                    accountId = parentTransaction.Account.Id;
                    payeeId = parentTransaction.Payee.Id;
                    type = TransType.ParentTransaction.ToString();
                    break;
                case ITransfer transfer:
                    categoryId = transfer.ToAccount.Id;
                    payeeId = transfer.FromAccount.Id;
                    sum = transfer.Sum;
                    type = TransType.Transfer.ToString();
                    break;
            }

            var transDto = _transDtoFactory();

            transDto.Id = domainTransBase.Id;
            transDto.AccountId = accountId;
            transDto.FlagId = domainTransBase.Flag is null || domainTransBase.Flag == Flag.Default
                ? (long?)null
                : domainTransBase.Flag.Id;
            transDto.CheckNumber = domainTransBase.CheckNumber;
            transDto.CategoryId = categoryId;
            transDto.PayeeId = payeeId;
            transDto.Date = domainTransBase.Date;
            transDto.Memo = domainTransBase.Memo;
            transDto.Sum = sum;
            transDto.Cleared = domainTransBase.Cleared ? 1L : 0L;
            transDto.Type = type;

            return transDto;
        };

        public async Task<IEnumerable<ITransBase>> GetPageAsync(int offset, int pageSize, IAccount specifyingObject)
        {
            return await (await (specifyingObject is IAccount account
                ? _transOrm.GetPageFromSpecificAccountAsync(offset, pageSize, account.Id)
                : _transOrm.GetPageFromSummaryAccountAsync(offset, pageSize)).ConfigureAwait(false))
                .Select(async t => await ConvertToDomainAsync(t).ConfigureAwait(false)).ToAwaitableEnumerable().ConfigureAwait(false);
        }

        public async Task<long> GetCountAsync(IAccount specifyingObject)
        {
            return await (specifyingObject is IAccount account
                ? _transOrm.GetCountFromSpecificAccountAsync(account.Id)
                : _transOrm.GetCountFromSummaryAccountAsync()).ConfigureAwait(false);
        }

        public async Task<IEnumerable<ITransBase>> GetFromMonthAsync(DateTime month)
        {
            return await(await _transOrm.GetFromMonthAsync(month).ConfigureAwait(false))
                .Select(async t => await ConvertToDomainAsync(t).ConfigureAwait(false)).ToAwaitableEnumerable().ConfigureAwait(false);
        }

        public async Task<IEnumerable<ITransBase>> GetFromMonthAndCategoryAsync(DateTime month, ICategoryBase category)
        {
            return await (await _transOrm.GetFromMonthAndCategoryAsync(month, category.Id).ConfigureAwait(false))
                .Select(async t => await ConvertToDomainAsync(t).ConfigureAwait(false)).ToAwaitableEnumerable().ConfigureAwait(false);
        }

        public async Task<IEnumerable<ITransBase>> GetFromMonthAndCategoriesAsync(DateTime month, IEnumerable<ICategoryBase> categories)
        {
            return await (await _transOrm.GetFromMonthAndCategoriesAsync(month, categories.Select(c => c.Id).ToArray()).ConfigureAwait(false))
                .Select(async t => await ConvertToDomainAsync(t).ConfigureAwait(false)).ToAwaitableEnumerable().ConfigureAwait(false);
        }

        protected override async Task<ITransBase> ConvertToDomainAsync(ITransSql persistenceModel)
        {
            Enum.TryParse(persistenceModel.Type, true, out TransType type);
            ITransBase ret;
            switch (type)
            {
                case TransType.Transaction:
                    ret = new Transaction(
                        _transactionRepository,
                        _rxSchedulerProvider,
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
                    ret = new Transfer(
                        _transferRepository,
                        _rxSchedulerProvider,
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
                    ret = new ParentTransaction(
                        _parentTransactionRepository,
                        _rxSchedulerProvider,
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
                    ret = new Transaction(
                            _transactionRepository, 
                            _rxSchedulerProvider, 
                            DateTime.Today)
                    { Memo = "ERROR ERROR In the custom mapping ERROR ERROR ERROR ERROR" };
                    break;
            }

            return ret;
        }
    }
}