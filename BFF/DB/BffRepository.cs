using System;
using System.Data.Common;
using System.Transactions;
using BFF.DB.SQLite;
using Dapper;
using Dapper.Contrib.Extensions;
using DbSetting = BFF.DB.PersistenceModels.DbSetting;

namespace BFF.DB
{
    public abstract class CreateDatabaseBase : ICreateDatabase
    {
        protected abstract ICreateTable CreateAccountTable { get; }
        protected abstract ICreateTable CreateBudgetEntryTable { get; }
        protected abstract ICreateTable CreateCategoryTable { get; }
        protected abstract ICreateTable CreateDbSettingTable { get; }
        protected abstract ICreateTable CreatePayeeTable { get; }
        protected abstract ICreateTable CreateFlagTable { get; }
        protected abstract ICreateTable CreateSubTransactionTable { get; }
        protected abstract ICreateTable CreateTransTable { get; }
        protected abstract IProvideSqLiteConnection ProvideConnection { get; }
        
        private void CreateTablesInner(DbConnection connection)
        {
            CreateFlagTable.CreateTable(connection);
            CreatePayeeTable.CreateTable(connection);
            CreateCategoryTable.CreateTable(connection);
            CreateAccountTable.CreateTable(connection);
            
            CreateTransTable.CreateTable(connection);
            CreateSubTransactionTable.CreateTable(connection);

            CreateDbSettingTable.CreateTable(connection);
            connection.Insert(new DbSetting());

            CreateBudgetEntryTable.CreateTable(connection);

            connection.Execute(SqLiteQueries.SetDatabaseSchemaVersion);
        }
        
        public IProvideSqLiteConnection Create()
        {
            using(TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromSeconds(10)))
            using(DbConnection newConnection = ProvideConnection.Connection)
            {
                newConnection.Open();
                CreateTablesInner(newConnection);
                transactionScope.Complete();
            }
            return ProvideConnection;
        }
    }
}