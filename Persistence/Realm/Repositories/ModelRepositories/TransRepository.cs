using BFF.Core.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Model.Models;
using BFF.Model.Models.Structure;
using BFF.Persistence.Helper;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using MrMeeseeks.Extensions;
using Account = BFF.Persistence.Realm.Models.Domain.Account;
using Category = BFF.Persistence.Realm.Models.Domain.Category;
using IncomeCategory = BFF.Persistence.Realm.Models.Domain.IncomeCategory;

namespace BFF.Persistence.Realm.Repositories.ModelRepositories
{
    internal interface IRealmTransRepository
    {
        Task<IEnumerable<ITransBase>> GetFromMonthAsync(DateTime month);
        Task<IEnumerable<ITransBase>> GetFromMonthAndCategoryAsync(DateTime month, ICategoryBase category);
        Task<IEnumerable<ITransBase>> GetFromMonthAndCategoriesAsync(DateTime month, IEnumerable<ICategoryBase> categories);
        Task<IEnumerable<ITransBase>> GetPageAsync(int offset, int pageSize, IAccount specifyingObject);
        Task<long> GetCountAsync(IAccount specifyingObject);
    }

    internal sealed class RealmTransRepository : RealmRepositoryBase<ITransBase, Trans>, IRealmTransRepository, IScopeInstance
    {
        private readonly Lazy<IRealmAccountRepositoryInternal> _accountRepository;
        private readonly Lazy<IRealmCategoryBaseRepositoryInternal> _categoryBaseRepository;
        private readonly Lazy<IRealmPayeeRepositoryInternal> _payeeRepository;
        private readonly Lazy<IRealmFlagRepositoryInternal> _flagRepository;
        private readonly Lazy<IRealmSubTransactionRepository> _subTransactionsRepository;
        private readonly IDateTimeStaticDelegate _dateTimeStaticDelegate;
        private readonly ICrudOrm<Trans> _crudOrm;
        private readonly IRealmOperations _realmOperations;
        private readonly Lazy<ITransOrm> _transOrm;

        public RealmTransRepository(
            Lazy<IRealmAccountRepositoryInternal> accountRepository,
            Lazy<IRealmCategoryBaseRepositoryInternal> categoryBaseRepository,
            Lazy<IRealmPayeeRepositoryInternal> payeeRepository,
            Lazy<IRealmFlagRepositoryInternal> flagRepository,
            Lazy<IRealmSubTransactionRepository> subTransactionsRepository, 
            IDateTimeStaticDelegate dateTimeStaticDelegate,
            ICrudOrm<Trans> crudOrm,
            IRealmOperations realmOperations,
            Lazy<ITransOrm> transOrm)
            : base(crudOrm)
        {
            _accountRepository = accountRepository;
            _categoryBaseRepository = categoryBaseRepository;
            _payeeRepository = payeeRepository;
            _subTransactionsRepository = subTransactionsRepository;
            _dateTimeStaticDelegate = dateTimeStaticDelegate;
            _crudOrm = crudOrm;
            _realmOperations = realmOperations;
            _transOrm = transOrm;
            _flagRepository = flagRepository;
        }

        public async Task<IEnumerable<ITransBase>> GetPageAsync(int offset, int pageSize, IAccount specifyingObject)
        {
            Task<IEnumerable<Trans>> elements;
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
                    account.RealmObject ?? throw new NullReferenceException("Shouldn't be null at that point"));
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
                    account.RealmObject ?? throw new NullReferenceException("Shouldn't be null at that point"));
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
                } ?? throw new NullReferenceException("Shouldn't be null at that point");
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
                                } ?? throw new NullReferenceException("Shouldn't be null at that point"))
                            .ToArray())
                    .ConfigureAwait(false))
                .Select(async t => 
                    await ConvertToDomainAsync(t)
                        .ConfigureAwait(false))
                .ToAwaitableEnumerable()
                .ConfigureAwait(false);
        }

        protected override Task<ITransBase> ConvertToDomainAsync(Trans persistenceModel)
        {
            return _realmOperations.RunFuncAsync(InnerAsync);

            async Task<ITransBase> InnerAsync(Realms.Realm _)
            {
                ITransBase ret = (TransType)persistenceModel.TypeIndex switch
                {
                    TransType.Transaction => new Models.Domain.Transaction(
                        persistenceModel,
                        persistenceModel.Date.UtcDateTime,
                        persistenceModel.Flag is null
                            ? null
                            : await _flagRepository.Value.FindAsync(persistenceModel.Flag).ConfigureAwait(false),
                        persistenceModel.CheckNumber ?? String.Empty,
                        await _accountRepository.Value.FindAsync(persistenceModel.Account ?? throw new NullReferenceException("Shouldn't be null at that point")).ConfigureAwait(false),
                        persistenceModel.Payee is null
                            ? null
                            : await _payeeRepository.Value.FindAsync(persistenceModel.Payee).ConfigureAwait(false),
                        persistenceModel.Category is null
                            ? null
                            : await _categoryBaseRepository.Value.FindAsync(persistenceModel.Category)
                                .ConfigureAwait(false), 
                        persistenceModel.Memo ?? String.Empty, 
                        persistenceModel.Sum,
                        persistenceModel.Cleared,
                        _crudOrm),
                    TransType.Transfer => new Models.Domain.Transfer(
                        persistenceModel,
                        persistenceModel.Date.UtcDateTime,
                        persistenceModel.Flag is null
                            ? null
                            : await _flagRepository.Value.FindAsync(persistenceModel.Flag).ConfigureAwait(false),
                        persistenceModel.CheckNumber ?? String.Empty,
                        persistenceModel.FromAccount is null
                            ? null
                            : await _accountRepository.Value.FindAsync(persistenceModel.FromAccount)
                                .ConfigureAwait(false),
                        persistenceModel.ToAccount is null
                            ? null
                            : await _accountRepository.Value.FindAsync(persistenceModel.ToAccount)
                                .ConfigureAwait(false),
                        persistenceModel.Memo ?? String.Empty,
                        persistenceModel.Sum,
                        persistenceModel.Cleared,
                        _crudOrm),
                    TransType.ParentTransaction => new Models.Domain.ParentTransaction(
                        persistenceModel, 
                        persistenceModel.Date.UtcDateTime,
                        persistenceModel.Flag is null
                            ? null
                            : await _flagRepository.Value.FindAsync(persistenceModel.Flag).ConfigureAwait(false),
                        persistenceModel.CheckNumber ?? String.Empty,
                        await _accountRepository.Value.FindAsync(persistenceModel.Account ?? throw new NullReferenceException("Shouldn't be null at that point")).ConfigureAwait(false),
                        persistenceModel.Payee is null
                            ? null
                            : await _payeeRepository.Value.FindAsync(persistenceModel.Payee).ConfigureAwait(false),
                        persistenceModel.Memo ?? String.Empty, 
                        persistenceModel.Cleared,
                        _crudOrm,
                        _subTransactionsRepository.Value),
                    _ => new Models.Domain.Transaction(
                        null,
                        _dateTimeStaticDelegate.Today, 
                        null, 
                        "", 
                        null, 
                        null, 
                        null,
                        "ERROR ERROR In the custom mapping ERROR ERROR ERROR ERROR", 
                        0L, 
                        false,
                        _crudOrm)
                };

                return ret;
            }
        }
    }
}