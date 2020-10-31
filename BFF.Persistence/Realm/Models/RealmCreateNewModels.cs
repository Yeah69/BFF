using System;
using System.Drawing;
using BFF.Core.Helper;
using BFF.Model;
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
        private readonly Lazy<RealmSubTransactionRepository> _subTransactionRepository;
        private readonly Lazy<RealmAccountOrm> _accountOrm;
        private readonly Lazy<RealmTransRepository> _transRepository;
        private readonly Lazy<RealmAccountRepository> _accountRepository;
        private readonly Lazy<RealmPayeeRepository> _payeeRepository;
        private readonly Lazy<RealmCategoryRepository> _categoryRepository;
        private readonly Lazy<RealmIncomeCategoryRepository> _incomeCategoryRepository;
        private readonly Lazy<RealmFlagRepository> _flagRepository;
        private readonly Lazy<RealmBudgetOrm> _budgetOrm;
        private readonly ILastSetDate _lastSetDate;
        private readonly IClearBudgetCache _clearBudgetCache;
        private readonly IUpdateBudgetCategory _updateBudgetCategory;

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
            Lazy<RealmSubTransactionRepository> subTransactionRepository,
            Lazy<RealmAccountOrm> accountOrm,
            Lazy<RealmTransRepository> transRepository,
            Lazy<RealmAccountRepository> accountRepository,
            Lazy<RealmPayeeRepository> payeeRepository,
            Lazy<RealmCategoryRepository> categoryRepository,
            Lazy<RealmIncomeCategoryRepository> incomeCategoryRepository,
            Lazy<RealmFlagRepository> flagRepository,
            Lazy<RealmBudgetOrm> budgetOrm,
            ILastSetDate lastSetDate,
            IClearBudgetCache clearBudgetCache,
            IUpdateBudgetCategory updateBudgetCategory)
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
            _subTransactionRepository = subTransactionRepository;
            _accountOrm = accountOrm;
            _transRepository = transRepository;
            _accountRepository = accountRepository;
            _payeeRepository = payeeRepository;
            _categoryRepository = categoryRepository;
            _incomeCategoryRepository = incomeCategoryRepository;
            _flagRepository = flagRepository;
            _budgetOrm = budgetOrm;
            _lastSetDate = lastSetDate;
            _clearBudgetCache = clearBudgetCache;
            _updateBudgetCategory = updateBudgetCategory;
        }

        public ITransaction CreateTransaction()
        {
            return new Domain.Transaction(
                null, 
                _lastSetDate.Date,
                null,
                "",
                null,
                null,
                null,
                "",
                0L,
                false,
                _transCrudOrm.Value);
        }

        public ITransfer CreateTransfer()
        {
            return new Domain.Transfer(
                null,
                _lastSetDate.Date,
                null,
                "",
                null,
                null,
                "",
                0L,
                false,
                _transCrudOrm.Value);
        }

        public IParentTransaction CreateParentTransfer()
        {
            return new Domain.ParentTransaction(
                null,
                _lastSetDate.Date,
                null,
                "",
                null,
                null,
                "",
                false,
                _transCrudOrm.Value,
                _subTransactionRepository.Value);
        }

        public ISubTransaction CreateSubTransaction()
        {
            return new Domain.SubTransaction(
                null,
                null,
                "",
                0L,
                _subTransactionCrudOrm.Value);
        }

        public IAccount CreateAccount()
        {
            return new Domain.Account(
                null,
                _lastSetDate.Date,
                "",
                0L,
                _accountCrudOrm.Value,
                _accountOrm.Value,
                _accountRepository.Value,
                _transRepository.Value);
        }

        public IPayee CreatePayee()
        {
            return new Domain.Payee(
                null,
                "",
                _payeeCrudOrm.Value,
                _mergeOrm.Value,
                _payeeRepository.Value);
        }

        public ICategory CreateCategory()
        {
            return new Domain.Category(
                null,
                "",
                null,
                _categoryCrudOrm.Value,
                _mergeOrm.Value,
                _categoryRepository.Value);
        }

        public IIncomeCategory CreateIncomeCategory()
        {
            return new Domain.IncomeCategory(
                null,
                "",
                0,
                _categoryCrudOrm.Value,
                _mergeOrm.Value,
                _incomeCategoryRepository.Value);
        }

        public IFlag CreateFlag()
        {
            return new Domain.Flag(
                null,
                Color.BlueViolet,
                "",
                _flagCrudOrm.Value,
                _mergeOrm.Value,
                _flagRepository.Value);
        }

        public IDbSetting CreateDbSetting()
        {
            return new Domain.DbSetting(
                null,
                _dbSettingCrudOrm.Value);
        }

        public IBudgetEntry CreateBudgetEntry(
            DateTime month, 
            ICategory category, 
            long budget, 
            long outflow, 
            long balance,
            long aggregatedBudget,
            long aggregatedOutflow,
            long aggregatedBalance)
        {
            return new Domain.BudgetEntry(
                null,
                month,
                category,
                budget,
                outflow,
                balance,
                aggregatedBudget,
                aggregatedOutflow,
                aggregatedBalance,
                _budgetEntryCrudOrm.Value,
                _budgetOrm.Value,
                _transRepository.Value,
                _clearBudgetCache,
                _updateBudgetCategory);
        }
    }
}
