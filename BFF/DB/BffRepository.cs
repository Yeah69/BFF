using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Transactions;
using BFF.DB.SQLite;
using BFF.Helper.Import;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using Dapper;
using Dapper.Contrib.Extensions;
using Transaction = BFF.MVVM.Models.Native.Transaction;

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
            using(TransactionScope transactionScope = new TransactionScope())
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
        private IDbTableRepository<Account> _accountRepository;
        private IDbTableRepository<BudgetEntry> _budgetEntryRepository;
        private IDbTableRepository<Category> _categoryRepository;
        private IDbTableRepository<DbSetting> _dbSettingRepository;
        private IDbTableRepository<Income> _incomeRepository;
        private IDbTableRepository<ParentIncome> _parentIncomeRepository;
        private IDbTableRepository<ParentTransaction> _parentTransactionRepository;
        private IDbTableRepository<Payee> _payeeRepository;
        private IDbTableRepository<SubIncome> _subIncomeRepository;
        private IDbTableRepository<SubTransaction> _subTransactionRepository;
        private IDbTableRepository<Transaction> _transactionRepository;
        private IDbTableRepository<Transfer> _transferRepository;
        private IViewRepository<ITitBase, Account> _titRepository;

        private IProvideConnection _provideConnection;
        
        protected BffRepository(
            IDbTableRepository<Account> accountRepository, 
            IDbTableRepository<BudgetEntry> budgetEntryRepository, 
            IDbTableRepository<Category> categoryRepository, 
            IDbTableRepository<DbSetting> dbSettingRepository, 
            IDbTableRepository<Income> incomeRepository, 
            IDbTableRepository<ParentIncome> parentIncomeRepository, 
            IDbTableRepository<ParentTransaction> parentTransactionRepository, 
            IDbTableRepository<Payee> payeeRepository, 
            IDbTableRepository<SubIncome> subIncomeRepository, 
            IDbTableRepository<SubTransaction> subTransactionRepository, 
            IDbTableRepository<Transaction> transactionRepository, 
            IDbTableRepository<Transfer> transferRepository, 
            IViewRepository<ITitBase, Account> titRepository, 
            IProvideConnection provideConnection)
        {
            _accountRepository = accountRepository;
            _budgetEntryRepository = budgetEntryRepository;
            _categoryRepository = categoryRepository;
            _dbSettingRepository = dbSettingRepository;
            _incomeRepository = incomeRepository;
            _parentIncomeRepository = parentIncomeRepository;
            _parentTransactionRepository = parentTransactionRepository;
            _payeeRepository = payeeRepository;
            _subIncomeRepository = subIncomeRepository;
            _subTransactionRepository = subTransactionRepository;
            _transactionRepository = transactionRepository;
            _transferRepository = transferRepository;
            _titRepository = titRepository;
            _provideConnection = provideConnection;
        }

        public IDbTableRepository<Account> AccountRepository => _accountRepository;
        public IDbTableRepository<BudgetEntry> BudgetEntryRepository => _budgetEntryRepository;
        public IDbTableRepository<Category> CategoryRepository => _categoryRepository;
        public IDbTableRepository<DbSetting> DbSettingRepository => _dbSettingRepository;
        public IDbTableRepository<Income> IncomeRepository => _incomeRepository;
        public IDbTableRepository<ParentIncome> ParentIncomeRepository => _parentIncomeRepository;
        public IDbTableRepository<ParentTransaction> ParentTransactionRepository => _parentTransactionRepository;
        public IDbTableRepository<Payee> PayeeRepository => _payeeRepository;
        public IDbTableRepository<SubIncome> SubIncomeRepository => _subIncomeRepository;
        public IDbTableRepository<SubTransaction> SubTransactionRepository => _subTransactionRepository;
        public IDbTableRepository<Transaction> TransactionRepository => _transactionRepository;
        public IDbTableRepository<Transfer> TransferRepository => _transferRepository;
        public IViewRepository<ITitBase, Account> TitRepository => _titRepository;

        private void executeOnExistingOrNewConnection(Action<DbConnection> action, DbConnection connection = null)
        {
            if(connection != null) action(connection);
            else
            {
                using(TransactionScope transactionScope = new TransactionScope())
                using(DbConnection newConnection = _provideConnection.Connection)
                {
                    newConnection.Open();
                    action(newConnection);
                    transactionScope.Complete();
                }
            }
        }

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
                CategoryRepository.Add(current.Category as Category, connection);
                foreach (IHaveCategory currentTitAssignment in current.TitAssignments)
                {
                    currentTitAssignment.CategoryId = current.Category.Id;
                }
                foreach (CategoryImportWrapper categoryImportWrapper in current.Categories)
                {
                    categoryImportWrapper.Category.ParentId = current.Category.Id;
                    categoriesOrder.Enqueue(categoryImportWrapper);
                }
            }
            foreach (IPayee payee in importLists.Payees)
            {
                PayeeRepository.Add(payee as Payee, connection);
                foreach (ITransIncBase transIncBase in importAssignments.PayeeToTransIncBase[payee])
                {
                    transIncBase.PayeeId = payee.Id;
                }
            }
            foreach (IAccount account in importLists.Accounts)
            {
                AccountRepository.Add(account as Account, connection);
                foreach (ITransIncBase transIncBase in importAssignments.AccountToTransIncBase[account])
                {
                    transIncBase.AccountId = account.Id;
                }
                foreach (ITransfer transfer in importAssignments.FromAccountToTransfer[account])
                {
                    transfer.FromAccountId = account.Id;
                }
                foreach (ITransfer transfer in importAssignments.ToAccountToTransfer[account])
                {
                    transfer.ToAccountId = account.Id;
                }
            }
            foreach (ITransaction transaction in importLists.Transactions)
                TransactionRepository.Add(transaction as Transaction, connection);
            foreach (IParentTransaction parentTransaction in importLists.ParentTransactions)
            {
                ParentTransactionRepository.Add(parentTransaction as ParentTransaction, connection);
                foreach (ISubTransaction subTransaction in importAssignments.ParentTransactionToSubTransaction[parentTransaction])
                {
                    subTransaction.ParentId = parentTransaction.Id;
                }
            }
            foreach (ISubTransaction subTransaction in importLists.SubTransactions) 
                SubTransactionRepository.Add(subTransaction as SubTransaction, connection);
            foreach (IIncome income in importLists.Incomes) 
                IncomeRepository.Add(income as Income, connection);
            foreach (IParentIncome parentIncome in importLists.ParentIncomes)
            {
                ParentIncomeRepository.Add(parentIncome as ParentIncome, connection);
                foreach (ISubIncome subIncome in importAssignments.ParentIncomeToSubIncome[parentIncome])
                {
                    subIncome.ParentId = parentIncome.Id;
                }
            }
            foreach (ISubIncome subIncome in importLists.SubIncomes) 
                SubIncomeRepository.Add(subIncome as SubIncome, connection);
            foreach (ITransfer transfer in importLists.Transfers) 
                TransferRepository.Add(transfer as Transfer, connection);
            foreach (IBudgetEntry budgetEntry in importLists.BudgetEntries) 
                BudgetEntryRepository.Add(budgetEntry as BudgetEntry, connection);
        }

        public void PopulateDatabase(ImportLists importLists, ImportAssignments importAssignments, DbConnection connection = null)
        {
            executeOnExistingOrNewConnection(
                conn => PopulateDatabaseInner(importLists, importAssignments, conn), 
                connection);
        }
    }
}