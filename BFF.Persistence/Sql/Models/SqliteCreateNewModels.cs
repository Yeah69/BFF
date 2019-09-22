﻿using System;
using System.Drawing;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM;
using BFF.Persistence.Sql.ORM.Interfaces;
using BFF.Persistence.Sql.Repositories.ModelRepositories;

namespace BFF.Persistence.Sql.Models
{
    internal class SqliteCreateNewModels : ICreateNewModels
    {
        private readonly Lazy<DapperCrudOrm<ITransSql>> _transCrudOrm;
        private readonly Lazy<DapperCrudOrm<ISubTransactionSql>> _subTransactionCrudOrm;
        private readonly Lazy<DapperCrudOrm<IAccountSql>> _accountCrudOrm;
        private readonly Lazy<DapperCrudOrm<IPayeeSql>> _payeeCrudOrm;
        private readonly Lazy<DapperCrudOrm<ICategorySql>> _categoryCrudOrm;
        private readonly Lazy<DapperCrudOrm<IFlagSql>> _flagCrudOrm;
        private readonly Lazy<DapperCrudOrm<IBudgetEntrySql>> _budgetEntryCrudOrm;
        private readonly Lazy<DapperCrudOrm<IDbSettingSql>> _dbSettingCrudOrm;
        private readonly Lazy<SqliteSubTransactionRepository> _subTransactionRepository;
        private readonly Lazy<DapperAccountOrm> _accountOrm;
        private readonly Lazy<SqliteTransRepository> _transRepository;
        private readonly Lazy<ISqlitePayeeRepositoryInternal> _payeeRepository;
        private readonly Lazy<ISqliteCategoryRepositoryInternal> _categoryRepository;
        private readonly Lazy<ISqliteIncomeCategoryRepositoryInternal> _incomeCategoryRepository;
        private readonly Lazy<ISqliteFlagRepositoryInternal> _flagRepository;
        private readonly Lazy<IMergeOrm> _mergeOrm;
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ILastSetDate _lastSetDate;

        public SqliteCreateNewModels(
            Lazy<DapperCrudOrm<ITransSql>> transCrudOrm,
            Lazy<DapperCrudOrm<ISubTransactionSql>> subTransactionCrudOrm,
            Lazy<DapperCrudOrm<IAccountSql>> accountCrudOrm,
            Lazy<DapperCrudOrm<IPayeeSql>> payeeCrudOrm,
            Lazy<DapperCrudOrm<ICategorySql>> categoryCrudOrm,
            Lazy<DapperCrudOrm<IFlagSql>> flagCrudOrm,
            Lazy<DapperCrudOrm<IBudgetEntrySql>> budgetEntryCrudOrm,
            Lazy<DapperCrudOrm<IDbSettingSql>> dbSettingCrudOrm,
            Lazy<SqliteSubTransactionRepository> subTransactionRepository,
            Lazy<DapperAccountOrm> accountOrm,
            Lazy<SqliteTransRepository> transRepository,
            Lazy<ISqlitePayeeRepositoryInternal> payeeRepository,
            Lazy<ISqliteCategoryRepositoryInternal> categoryRepository,
            Lazy<ISqliteIncomeCategoryRepositoryInternal> incomeCategoryRepository,
            Lazy<ISqliteFlagRepositoryInternal> flagRepository,
            Lazy<IMergeOrm> mergeOrm,
            IRxSchedulerProvider rxSchedulerProvider,
            ILastSetDate lastSetDate)
        {
            _transCrudOrm = transCrudOrm;
            _subTransactionCrudOrm = subTransactionCrudOrm;
            _accountCrudOrm = accountCrudOrm;
            _payeeCrudOrm = payeeCrudOrm;
            _categoryCrudOrm = categoryCrudOrm;
            _flagCrudOrm = flagCrudOrm;
            _dbSettingCrudOrm = dbSettingCrudOrm;
            _subTransactionRepository = subTransactionRepository;
            _accountOrm = accountOrm;
            _transRepository = transRepository;
            _payeeRepository = payeeRepository;
            _categoryRepository = categoryRepository;
            _incomeCategoryRepository = incomeCategoryRepository;
            _flagRepository = flagRepository;
            _mergeOrm = mergeOrm;
            _rxSchedulerProvider = rxSchedulerProvider;
            _lastSetDate = lastSetDate;
            _budgetEntryCrudOrm = budgetEntryCrudOrm;
        }

        public ITransaction CreateTransaction()
        {
            return new Domain.Transaction(
                _transCrudOrm.Value, 
                _rxSchedulerProvider,
                0L,
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
                0L,
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
                0L,
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
                0L,
                null,
                "",
                0L);
        }

        public IAccount CreateAccount()
        {
            return new Domain.Account(
                _accountCrudOrm.Value,
                _accountOrm.Value,
                _transRepository.Value,
                _rxSchedulerProvider,
                0L,
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
                0L,
                "");
        }

        public ICategory CreateCategory()
        {
            return new Domain.Category(
                _categoryCrudOrm.Value,
                _mergeOrm.Value,
                _categoryRepository.Value,
                _rxSchedulerProvider,
                0L,
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
                0L,
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
                0L,
                Color.BlueViolet,
                "");
        }

        public IDbSetting CreateDbSetting()
        {
            return new Domain.DbSetting(
                _dbSettingCrudOrm.Value,
                _rxSchedulerProvider);
        }

        public IBudgetEntry CreateBudgetEntry(DateTime month, ICategory category, long budget, long outflow, long balance)
        {
            return new Domain.BudgetEntry(
                _budgetEntryCrudOrm.Value,
                _transRepository.Value,
                _rxSchedulerProvider,
                0L,
                month,
                category,
                budget,
                outflow,
                balance);
        }
    }
}
