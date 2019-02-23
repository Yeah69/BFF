using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Models.Structure;
using BFF.Persistence.Models.Sql;
using BFF.Persistence.ORM;
using BFF.Persistence.ORM.Sqlite.Interfaces;

namespace BFF.Model.Repositories.ModelRepositories
{
    public interface ITransRepository :
        ISpecifiedPagedAccessAsync<ITransBase, IAccount>
    {
        Task<IEnumerable<ITransBase>> GetFromMonthAsync(DateTime month);
        Task<IEnumerable<ITransBase>> GetFromMonthAndCategoryAsync(DateTime month, ICategoryBase category);
        Task<IEnumerable<ITransBase>> GetFromMonthAndCategoriesAsync(DateTime month, IEnumerable<ICategoryBase> categories);
    }


    internal sealed class TransRepository : RepositoryBase<ITransBase, ITransSql>, ITransRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly IRepository<ITransaction, ITransSql> _transactionRepository;
        private readonly IRepository<ITransfer, ITransSql> _transferRepository;
        private readonly IRepository<IParentTransaction, ITransSql> _parentTransactionRepository;
        private readonly IAccountRepositoryInternal _accountRepository;
        private readonly ICategoryBaseRepositoryInternal _categoryBaseRepository;
        private readonly IPayeeRepositoryInternal _payeeRepository;
        private readonly ISubTransactionRepository _subTransactionsRepository;
        private readonly ITransOrm _transOrm;
        private readonly IFlagRepositoryInternal _flagRepository;

        public TransRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            IRepository<ITransaction, ITransSql> transactionRepository, 
            IRepository<ITransfer, ITransSql> transferRepository, 
            IRepository<IParentTransaction, ITransSql> parentTransactionRepository,
            IAccountRepositoryInternal accountRepository,
            ICategoryBaseRepositoryInternal categoryBaseRepository,
            IPayeeRepositoryInternal payeeRepository,
            ISubTransactionRepository subTransactionsRepository, 
            ICrudOrm<ITransSql> crudOrm,
            ITransOrm transOrm,
            IFlagRepositoryInternal flagRepository)
            : base(crudOrm)
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
        }

        public async Task<IEnumerable<ITransBase>> GetPageAsync(int offset, int pageSize, IAccount specifyingObject)
        {
            Task<IEnumerable<ITransSql>> elements;
            if (specifyingObject is null)
            {
                elements = _transOrm.GetPageFromSummaryAccountAsync(offset, pageSize);
            }
            else
            {
                var internalDataModel = specifyingObject as IDataModelInternal<IAccountSql>;
                elements = _transOrm.GetPageFromSpecificAccountAsync(offset, pageSize,
                    internalDataModel.BackingPersistenceModel.Id);
            }

            return await (await elements.ConfigureAwait(false))
                .Select(async t => await ConvertToDomainAsync(t).ConfigureAwait(false)).ToAwaitableEnumerable().ConfigureAwait(false);
        }

        public async Task<long> GetCountAsync(IAccount specifyingObject)
        {
            Task<long> result;
            if (specifyingObject is null)
            {
                result = _transOrm.GetCountFromSummaryAccountAsync();
            }
            else
            {
                var internalDataModel = specifyingObject as IDataModelInternal<IAccountSql>;
                result = _transOrm.GetCountFromSpecificAccountAsync(
                    internalDataModel.BackingPersistenceModel.Id);
            }
            return await result.ConfigureAwait(false);
        }

        public async Task<IEnumerable<ITransBase>> GetFromMonthAsync(DateTime month)
        {
            return await(await _transOrm.GetFromMonthAsync(month).ConfigureAwait(false))
                .Select(async t => await ConvertToDomainAsync(t).ConfigureAwait(false)).ToAwaitableEnumerable().ConfigureAwait(false);
        }

        public async Task<IEnumerable<ITransBase>> GetFromMonthAndCategoryAsync(DateTime month, ICategoryBase category)
        {
            long id = (category as IDataModelInternal<ICategorySql>).BackingPersistenceModel.Id;
            return await (await _transOrm.GetFromMonthAndCategoryAsync(month, id).ConfigureAwait(false))
                .Select(async t => await ConvertToDomainAsync(t).ConfigureAwait(false)).ToAwaitableEnumerable().ConfigureAwait(false);
        }

        public async Task<IEnumerable<ITransBase>> GetFromMonthAndCategoriesAsync(DateTime month, IEnumerable<ICategoryBase> categories)
        {
            return await (await _transOrm.GetFromMonthAndCategoriesAsync(month, categories.OfType<IDataModelInternal<ICategorySql>>().Select(c => c.BackingPersistenceModel.Id).ToArray()).ConfigureAwait(false))
                .Select(async t => await ConvertToDomainAsync(t).ConfigureAwait(false)).ToAwaitableEnumerable().ConfigureAwait(false);
        }

        protected override async Task<ITransBase> ConvertToDomainAsync(ITransSql persistenceModel)
        {
            Enum.TryParse(persistenceModel.Type, true, out TransType type);
            ITransBase ret;
            switch (type)
            {
                case TransType.Transaction:
                    ret = new Transaction<ITransSql>(
                        persistenceModel,
                        _transactionRepository,
                        _rxSchedulerProvider,
                        persistenceModel.Date,
                        persistenceModel.Id > 0,
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
                    ret = new Transfer<ITransSql>(
                        persistenceModel,
                        _transferRepository,
                        _rxSchedulerProvider,
                        persistenceModel.Date,
                        persistenceModel.Id > 0,
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
                    ret = new ParentTransaction<ITransSql>(
                        persistenceModel,
                        _parentTransactionRepository,
                        _rxSchedulerProvider,
                        await _subTransactionsRepository.GetChildrenOfAsync(persistenceModel.Id).ConfigureAwait(false),
                        persistenceModel.Date,
                        persistenceModel.Id > 0,
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
                    ret = new Transaction<ITransSql>(
                            persistenceModel,
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