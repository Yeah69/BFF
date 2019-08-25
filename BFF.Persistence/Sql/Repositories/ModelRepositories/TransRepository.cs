using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Models.Structure;
using BFF.Persistence.Sql.Models.Domain;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;
using MrMeeseeks.Extensions;

namespace BFF.Persistence.Sql.Repositories.ModelRepositories
{
    public interface ISqliteTransRepository
    {
        Task<IEnumerable<ITransBase>> GetFromMonthAsync(DateTime month);
        Task<IEnumerable<ITransBase>> GetFromMonthAndCategoryAsync(DateTime month, ICategoryBase category);
        Task<IEnumerable<ITransBase>> GetFromMonthAndCategoriesAsync(DateTime month, IEnumerable<ICategoryBase> categories);
        Task<IEnumerable<ITransBase>> GetPageAsync(int offset, int pageSize, IAccount specifyingObject);
        Task<long> GetCountAsync(IAccount specifyingObject);
    }

    internal sealed class SqliteTransRepository : SqliteRepositoryBase<ITransBase, ITransSql>, ISqliteTransRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly Lazy<ISqliteAccountRepositoryInternal> _accountRepository;
        private readonly Lazy<ISqliteCategoryBaseRepositoryInternal> _categoryBaseRepository;
        private readonly Lazy<ISqlitePayeeRepositoryInternal> _payeeRepository;
        private readonly Lazy<ISqliteFlagRepositoryInternal> _flagRepository;
        private readonly Lazy<ISqliteSubTransactionRepository> _subTransactionsRepository;
        private readonly ICrudOrm<ITransSql> _crudOrm;
        private readonly Lazy<ITransOrm> _transOrm;

        public SqliteTransRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            Lazy<ISqliteAccountRepositoryInternal> accountRepository,
            Lazy<ISqliteCategoryBaseRepositoryInternal> categoryBaseRepository,
            Lazy<ISqlitePayeeRepositoryInternal> payeeRepository,
            Lazy<ISqliteFlagRepositoryInternal> flagRepository,
            Lazy<ISqliteSubTransactionRepository> subTransactionsRepository, 
            ICrudOrm<ITransSql> crudOrm,
            Lazy<ITransOrm> transOrm)
            : base(crudOrm)
        {
            _rxSchedulerProvider = rxSchedulerProvider;
            _accountRepository = accountRepository;
            _categoryBaseRepository = categoryBaseRepository;
            _payeeRepository = payeeRepository;
            _subTransactionsRepository = subTransactionsRepository;
            _crudOrm = crudOrm;
            _transOrm = transOrm;
            _flagRepository = flagRepository;
        }

        public async Task<IEnumerable<ITransBase>> GetPageAsync(int offset, int pageSize, IAccount specifyingObject)
        {
            Task<IEnumerable<ITransSql>> elements;
            if (specifyingObject is ISummaryAccount)
            {
                elements = _transOrm.Value.GetPageFromSummaryAccountAsync(offset, pageSize);
            }
            else
            {
                if (!(specifyingObject is ISqlModel)) throw new ArgumentException("Model instance has not the correct type", nameof(specifyingObject));
                
                elements = _transOrm.Value.GetPageFromSpecificAccountAsync(
                    offset, 
                    pageSize,
                    ((ISqlModel)specifyingObject).Id);
            }

            return await (await elements.ConfigureAwait(false))
                .Select(async t => await ConvertToDomainAsync(t).ConfigureAwait(false)).ToAwaitableEnumerable().ConfigureAwait(false);
        }

        public async Task<long> GetCountAsync(IAccount specifyingObject)
        {
            Task<long> result;
            if (specifyingObject is ISummaryAccount)
            {
                result = _transOrm.Value.GetCountFromSummaryAccountAsync();
            }
            else
            {
                if (!(specifyingObject is ISqlModel)) throw new ArgumentException("Model instance has not the correct type", nameof(specifyingObject));
                
                result = _transOrm.Value.GetCountFromSpecificAccountAsync(
                    ((ISqlModel)specifyingObject).Id);
            }
            return await result.ConfigureAwait(false);
        }

        public async Task<IEnumerable<ITransBase>> GetFromMonthAsync(DateTime month)
        {
            return await(await _transOrm.Value.GetFromMonthAsync(month).ConfigureAwait(false))
                .Select(async t => await ConvertToDomainAsync(t).ConfigureAwait(false)).ToAwaitableEnumerable().ConfigureAwait(false);
        }

        public async Task<IEnumerable<ITransBase>> GetFromMonthAndCategoryAsync(DateTime month, ICategoryBase category)
        {
            if (!(category is ISqlModel)) throw new ArgumentException("Model instance has not the correct type", nameof(category));

            long id = ((ISqlModel) category).Id;
            return await (await _transOrm.Value.GetFromMonthAndCategoryAsync(month, id).ConfigureAwait(false))
                .Select(async t => await ConvertToDomainAsync(t).ConfigureAwait(false)).ToAwaitableEnumerable().ConfigureAwait(false);
        }

        public async Task<IEnumerable<ITransBase>> GetFromMonthAndCategoriesAsync(DateTime month, IEnumerable<ICategoryBase> categories)
        {
            return await (await _transOrm.Value.GetFromMonthAndCategoriesAsync(month, categories.OfType<ISqlModel>().Select(c => c.Id).ToArray()).ConfigureAwait(false))
                .Select(async t => await ConvertToDomainAsync(t).ConfigureAwait(false)).ToAwaitableEnumerable().ConfigureAwait(false);
        }

        protected override async Task<ITransBase> ConvertToDomainAsync(ITransSql persistenceModel)
        {
            Enum.TryParse(persistenceModel.Type, true, out TransType type);
            ITransBase ret;
            switch (type)
            {
                case TransType.Transaction:
                    ret = new Models.Domain.Transaction(
                        _crudOrm,
                        _rxSchedulerProvider,
                        persistenceModel.Id,
                        persistenceModel.Date,
                        persistenceModel.FlagId is null
                            ? null
                            : await _flagRepository.Value.FindAsync((long)persistenceModel.FlagId).ConfigureAwait(false),
                        persistenceModel.CheckNumber,
                        await _accountRepository.Value.FindAsync(persistenceModel.AccountId).ConfigureAwait(false),
                        persistenceModel.PayeeId is null
                            ? null
                            : await _payeeRepository.Value.FindAsync((long)persistenceModel.PayeeId).ConfigureAwait(false),
                        persistenceModel.CategoryId is null
                            ? null
                            : await _categoryBaseRepository.Value.FindAsync((long)persistenceModel.CategoryId).ConfigureAwait(false),
                        persistenceModel.Memo,
                        persistenceModel.Sum,
                        persistenceModel.Cleared == 1L);
                    break;
                case TransType.Transfer:
                    ret = new Models.Domain.Transfer(
                        _crudOrm,
                        _rxSchedulerProvider,
                        persistenceModel.Id,
                        persistenceModel.Date,
                        persistenceModel.FlagId is null
                            ? null
                            : await _flagRepository.Value.FindAsync((long)persistenceModel.FlagId).ConfigureAwait(false),
                        persistenceModel.CheckNumber,
                        persistenceModel.PayeeId is null
                            ? null
                            : await _accountRepository.Value.FindAsync((long)persistenceModel.PayeeId).ConfigureAwait(false),
                        persistenceModel.CategoryId is null
                            ? null
                            : await _accountRepository.Value.FindAsync((long)persistenceModel.CategoryId).ConfigureAwait(false),
                        persistenceModel.Memo,
                        persistenceModel.Sum,
                        persistenceModel.Cleared == 1L);
                    break;
                case TransType.ParentTransaction:
                    ret = new Models.Domain.ParentTransaction(
                        _crudOrm,
                        _subTransactionsRepository.Value,
                        _rxSchedulerProvider,
                        persistenceModel.Id,
                        persistenceModel.Date,
                        persistenceModel.FlagId is null
                            ? null
                            : await _flagRepository.Value.FindAsync((long)persistenceModel.FlagId).ConfigureAwait(false),
                        persistenceModel.CheckNumber,
                        await _accountRepository.Value.FindAsync(persistenceModel.AccountId).ConfigureAwait(false),
                        persistenceModel.PayeeId is null
                            ? null
                            : await _payeeRepository.Value.FindAsync((long)persistenceModel.PayeeId).ConfigureAwait(false),
                        persistenceModel.Memo,
                        persistenceModel.Cleared == 1L);
                    break;
                default:
                    ret = new Models.Domain.Transaction(
                            _crudOrm,
                            _rxSchedulerProvider,
                            -69,
                            DateTime.Today,
                            null,
                            "",
                            null,
                            null,
                            null,
                            "ERROR ERROR In the custom mapping ERROR ERROR ERROR ERROR",
                            0L,
                            false);
                    break;
            }

            return ret;
        }
    }
}