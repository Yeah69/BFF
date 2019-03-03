using System;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM;
using BFF.Persistence.Sql.Repositories.ModelRepositories;

namespace BFF.Persistence.Sql.Models
{
    internal class SqliteCreateNewModels : ICreateNewModels
    {
        private readonly Lazy<DapperCrudOrm<ITransSql>> _transSqlCrudOrm;
        private readonly Lazy<DapperCrudOrm<ISubTransactionSql>> _subTransactionSqlCrudOrm;
        private readonly Lazy<DapperCrudOrm<IAccountSql>> _accountSqlCrudOrm;
        private readonly Lazy<SubTransactionRepository> _subTransactionRepository;
        private readonly Lazy<DapperAccountOrm> _accountOrm;
        private readonly Lazy<TransRepository> _transRepository;
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly ILastSetDate _lastSetDate;

        public SqliteCreateNewModels(
            Lazy<DapperCrudOrm<ITransSql>> transSqlCrudOrm,
            Lazy<DapperCrudOrm<ISubTransactionSql>> subTransactionSqlCrudOrm,
            Lazy<DapperCrudOrm<IAccountSql>> accountSqlCrudOrm,
            Lazy<SubTransactionRepository> subTransactionRepository,
            Lazy<DapperAccountOrm> accountOrm,
            Lazy<TransRepository> transRepository,
            IRxSchedulerProvider rxSchedulerProvider,
            ILastSetDate lastSetDate)
        {
            _transSqlCrudOrm = transSqlCrudOrm;
            _subTransactionSqlCrudOrm = subTransactionSqlCrudOrm;
            _accountSqlCrudOrm = accountSqlCrudOrm;
            _subTransactionRepository = subTransactionRepository;
            _accountOrm = accountOrm;
            _transRepository = transRepository;
            _rxSchedulerProvider = rxSchedulerProvider;
            _lastSetDate = lastSetDate;
        }

        public ITransaction CreateTransaction()
        {
            return new Sql.Models.Domain.Transaction(
                _transSqlCrudOrm.Value, 
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
            return new Sql.Models.Domain.Transfer(
                _transSqlCrudOrm.Value,
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
            return new Sql.Models.Domain.ParentTransaction(
                _transSqlCrudOrm.Value,
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
            return new Sql.Models.Domain.SubTransaction(
                _subTransactionSqlCrudOrm.Value,
                _rxSchedulerProvider,
                0L,
                null,
                "",
                0L);
        }

        public IAccount CreateAccount()
        {
            return new Sql.Models.Domain.Account(
                _accountSqlCrudOrm.Value,
                _accountOrm.Value,
                _transRepository.Value,
                _rxSchedulerProvider,
                0L,
                _lastSetDate.Date,
                "",
                0L);
        }
    }
}
