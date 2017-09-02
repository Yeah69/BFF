using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Transactions;
using BFF.DB.Dapper;
using BFF.DB.Dapper.ModelRepositories;
using BFF.DB.SQLite;
using BFF.Helper.Import;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using Dapper;
using Dapper.Contrib.Extensions;

namespace BFF.DB
{
    public abstract class CreateDatabaseBase : ICreateDatabase
    {
        protected abstract ICreateTable CreateAccountTable { get; }
        protected abstract ICreateTable CreateBudgetEntryTable { get; }
        protected abstract ICreateTable CreateCategoryTable { get; }
        protected abstract ICreateTable CreateDbSettingTable { get; }
        protected abstract ICreateTable CreateIncomeTable { get; }
        protected abstract ICreateTable CreateParentIncomeTable { get; }
        protected abstract ICreateTable CreateParentTransactionTable { get; }
        protected abstract ICreateTable CreatePayeeTable { get; }
        protected abstract ICreateTable CreateSubIncomeTable { get; }
        protected abstract ICreateTable CreateSubTransactionTable { get; }
        protected abstract ICreateTable CreateTransactionTable { get; }
        protected abstract ICreateTable CreateTransferTable { get; }
        protected abstract ICreateTable CreateTitTable { get; }
        protected abstract IProvideConnection ProvideConnection { get; }
        
        private void CreateTablesInner(DbConnection connection)
        {
            CreatePayeeTable.CreateTable(connection);
            CreateCategoryTable.CreateTable(connection);
            CreateAccountTable.CreateTable(connection);

            CreateTransactionTable.CreateTable(connection);
            CreateParentTransactionTable.CreateTable(connection);
            CreateSubTransactionTable.CreateTable(connection);

            CreateIncomeTable.CreateTable(connection);
            CreateParentIncomeTable.CreateTable(connection);
            CreateSubIncomeTable.CreateTable(connection);

            CreateTransferTable.CreateTable(connection);

            CreateDbSettingTable.CreateTable(connection);
            connection.Insert(new PersistanceModels.DbSetting());

            CreateBudgetEntryTable.CreateTable(connection);
            
            CreateTitTable.CreateTable(connection);

            connection.Execute(SqLiteQueries.SetDatabaseSchemaVersion);
        }
        
        public IProvideConnection Create()
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

    public abstract class BffRepository
    {
        private readonly IProvideConnection _provideConnection;
        
        protected BffRepository(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public abstract AccountRepository AccountRepository { get; }
        public abstract BudgetEntryRepository BudgetEntryRepository { get; }
        public abstract CategoryRepository CategoryRepository { get; }
        public abstract DbSettingRepository DbSettingRepository { get; }
        public abstract IncomeRepository IncomeRepository { get; }
        public abstract ParentIncomeRepository ParentIncomeRepository { get; }
        public abstract ParentTransactionRepository ParentTransactionRepository { get; }
        public abstract PayeeRepository PayeeRepository { get; }
        public abstract SubIncomeRepository SubIncomeRepository { get; }
        public abstract SubTransactionRepository SubTransactionRepository { get; }
        public abstract TransactionRepository TransactionRepository { get; }
        public abstract TransferRepository TransferRepository { get; }
        public abstract TitRepository TitRepository { get; }

        private void PopulateDatabaseInner(ImportLists importLists, ImportAssignments importAssignments, DbConnection connection = null)
        {
            /*  
            Hierarchical Category Inserting (which means that the ParentId is set right) is done automatically,
            because the structure of the imported csv-Entry of Categories allows to get the master category first and
            then the sub category. Thus, the parents id is known beforehand.
            */
            Queue<CategoryImportWrapper> categoriesOrder = new Queue<CategoryImportWrapper>(importLists.Categories);
            while (categoriesOrder.Count > 0)
            {
                CategoryImportWrapper current = categoriesOrder.Dequeue();
                CategoryRepository.Add(current.Category, connection);
                foreach (IHaveCategory currentTitAssignment in current.TitAssignments)
                {
                    currentTitAssignment.Category = current.Category;
                }
                foreach (CategoryImportWrapper categoryImportWrapper in current.Categories)
                {
                    categoryImportWrapper.Category.Parent = current.Category;
                    categoriesOrder.Enqueue(categoryImportWrapper);
                }
            }
            foreach (IPayee payee in importLists.Payees)
            {
                PayeeRepository.Add(payee, connection);
                foreach (ITransIncBase transIncBase in importAssignments.PayeeToTransIncBase[payee])
                {
                    transIncBase.Payee = payee;
                }
            }
            foreach (IAccount account in importLists.Accounts)
            {
                AccountRepository.Add(account, connection);
                foreach (ITransIncBase transIncBase in importAssignments.AccountToTransIncBase[account])
                {
                    transIncBase.Account = account;
                }
                foreach (ITransfer transfer in importAssignments.FromAccountToTransfer[account])
                {
                    transfer.FromAccount = account;
                }
                foreach (ITransfer transfer in importAssignments.ToAccountToTransfer[account])
                {
                    transfer.ToAccount = account;
                }
            }
            foreach (ITransaction transaction in importLists.Transactions)
                TransactionRepository.Add(transaction, connection);
            foreach (IParentTransaction parentTransaction in importLists.ParentTransactions)
            {
                ParentTransactionRepository.Add(parentTransaction, connection);
                foreach (ISubTransaction subTransaction in importAssignments.ParentTransactionToSubTransaction[parentTransaction])
                {
                    subTransaction.Parent = parentTransaction;
                }
            }
            foreach (ISubTransaction subTransaction in importLists.SubTransactions) 
                SubTransactionRepository.Add(subTransaction, connection);
            foreach (IIncome income in importLists.Incomes) 
                IncomeRepository.Add(income, connection);
            foreach (IParentIncome parentIncome in importLists.ParentIncomes)
            {
                ParentIncomeRepository.Add(parentIncome, connection);
                foreach (ISubIncome subIncome in importAssignments.ParentIncomeToSubIncome[parentIncome])
                {
                    subIncome.Parent = parentIncome;
                }
            }
            foreach (ISubIncome subIncome in importLists.SubIncomes) 
                SubIncomeRepository.Add(subIncome, connection);
            foreach (ITransfer transfer in importLists.Transfers) 
                TransferRepository.Add(transfer, connection);
            foreach (IBudgetEntry budgetEntry in importLists.BudgetEntries) 
                BudgetEntryRepository.Add(budgetEntry, connection);
        }

        public void PopulateDatabase(ImportLists importLists, ImportAssignments importAssignments, DbConnection connection = null)
        {
            ConnectionHelper.ExecuteOnExistingOrNewConnection(
                conn => PopulateDatabaseInner(importLists, importAssignments, conn),
                _provideConnection, 
                connection);
        }
    }
}