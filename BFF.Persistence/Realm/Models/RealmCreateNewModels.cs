using System;
using System.Drawing;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM;
using BFF.Persistence.Realm.Repositories.ModelRepositories;

namespace BFF.Persistence.Realm.Models
{
    internal interface IRealmCreateNewModels : ICreateNewModels
    {
    }

    internal class RealmCreateNewModels : IRealmCreateNewModels
    {
        private readonly Lazy<RealmCrudOrm<ITransRealm>> _transCrudOrm;
        private readonly Lazy<RealmCrudOrm<ISubTransactionRealm>> _subTransactionCrudOrm;
        private readonly Lazy<RealmCrudOrm<IAccountRealm>> _accountCrudOrm;
        private readonly Lazy<RealmCrudOrm<IPayeeRealm>> _payeeCrudOrm;
        private readonly Lazy<RealmCrudOrm<ICategoryRealm>> _categoryCrudOrm;
        private readonly Lazy<RealmCrudOrm<IFlagRealm>> _flagCrudOrm;
        private readonly Lazy<RealmCrudOrm<IBudgetEntryRealm>> _budgetEntryCrudOrm;
        private readonly Lazy<RealmCrudOrm<IDbSettingRealm>> _dbSettingCrudOrm;
        private readonly Lazy<RealmMergeOrm> _mergeOrm;
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
            Lazy<RealmCrudOrm<ITransRealm>> transCrudOrm,
            Lazy<RealmCrudOrm<ISubTransactionRealm>> subTransactionCrudOrm,
            Lazy<RealmCrudOrm<IAccountRealm>> accountCrudOrm,
            Lazy<RealmCrudOrm<IPayeeRealm>> payeeCrudOrm,
            Lazy<RealmCrudOrm<ICategoryRealm>> categoryCrudOrm,
            Lazy<RealmCrudOrm<IFlagRealm>> flagCrudOrm,
            Lazy<RealmCrudOrm<IBudgetEntryRealm>> budgetEntryCrudOrm,
            Lazy<RealmCrudOrm<IDbSettingRealm>> dbSettingCrudOrm,
            Lazy<RealmMergeOrm> mergeOrm,
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
