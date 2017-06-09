using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using BFF.DB.Dapper;
using BFF.Helper.Import;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using DbTransactions = System.Transactions;
using Dapper;
using Dapper.Contrib.Extensions;
using NLog;

namespace BFF.DB.SQLite
{

    class SqLiteBffOrm : IBffOrm
    {
        private BffRepository _bffRepository;
        
        public ICommonPropertyProvider CommonPropertyProvider { get; }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public string DbPath { get; }

        protected string ConnectionString => $"Data Source={DbPath};Version=3;foreign keys=true;";

        private const string TheTitName = "The Tit";

        public void CreateNewDatabase()
        {
            Logger.Info("Creating a new Database with the path: {0}.", DbPath);
            if (File.Exists(DbPath)) //todo: This will make problems
                File.Delete(DbPath);
            SQLiteConnection.CreateFile(DbPath);

            _bffRepository.CreateTables();
        }

        public void PopulateDatabase(ImportLists importLists, ImportAssignments importAssignments)
        {
            Logger.Info("Populating the current database.");
            using (DbTransactions.TransactionScope transactionScope = new DbTransactions.TransactionScope())
            using (SQLiteConnection cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();
                /*  
                Hierarchical Category Inserting (which means that the ParentId is set right) is done automatically,
                because the structure of the imported csv-Entry of Categories allows to get the master category first and
                then the sub category. Thus, the parents id is known beforehand.
                */
                Queue<CategoryImportWrapper> categoriesOrder = new Queue<CategoryImportWrapper>(importLists.Categories);
                while (categoriesOrder.Count > 0)
                {
                    CategoryImportWrapper current = categoriesOrder.Dequeue();
                    _bffRepository.CategoryRepository.Add(current.Category as Category, cnn);
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
                    _bffRepository.PayeeRepository.Add(payee as Payee, cnn);
                    foreach (ITransIncBase transIncBase in importAssignments.PayeeToTransIncBase[payee])
                    {
                        transIncBase.PayeeId = payee.Id;
                    }
                }
                foreach (IAccount account in importLists.Accounts)
                {
                    _bffRepository.AccountRepository.Add(account as Account, cnn);
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
                    _bffRepository.TransactionRepository.Add(transaction as Transaction, cnn);
                foreach (IParentTransaction parentTransaction in importLists.ParentTransactions)
                {
                    _bffRepository.ParentTransactionRepository.Add(parentTransaction as ParentTransaction, cnn);
                    foreach (ISubTransaction subTransaction in importAssignments.ParentTransactionToSubTransaction[parentTransaction])
                    {
                        subTransaction.ParentId = parentTransaction.Id;
                    }
                }
                foreach (ISubTransaction subTransaction in importLists.SubTransactions) 
                    _bffRepository.SubTransactionRepository.Add(subTransaction as SubTransaction, cnn);
                foreach (IIncome income in importLists.Incomes) 
                    _bffRepository.IncomeRepository.Add(income as Income, cnn);
                foreach (IParentIncome parentIncome in importLists.ParentIncomes)
                {
                    _bffRepository.ParentIncomeRepository.Add(parentIncome as ParentIncome, cnn);
                    foreach (ISubIncome subIncome in importAssignments.ParentIncomeToSubIncome[parentIncome])
                    {
                        subIncome.ParentId = parentIncome.Id;
                    }
                }
                foreach (ISubIncome subIncome in importLists.SubIncomes) 
                    _bffRepository.SubIncomeRepository.Add(subIncome as SubIncome, cnn);
                foreach (ITransfer transfer in importLists.Transfers) 
                    _bffRepository.TransferRepository.Add(transfer as Transfer, cnn);
                foreach (IBudgetEntry budgetEntry in importLists.BudgetEntries) 
                    _bffRepository.BudgetEntryRepository.Add(budgetEntry as BudgetEntry, cnn);

                transactionScope.Complete();
            }
            Logger.Info("Finished populating the current database.");
        }

        public IEnumerable<ITitBase> GetAllTits(DateTime startTime, DateTime endTime, IAccount account = null)
        {
            throw new NotImplementedException();
        }

        public long? GetAccountBalance(IAccount account)
        {
            Logger.Debug("Getting account balance from {0}.", account?.Name ?? "NULL");
            long ret;

            using (DbTransactions.TransactionScope transactionScope = new DbTransactions.TransactionScope())
            using (SQLiteConnection cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();
                try
                {
                    ret = cnn.Query<long>(SqLiteQueries.AccountSpecificBalanceStatement, new { accountId = account?.Id ?? -1 }).First();
                }
                catch (OverflowException)
                {
                    return null;
                }

                transactionScope.Complete();
            }

            return ret;
        }

        public long? GetSummaryAccountBalance()
        {
            Logger.Debug("Getting account balance from SummaryAccount.");
            long ret;

            using (DbTransactions.TransactionScope transactionScope = new DbTransactions.TransactionScope())
            using (SQLiteConnection cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();
                try
                {
                    ret = cnn.Query<long>(SqLiteQueries.AllAccountsBalanceStatement).First();
                }
                catch (OverflowException)
                {
                    return null;
                }

                transactionScope.Complete();
            }

            return ret;
        }

        public IEnumerable<ISubTransInc> GetSubTransInc<T>(long parentId) where T : ISubTransInc
        {
            Logger.Debug("Getting SubTransactions/SubIncomes from table {0} with the ParentId {1}.", typeof(T).Name, parentId);
            IEnumerable<ISubTransInc> ret;
            using (DbTransactions.TransactionScope transactionScope = new DbTransactions.TransactionScope())
            using (SQLiteConnection cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();
                string query = $"SELECT * FROM [{typeof(T).Name}s] WHERE {nameof(ISubTransInc.ParentId)} = @id;";
                ret = cnn.Query<T>(query, new { id = parentId }).Cast<ISubTransInc>();

                transactionScope.Complete();
            }
            return ret;
        }

        public IEnumerable<T> GetAll<T>() where T : class, IDataModel
        {
            Logger.Debug("Getting all entries from table {0}.", typeof(T).Name);
            IEnumerable<T> ret;
            using (DbTransactions.TransactionScope transactionScope = new DbTransactions.TransactionScope())
            using (SQLiteConnection cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();
                ret = cnn.GetAll<T>();
                transactionScope.Complete();
            }
            return ret;
        }

        public void Insert<T>(T dataModelBase) where T : class, IDataModel
        {
            Logger.Debug("Insert an entry into table {0}.", typeof(T).Name);
            long ret;
            using (DbTransactions.TransactionScope transactionScope = new DbTransactions.TransactionScope())
            using (SQLiteConnection cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();
                ret = cnn.Insert(dataModelBase);
                dataModelBase.Id = ret;
                transactionScope.Complete();
            }
        }

        public T Get<T>(long id) where T : class, IDataModel
        {
            T ret;
            using (DbTransactions.TransactionScope transactionScope = new DbTransactions.TransactionScope())
            using (SQLiteConnection cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();
                ret = cnn.Get<T>(id);
                transactionScope.Complete();
            }
            return ret;
        }
        
        public void Update<T>(T dataModelBase) where T : class, IDataModel
        {
            using (DbTransactions.TransactionScope transactionScope = new DbTransactions.TransactionScope())
            using (SQLiteConnection cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();
                cnn.Update<T>(dataModelBase);
                transactionScope.Complete();
            }
        }

        public void Delete<T>(T dataModelBase) where T : class, IDataModel
        {
            Logger.Debug("Delete an entry from table {0}.", typeof(T).Name);
            using (DbTransactions.TransactionScope transactionScope = new DbTransactions.TransactionScope())
            using (SQLiteConnection cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();
                cnn.Delete(dataModelBase);
                transactionScope.Complete();
            }
        }

        public SqLiteBffOrm(string dbPath)
        {
            DbPath = dbPath;
            _bffRepository = new DapperBffRepository(new ProvideSqLiteConnection(ConnectionString));
            
            CommonPropertyProvider = new CommonPropertyProvider(this, _bffRepository);
            (CommonPropertyProvider as CommonPropertyProvider).InitializeCategoryViewModels();
        }

        public IEnumerable<T> GetPage<T>(int offset, int pageSize, object specifyingObject = null) //todo: sorting options
        {
            return _bffRepository.TitRepository.GetPage(offset, pageSize, specifyingObject as Account) as IEnumerable<T>;
        }

        public int GetCount<T>(object specifyingObject = null)
        {
            return _bffRepository.TitRepository.GetCount(specifyingObject as Account);
        }
    }
}
