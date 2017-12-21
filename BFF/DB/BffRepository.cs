using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Transactions;
using BFF.DB.Dapper;
using BFF.DB.Dapper.ModelRepositories;
using BFF.DB.SQLite;
using BFF.Helper.Import;
using BFF.DB.PersistenceModels;
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
            connection.Insert(new DbSetting());

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

    public interface IBffRepository : IDisposable
    {
        IAccountRepository AccountRepository { get; }
        IBudgetEntryRepository BudgetEntryRepository { get; }
        ICategoryRepository CategoryRepository { get; }
        IIncomeCategoryRepository IncomeCategoryRepository { get; }
        IDbSettingRepository DbSettingRepository { get; }
        IIncomeRepository IncomeRepository { get; }
        IParentIncomeRepository ParentIncomeRepository { get; }
        IParentTransactionRepository ParentTransactionRepository { get; }
        IPayeeRepository PayeeRepository { get; }
        ISubIncomeRepository SubIncomeRepository { get; }
        ISubTransactionRepository SubTransactionRepository { get; }
        ITransactionRepository TransactionRepository { get; }
        ITransferRepository TransferRepository { get; }
        ITitRepository TitRepository { get; }
        IBudgetMonthRepository BudgetMonthRepository { get; }

        void PopulateDatabase(
            ImportLists importLists, 
            ImportAssignments importAssignments,
            DbConnection connection = null);
    }

    public abstract class BffRepository : IBffRepository
    {
        private readonly IProvideConnection _provideConnection;
        
        protected BffRepository(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public abstract IAccountRepository AccountRepository { get; }
        public abstract IBudgetEntryRepository BudgetEntryRepository { get; }
        public abstract ICategoryRepository CategoryRepository { get; }
        public abstract IIncomeCategoryRepository IncomeCategoryRepository { get; }
        public abstract IDbSettingRepository DbSettingRepository { get; }
        public abstract IIncomeRepository IncomeRepository { get; }
        public abstract IParentIncomeRepository ParentIncomeRepository { get; }
        public abstract IParentTransactionRepository ParentTransactionRepository { get; }
        public abstract IPayeeRepository PayeeRepository { get; }
        public abstract ISubIncomeRepository SubIncomeRepository { get; }
        public abstract ISubTransactionRepository SubTransactionRepository { get; }
        public abstract ITransactionRepository TransactionRepository { get; }
        public abstract ITransferRepository TransferRepository { get; }
        public abstract ITitRepository TitRepository { get; }
        public abstract IBudgetMonthRepository BudgetMonthRepository { get; }

        private void PopulateDatabaseInner(ImportLists importLists, ImportAssignments importAssignments, DbConnection connection)
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
                var id = connection.Insert(current.Category);
                foreach (IHaveCategory currentTitAssignment in current.TitAssignments)
                {
                    currentTitAssignment.CategoryId = id;
                }
                foreach (CategoryImportWrapper categoryImportWrapper in current.Categories)
                {
                    categoryImportWrapper.Category.ParentId = id;
                    categoriesOrder.Enqueue(categoryImportWrapper);
                }
            }
            foreach (Payee payee in importLists.Payees)
            {
                var id = connection.Insert(payee);
                foreach (IHavePayee transIncBase in importAssignments.PayeeToTransIncBase[payee])
                {
                    transIncBase.PayeeId = id;
                }
            }
            foreach (Account account in importLists.Accounts)
            {
                var id = connection.Insert(account);
                foreach (IHaveAccount transIncBase in importAssignments.AccountToTransIncBase[account])
                {
                    transIncBase.AccountId = id;
                }
                foreach (Transfer transfer in importAssignments.FromAccountToTransfer[account])
                {
                    transfer.FromAccountId = id;
                }
                foreach (Transfer transfer in importAssignments.ToAccountToTransfer[account])
                {
                    transfer.ToAccountId = id;
                }
            }
            foreach (PersistenceModels.Transaction transaction in importLists.Transactions)
                connection.Insert(transaction);
            foreach (ParentTransaction parentTransaction in importLists.ParentTransactions)
            {
                var id = connection.Insert(parentTransaction);
                foreach (SubTransaction subTransaction in importAssignments.ParentTransactionToSubTransaction[parentTransaction])
                {
                    subTransaction.ParentId = id;
                }
            }
            foreach (SubTransaction subTransaction in importLists.SubTransactions) 
                connection.Insert(subTransaction);
            foreach (Income income in importLists.Incomes) 
                connection.Insert(income);
            foreach (ParentIncome parentIncome in importLists.ParentIncomes)
            {
                var id = connection.Insert(parentIncome);
                foreach (SubIncome subIncome in importAssignments.ParentIncomeToSubIncome[parentIncome])
                {
                    subIncome.ParentId = id;
                }
            }
            foreach (SubIncome subIncome in importLists.SubIncomes) 
                connection.Insert(subIncome);
            foreach (Transfer transfer in importLists.Transfers) 
                connection.Insert(transfer);
            foreach (BudgetEntry budgetEntry in importLists.BudgetEntries) 
                connection.Insert(budgetEntry);
        }

        public void PopulateDatabase(ImportLists importLists, ImportAssignments importAssignments, DbConnection connection = null)
        {
            ConnectionHelper.ExecuteOnExistingOrNewConnection(
                conn => PopulateDatabaseInner(importLists, importAssignments, conn),
                _provideConnection, 
                connection);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}