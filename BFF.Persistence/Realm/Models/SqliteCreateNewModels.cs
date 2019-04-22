using System;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM;
using BFF.Persistence.Realm.Repositories.ModelRepositories;
using Account = BFF.Persistence.Realm.Models.Persistence.Account;
using Payee = BFF.Persistence.Realm.Models.Persistence.Payee;
using SubTransaction = BFF.Persistence.Realm.Models.Persistence.SubTransaction;

namespace BFF.Persistence.Realm.Models
{
    internal interface IRealmCreateNewModels : ICreateNewModels
    {
        IPayee CreatePayee();
        ICategory CreateCategory();
        IIncomeCategory CreateIncomeCategory();
        IFlag CreateFlag();
        IDbSetting CreateDbSetting();
        IBudgetEntry CreateBudgetEntry();
    }

    internal class RealmCreateNewModels : IRealmCreateNewModels
    {
        private readonly Lazy<RealmCrudOrm<ITransRealm>> _transSqlCrudOrm;
        private readonly Lazy<RealmCrudOrm<ISubTransactionRealm>> _subTransactionSqlCrudOrm;
        private readonly Lazy<RealmCrudOrm<IAccountRealm>> _accountSqlCrudOrm;
        private readonly Lazy<RealmCrudOrm<IPayeeRealm>> _payeeSqlCrudOrm;
        private readonly Lazy<RealmMergeOrm> _mergeOrm;
        private readonly Lazy<RealmSubTransactionRepository> _subTransactionRepository;
        private readonly Lazy<RealmAccountOrm> _accountOrm;
        private readonly Lazy<RealmTransRepository> _transRepository;
        private readonly Lazy<RealmPayeeRepository> _payeeRepository;
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ILastSetDate _lastSetDate;

        public RealmCreateNewModels(
            Lazy<RealmCrudOrm<ITransRealm>> transSqlCrudOrm,
            Lazy<RealmCrudOrm<ISubTransactionRealm>> subTransactionSqlCrudOrm,
            Lazy<RealmCrudOrm<IAccountRealm>> accountSqlCrudOrm,
            Lazy<RealmCrudOrm<IPayeeRealm>> payeeSqlCrudOrm,
            Lazy<RealmMergeOrm> mergeOrm,
            Lazy<RealmSubTransactionRepository> subTransactionRepository,
            Lazy<RealmAccountOrm> accountOrm,
            Lazy<RealmTransRepository> transRepository,
            Lazy<RealmPayeeRepository> payeeRepository,
            IRxSchedulerProvider rxSchedulerProvider,
            ILastSetDate lastSetDate)
        {
            _transSqlCrudOrm = transSqlCrudOrm;
            _subTransactionSqlCrudOrm = subTransactionSqlCrudOrm;
            _accountSqlCrudOrm = accountSqlCrudOrm;
            _payeeSqlCrudOrm = payeeSqlCrudOrm;
            _mergeOrm = mergeOrm;
            _subTransactionRepository = subTransactionRepository;
            _accountOrm = accountOrm;
            _transRepository = transRepository;
            _payeeRepository = payeeRepository;
            _rxSchedulerProvider = rxSchedulerProvider;
            _lastSetDate = lastSetDate;
        }

        public ITransaction CreateTransaction()
        {
            return new Realm.Models.Domain.Transaction(
                _transSqlCrudOrm.Value, 
                _rxSchedulerProvider,
                new Trans
                {
                    Date = _lastSetDate.Date,
                    Flag = null,
                    CheckNumber = "",
                    Account = null,
                    Payee = null,
                    Category = null,
                    Memo = "",
                    Sum = 0L,
                    Cleared = false
                }, 
                false,
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
            return new Realm.Models.Domain.Transfer(
                _transSqlCrudOrm.Value,
                _rxSchedulerProvider,
                new Trans
                {
                    Date = _lastSetDate.Date,
                    Flag = null,
                    CheckNumber = "",
                    FromAccount = null,
                    ToAccount = null,
                    Memo = "",
                    Sum = 0L,
                    Cleared = false
                },
                false,
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
            return new Realm.Models.Domain.ParentTransaction(
                _transSqlCrudOrm.Value,
                _subTransactionRepository.Value,
                _rxSchedulerProvider,
                new Trans
                {
                    Date = _lastSetDate.Date,
                    Flag = null,
                    CheckNumber = "",
                    Account = null,
                    Payee = null,
                    Memo = "",
                    Cleared = false
                },
                false,
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
            return new Realm.Models.Domain.SubTransaction(
                _subTransactionSqlCrudOrm.Value,
                _rxSchedulerProvider,
                new SubTransaction
                {
                    Category = null,
                    Memo = "",
                    Sum = 0L
                },
                false,
                null,
                "",
                0L);
        }

        public IAccount CreateAccount()
        {
            return new Realm.Models.Domain.Account(
                _accountSqlCrudOrm.Value,
                _accountOrm.Value,
                _transRepository.Value,
                _rxSchedulerProvider,
                new Account
                {
                    StartingDate = _lastSetDate.Date,
                    Name = "",
                    StartingBalance = 0L
                },
                false,
                _lastSetDate.Date,
                "",
                0L);
        }

        public IPayee CreatePayee()
        {
            return new Realm.Models.Domain.Payee(
                _payeeSqlCrudOrm.Value,
                _mergeOrm.Value,
                _payeeRepository.Value,
                _rxSchedulerProvider,
                new Payee
                {
                    Name = ""
                },
                false,
                "");
        }

        public ICategory CreateCategory()
        {
            throw new NotImplementedException();
        }

        public IIncomeCategory CreateIncomeCategory()
        {
            throw new NotImplementedException();
        }

        public IFlag CreateFlag()
        {
            throw new NotImplementedException();
        }

        public IDbSetting CreateDbSetting()
        {
            throw new NotImplementedException();
        }

        public IBudgetEntry CreateBudgetEntry()
        {
            throw new NotImplementedException();
        }
    }
}
