using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Models.Structure;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using MrMeeseeks.Extensions;
using Account = BFF.Persistence.Realm.Models.Domain.Account;
using Category = BFF.Persistence.Realm.Models.Domain.Category;
using IncomeCategory = BFF.Persistence.Realm.Models.Domain.IncomeCategory;

namespace BFF.Persistence.Realm.Repositories.ModelRepositories
{
    public interface IRealmTransRepository
    {
        Task<IEnumerable<ITransBase>> GetFromMonthAsync(DateTime month);
        Task<IEnumerable<ITransBase>> GetFromMonthAndCategoryAsync(DateTime month, ICategoryBase category);
        Task<IEnumerable<ITransBase>> GetFromMonthAndCategoriesAsync(DateTime month, IEnumerable<ICategoryBase> categories);
        Task<IEnumerable<ITransBase>> GetPageAsync(int offset, int pageSize, IAccount specifyingObject);
        Task<long> GetCountAsync(IAccount specifyingObject);
    }

    internal sealed class RealmTransRepository : RealmRepositoryBase<ITransBase, ITransRealm>, IRealmTransRepository
    {
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly Lazy<IRealmAccountRepositoryInternal> _accountRepository;
        private readonly Lazy<IRealmCategoryBaseRepositoryInternal> _categoryBaseRepository;
        private readonly Lazy<IRealmPayeeRepositoryInternal> _payeeRepository;
        private readonly Lazy<IRealmFlagRepositoryInternal> _flagRepository;
        private readonly Lazy<IRealmSubTransactionRepository> _subTransactionsRepository;
        private readonly ICrudOrm<ITransRealm> _crudOrm;
        private readonly Lazy<ITransOrm> _transOrm;

        public RealmTransRepository(
            IRxSchedulerProvider rxSchedulerProvider,
            Lazy<IRealmAccountRepositoryInternal> accountRepository,
            Lazy<IRealmCategoryBaseRepositoryInternal> categoryBaseRepository,
            Lazy<IRealmPayeeRepositoryInternal> payeeRepository,
            Lazy<IRealmFlagRepositoryInternal> flagRepository,
            Lazy<IRealmSubTransactionRepository> subTransactionsRepository, 
            ICrudOrm<ITransRealm> crudOrm,
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
            Task<IEnumerable<ITransRealm>> elements;
            if (specifyingObject is ISummaryAccount)
            {
                elements = _transOrm.Value.GetPageFromSummaryAccountAsync(offset, pageSize);
            }
            else
            {
                if (!(specifyingObject is Account account)) throw new ArgumentException("Model instance has not the correct type", nameof(specifyingObject));
                
                elements = _transOrm.Value.GetPageFromSpecificAccountAsync(
                    offset, 
                    pageSize,
                    account.RealmObject);
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
                if (!(specifyingObject is Account account)) throw new ArgumentException("Model instance has not the correct type", nameof(specifyingObject));
                
                result = _transOrm.Value.GetCountFromSpecificAccountAsync(
                    account.RealmObject);
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
            if (!(category is Category || category is IncomeCategory)) throw new ArgumentException("Model instance has not the correct type", nameof(category));

            var id = category switch
                {
                    Category cat => cat.RealmObject,
                    IncomeCategory cat => cat.RealmObject,
                    _ => throw new Exception("Unexpected")
                };
            return await (await _transOrm.Value.GetFromMonthAndCategoryAsync(month, id).ConfigureAwait(false))
                .Select(async t => await ConvertToDomainAsync(t).ConfigureAwait(false)).ToAwaitableEnumerable().ConfigureAwait(false);
        }

        public async Task<IEnumerable<ITransBase>> GetFromMonthAndCategoriesAsync(DateTime month, IEnumerable<ICategoryBase> categories)
        {
            return await 
                (await _transOrm
                    .Value
                    .GetFromMonthAndCategoriesAsync(
                        month, 
                        categories
                            .Where(c => c is Category || c is IncomeCategory)
                            .Select(c => c switch
                                {
                                    Category cat => cat.RealmObject,
                                    IncomeCategory cat => cat.RealmObject,
                                    _ => throw new Exception("Unexpected")
                                })
                            .ToArray())
                    .ConfigureAwait(false))
                .Select(async t => 
                    await ConvertToDomainAsync(t)
                        .ConfigureAwait(false))
                .ToAwaitableEnumerable()
                .ConfigureAwait(false);
        }

        protected override async Task<ITransBase> ConvertToDomainAsync(ITransRealm persistenceModel)
        {
            ITransBase ret;
            switch (persistenceModel.Type)
            {
                case TransType.Transaction:
                    ret = new Models.Domain.Transaction(
                        _crudOrm,
                        _rxSchedulerProvider,
                        persistenceModel,
                        persistenceModel.Date,
                        persistenceModel.Flag is null
                            ? null
                            : await _flagRepository.Value.FindAsync(persistenceModel.Flag).ConfigureAwait(false),
                        persistenceModel.CheckNumber,
                        await _accountRepository.Value.FindAsync(persistenceModel.Account).ConfigureAwait(false),
                        persistenceModel.Payee is null
                            ? null
                            : await _payeeRepository.Value.FindAsync(persistenceModel.Payee).ConfigureAwait(false),
                        persistenceModel.Category is null
                            ? null
                            : await _categoryBaseRepository.Value.FindAsync(persistenceModel.Category).ConfigureAwait(false),
                        persistenceModel.Memo,
                        persistenceModel.Sum,
                        persistenceModel.Cleared);
                    break;
                case TransType.Transfer:
                    ret = new Models.Domain.Transfer(
                        _crudOrm,
                        _rxSchedulerProvider,
                        persistenceModel,
                        persistenceModel.Date,
                        persistenceModel.Flag is null
                            ? null
                            : await _flagRepository.Value.FindAsync(persistenceModel.Flag).ConfigureAwait(false),
                        persistenceModel.CheckNumber,
                        persistenceModel.FromAccount is null
                            ? null
                            : await _accountRepository.Value.FindAsync(persistenceModel.FromAccount).ConfigureAwait(false),
                        persistenceModel.ToAccount is null
                            ? null
                            : await _accountRepository.Value.FindAsync(persistenceModel.ToAccount).ConfigureAwait(false),
                        persistenceModel.Memo,
                        persistenceModel.Sum,
                        persistenceModel.Cleared);
                    break;
                case TransType.ParentTransaction:
                    ret = new Models.Domain.ParentTransaction(
                        _crudOrm,
                        _subTransactionsRepository.Value,
                        _rxSchedulerProvider,
                        persistenceModel,
                        persistenceModel.Date,
                        persistenceModel.Flag is null
                            ? null
                            : await _flagRepository.Value.FindAsync(persistenceModel.Flag).ConfigureAwait(false),
                        persistenceModel.CheckNumber,
                        await _accountRepository.Value.FindAsync(persistenceModel.Account).ConfigureAwait(false),
                        persistenceModel.Payee is null
                            ? null
                            : await _payeeRepository.Value.FindAsync(persistenceModel.Payee).ConfigureAwait(false),
                        persistenceModel.Memo,
                        persistenceModel.Cleared);
                    break;
                default:
                    ret = new Models.Domain.Transaction(
                            _crudOrm,
                            _rxSchedulerProvider,
                            null,
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