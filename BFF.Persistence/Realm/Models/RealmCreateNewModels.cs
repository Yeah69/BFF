using System;
using System.Drawing;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM;
using BFF.Persistence.Realm.Repositories.ModelRepositories;
using Account = BFF.Persistence.Realm.Models.Persistence.Account;
using BudgetEntry = BFF.Persistence.Realm.Models.Persistence.BudgetEntry;
using Category = BFF.Persistence.Realm.Models.Persistence.Category;
using DbSetting = BFF.Persistence.Realm.Models.Persistence.DbSetting;
using Flag = BFF.Persistence.Realm.Models.Persistence.Flag;
using Payee = BFF.Persistence.Realm.Models.Persistence.Payee;
using SubTransaction = BFF.Persistence.Realm.Models.Persistence.SubTransaction;

namespace BFF.Persistence.Realm.Models
{
    internal interface IRealmCreateNewModels : ICreateNewModels
    {
    }

    internal class RealmCreateNewModels : IRealmCreateNewModels
    {
        private readonly Lazy<RealmCrudOrm<Trans>> _transCrudOrm;
        private readonly Lazy<RealmCrudOrm<SubTransaction>> _subTransactionCrudOrm;
        private readonly Lazy<RealmCrudOrm<Account>> _accountCrudOrm;
        private readonly Lazy<RealmCrudOrm<Payee>> _payeeCrudOrm;
        private readonly Lazy<RealmCrudOrm<Category>> _categoryCrudOrm;
        private readonly Lazy<RealmCrudOrm<Flag>> _flagCrudOrm;
        private readonly Lazy<RealmCrudOrm<BudgetEntry>> _budgetEntryCrudOrm;
        private readonly Lazy<RealmCrudOrm<DbSetting>> _dbSettingCrudOrm;
        private readonly Lazy<RealmMergeOrm> _mergeOrm;
        private readonly Lazy<UpdateBudgetCache> _updateBudgetCache;
        private readonly Lazy<RealmSubTransactionRepository> _subTransactionRepository;
        private readonly Lazy<RealmAccountOrm> _accountOrm;
        private readonly Lazy<RealmTransRepository> _transRepository;
        private readonly Lazy<RealmAccountRepository> _accountRepository;
        private readonly Lazy<RealmPayeeRepository> _payeeRepository;
        private readonly Lazy<RealmCategoryRepository> _categoryRepository;
        private readonly Lazy<RealmIncomeCategoryRepository> _incomeCategoryRepository;
        private readonly Lazy<RealmFlagRepository> _flagRepository;
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ILastSetDate _lastSetDate;

        public RealmCreateNewModels(
            Lazy<RealmCrudOrm<Trans>> transCrudOrm,
            Lazy<RealmCrudOrm<SubTransaction>> subTransactionCrudOrm,
            Lazy<RealmCrudOrm<Account>> accountCrudOrm,
            Lazy<RealmCrudOrm<Payee>> payeeCrudOrm,
            Lazy<RealmCrudOrm<Category>> categoryCrudOrm,
            Lazy<RealmCrudOrm<Flag>> flagCrudOrm,
            Lazy<RealmCrudOrm<BudgetEntry>> budgetEntryCrudOrm,
            Lazy<RealmCrudOrm<DbSetting>> dbSettingCrudOrm,
            Lazy<RealmMergeOrm> mergeOrm,
            Lazy<UpdateBudgetCache> updateBudgetCache,
            Lazy<RealmSubTransactionRepository> subTransactionRepository,
            Lazy<RealmAccountOrm> accountOrm,
            Lazy<RealmTransRepository> transRepository,
            Lazy<RealmAccountRepository> accountRepository,
            Lazy<RealmPayeeRepository> payeeRepository,
            Lazy<RealmCategoryRepository> categoryRepository,
            Lazy<RealmIncomeCategoryRepository> incomeCategoryRepository,
            Lazy<RealmFlagRepository> flagRepository,
            IRxSchedulerProvider rxSchedulerProvider,
            ILastSetDate lastSetDate)
        {
            _transCrudOrm = transCrudOrm;
            _subTransactionCrudOrm = subTransactionCrudOrm;
            _accountCrudOrm = accountCrudOrm;
            _payeeCrudOrm = payeeCrudOrm;
            _categoryCrudOrm = categoryCrudOrm;
            _flagCrudOrm = flagCrudOrm;
            _budgetEntryCrudOrm = budgetEntryCrudOrm;
            _dbSettingCrudOrm = dbSettingCrudOrm;
            _mergeOrm = mergeOrm;
            _updateBudgetCache = updateBudgetCache;
            _subTransactionRepository = subTransactionRepository;
            _accountOrm = accountOrm;
            _transRepository = transRepository;
            _accountRepository = accountRepository;
            _payeeRepository = payeeRepository;
            _categoryRepository = categoryRepository;
            _incomeCategoryRepository = incomeCategoryRepository;
            _flagRepository = flagRepository;
            _rxSchedulerProvider = rxSchedulerProvider;
            _lastSetDate = lastSetDate;
        }

        public ITransaction CreateTransaction()
        {
            return new Domain.Transaction(
                _transCrudOrm.Value, 
                _updateBudgetCache.Value,
                _rxSchedulerProvider,
                null, 
                _lastSetDate.Date,
                null,
                "",
                null,
                null,
                null,
                "",
                0L,
                false);
        }

        public ITransfer CreateTransfer()
        {
            return new Domain.Transfer(
                _transCrudOrm.Value,
                _updateBudgetCache.Value,
                _rxSchedulerProvider,
                null,
                _lastSetDate.Date,
                null,
                "",
                null,
                null,
                "",
                0L,
                false);
        }

        public IParentTransaction CreateParentTransfer()
        {
            return new Domain.ParentTransaction(
                _transCrudOrm.Value,
                _subTransactionRepository.Value,
                _rxSchedulerProvider,
                null,
                _lastSetDate.Date,
                null,
                "",
                null,
                null,
                "",
                false);
        }

        public ISubTransaction CreateSubTransaction()
        {
            return new Domain.SubTransaction(
                _subTransactionCrudOrm.Value,
                _updateBudgetCache.Value,
                _rxSchedulerProvider,
                null,
                null,
                "",
                0L);
        }

        public IAccount CreateAccount()
        {
            return new Domain.Account(
                _accountCrudOrm.Value,
                _updateBudgetCache.Value,
                _accountOrm.Value,
                _accountRepository.Value,
                _transRepository.Value,
                _rxSchedulerProvider,
                null,
                _lastSetDate.Date,
                "",
                0L);
        }

        public IPayee CreatePayee()
        {
            return new Domain.Payee(
                _payeeCrudOrm.Value,
                _mergeOrm.Value,
                _payeeRepository.Value,
                _rxSchedulerProvider,
                null,
                "");
        }

        public ICategory CreateCategory()
        {
            return new Domain.Category(
                _categoryCrudOrm.Value,
                _updateBudgetCache.Value,
                _mergeOrm.Value,
                _categoryRepository.Value,
                _rxSchedulerProvider,
                null,
                "",
                null);
        }

        public IIncomeCategory CreateIncomeCategory()
        {
            return new Domain.IncomeCategory(
                _categoryCrudOrm.Value,
                _updateBudgetCache.Value,
                _mergeOrm.Value,
                _incomeCategoryRepository.Value,
                _rxSchedulerProvider,
                null,
                "",
                0);
        }

        public IFlag CreateFlag()
        {
            return new Domain.Flag(
                _flagCrudOrm.Value,
                _mergeOrm.Value,
                _flagRepository.Value,
                _rxSchedulerProvider,
                null,
                Color.BlueViolet,
                "");
        }

        public IDbSetting CreateDbSetting()
        {
            return new Domain.DbSetting(
                _dbSettingCrudOrm.Value,
                _rxSchedulerProvider,
                null);
        }

        public IBudgetEntry CreateBudgetEntry(DateTime month, ICategory category, long budget, long outflow, long balance)
        {
            return new Domain.BudgetEntry(
                _budgetEntryCrudOrm.Value,
                _updateBudgetCache.Value,
                _transRepository.Value,
                _rxSchedulerProvider,
                null,
                month,
                category,
                budget,
                outflow,
                balance);
        }
    }
}
