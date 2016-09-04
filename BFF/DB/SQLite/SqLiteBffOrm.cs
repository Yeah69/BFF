using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
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
        public ICommonPropertyProvider CommonPropertyProvider { get; }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected SQLiteConnection Cnn = new SQLiteConnection();

        private readonly string _dbPath;

        public string DbPath => _dbPath;

        public static void CreateNewDatabase(string dbPath)
        {
            Logger.Info("Creating a new Database with the path: {0}.", dbPath);
            if (File.Exists(dbPath)) //todo: This will make problems
                File.Delete(dbPath);
            SQLiteConnection.CreateFile(dbPath);
            using(var cnn = new SQLiteConnection($"Data Source={dbPath};Version=3;foreign keys=true;"))
            {
                cnn.Open();
                using(var transactionScope = new DbTransactions.TransactionScope())
                {
                    cnn.Execute(SqLiteQueries.CreatePayeeTableStatement);
                    cnn.Execute(SqLiteQueries.CreateCategoryTableStatement);
                    cnn.Execute(SqLiteQueries.CreateAccountTableStatement);

                    cnn.Execute(SqLiteQueries.CreateTransactionTableStatement);
                    cnn.Execute(SqLiteQueries.CreateParentTransactionTableStatement);
                    cnn.Execute(SqLiteQueries.CreateSubTransactionTableStatement);

                    cnn.Execute(SqLiteQueries.CreateIncomeTableStatement);
                    cnn.Execute(SqLiteQueries.CreateParentIncomeTableStatement);
                    cnn.Execute(SqLiteQueries.CreateSubIncomeTableStatement);

                    cnn.Execute(SqLiteQueries.CreateTransferTableStatement);

                    cnn.Execute(SqLiteQueries.CreateDbSettingTableStatement);
                    cnn.Insert(new DbSetting {CurrencyCultrureName = "de-DE", DateCultureName = "de-DE"});

                    cnn.Execute(SqLiteQueries.SetDatabaseSchemaVersion);

                    cnn.Execute(SqLiteQueries.CreateTheTitViewStatement);

                    transactionScope.Complete();
                }
                cnn.Close();
            }
        }

        public void PopulateDatabase(ImportLists importLists, ImportAssignments importAssignments)
        {
            Logger.Info("Populating the current database.");
            DbLockFlag = true;
            using (var transactionScope = new DbTransactions.TransactionScope(DbTransactions.TransactionScopeOption.RequiresNew, new DbTransactions.TransactionOptions {IsolationLevel = DbTransactions.IsolationLevel.RepeatableRead}))
            {
                /*  
                Hierarchical Category Inserting (which means that the ParentId is set right) is done automatically,
                because the structure of the imported csv-Entry of Categories allowes to get the master category first and
                then the sub category. Thus, the parents id is known beforehand.
                */
                foreach (ICategory category in importLists.Categories)
                {
                    category.Id = Cnn.Insert(category);
                    if (importAssignments.CategoryToCategory.ContainsKey(category))
                    {
                        foreach (ICategory subCategory in importAssignments.CategoryToCategory[category])
                        {
                            subCategory.ParentId = category.Id;
                        }
                    }
                    if (importAssignments.CategoryToIHaveCategory.ContainsKey(category))
                    {
                        foreach (IHaveCategory hasCategory in importAssignments.CategoryToIHaveCategory[category])
                        {
                            hasCategory.CategoryId = category.Id;
                        }
                    }
                }
                foreach (IPayee payee in importLists.Payees)
                {
                    payee.Id = Cnn.Insert(payee);
                    foreach(ITransIncBase transIncBase in importAssignments.PayeeToTransIncBase[payee])
                    {
                        transIncBase.PayeeId = payee.Id;
                    }
                }
                foreach (IAccount account in importLists.Accounts)
                {
                    account.Id = Cnn.Insert(account);
                    foreach(ITransIncBase transIncBase in importAssignments.AccountToTransIncBase[account])
                    {
                        transIncBase.AccountId = account.Id;
                    }
                    foreach(ITransfer transfer in importAssignments.FromAccountToTransfer[account])
                    {
                        transfer.FromAccountId = account.Id;
                    }
                    foreach(ITransfer transfer in importAssignments.ToAccountToTransfer[account])
                    {
                        transfer.ToAccountId = account.Id;
                    }
                }
                foreach (ITransaction transaction in importLists.Transactions)
                    transaction.Id = Cnn.Insert(transaction);
                foreach (IParentTransaction parentTransaction in importLists.ParentTransactions)
                {
                    parentTransaction.Id = Cnn.Insert(parentTransaction);
                    foreach(ISubTransaction subTransaction in importAssignments.ParentTransactionToSubTransaction[parentTransaction])
                    {
                        subTransaction.ParentId = parentTransaction.Id;
                    }
                }
                foreach (ISubTransaction subTransaction in importLists.SubTransactions)
                    subTransaction.Id = Cnn.Insert(subTransaction);
                foreach (IIncome income in importLists.Incomes)
                    income.Id = Cnn.Insert(income);
                foreach (IParentIncome parentIncome in importLists.ParentIncomes)
                {
                    parentIncome.Id = Cnn.Insert(parentIncome);
                    foreach (ISubIncome subIncome in importAssignments.ParentIncomeToSubIncome[parentIncome])
                    {
                        subIncome.ParentId = parentIncome.Id;
                    }
                }
                foreach (ISubIncome subIncome in importLists.SubIncomes)
                    subIncome.Id = Cnn.Insert(subIncome);
                foreach (ITransfer transfer in importLists.Transfers)
                    transfer.Id = Cnn.Insert(transfer);

                transactionScope.Complete();
            }
            DbLockFlag = false;
        }

        public IEnumerable<ITitBase> GetAllTits(DateTime startDate, DateTime endDate, IAccount account = null)
        {
            Logger.Debug("Getting all TITs from {0} between {1} and {2}.", account?.Name ?? "SummaryAccount", startDate, endDate);
            DbLockFlag = true;
            IEnumerable<ITitBase> results;

            string accountAddition = $"WHERE date({nameof(ITitBase.Date)}) BETWEEN date('{startDate:yyyy-MM-dd}') AND date('{endDate:yyyy-MM-dd}') ";
            accountAddition += account == null
                ? ""
                : $"AND ({nameof(ITransIncBase.AccountId)} = @accountId OR {nameof(ITransIncBase.AccountId)} = -69 AND ({nameof(ITransIncBase.PayeeId)} = @accountId OR {nameof(ITransInc.CategoryId)} = @accountId))";
            string sql = $"SELECT * FROM [{TheTitName}] {accountAddition} ORDER BY {nameof(ITitBase.Date)};";

            Type[] types =
            {
                typeof (long), typeof (long), typeof (long), typeof (long),
                typeof (DateTime), typeof (string), typeof (long), typeof (bool), typeof(string)
            };

            using (var transactionScope = new DbTransactions.TransactionScope())
            {
                results = Cnn.Query(sql, types, _theTitMap, account == null ? null : new { accountId = account.Id }, splitOn: "*");

                transactionScope.Complete();
            }

            DbLockFlag = false;
            return results;
        }

        public long? GetAccountBalance(IAccount account)
        {
            Logger.Debug("Getting account balance from {0}.", account?.Name ?? "NULL");
            long ret;

            using (var transactionScope = new DbTransactions.TransactionScope())
            {
                try
                {
                    ret = Cnn.Query<long>(SqLiteQueries.AccountSpecificBalanceStatement, new { accountId = account?.Id ?? -1 }).First();
                }
                catch (OverflowException)
                {
                    transactionScope.Complete();
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

            using (var transactionScope = new DbTransactions.TransactionScope())
            {
                try
                {
                    ret = Cnn.Query<long>(SqLiteQueries.AllAccountsBalanceStatement).First();
                }
                catch (OverflowException)
                {
                    transactionScope.Complete();
                    return null;
                }

                transactionScope.Complete();
            }

            return ret;
        }

        public IEnumerable<T> GetSubTransInc<T>(long parentId) where T : ISubTransInc
        {
            Logger.Debug("Getting SubTransactions/SubIncomes from table {0} with the ParentId {1}.", typeof(T).Name, parentId);
            IEnumerable<T> ret;
            using (var transactionScope = new DbTransactions.TransactionScope())
            {
                string query = $"SELECT * FROM [{typeof(T).Name}s] WHERE {nameof(ISubTransInc.ParentId)} = @id;";
                ret = Cnn.Query<T>(query, new { id = parentId });

                transactionScope.Complete();
            }
            return ret;
        }

        public IEnumerable<T> GetAll<T>() where T : class, IDataModelBase
        {
            Logger.Debug("Getting all entries from table {0}.", typeof(T).Name);
            if (!DbLockFlag)
            {
                IEnumerable<T> ret;
                using (var transactionScope = new DbTransactions.TransactionScope())
                {
                    ret = Cnn.GetAll<T>();
                    transactionScope.Complete();
                }
                return ret;
            }
            return null;
        }

        public long Insert<T>(T dataModelBase) where T : class, IDataModelBase
        {
            Logger.Debug("Insert an entry into table {0}.", typeof(T).Name);
            long ret = -1L;
            if (!DbLockFlag)
            {
                using (var transactionScope = new DbTransactions.TransactionScope())
                {
                    ret = Cnn.Insert(dataModelBase);
                    dataModelBase.Id = ret;
                    transactionScope.Complete();
                }
            }
            return ret;
        }

        public T Get<T>(long id) where T : class, IDataModelBase
        {
            if (!DbLockFlag)
            {
                T ret;
                using (var transactionScope = new DbTransactions.TransactionScope())
                {
                    ret = Cnn.Get<T>(id);
                    transactionScope.Complete();
                }
                return ret;
            }
            return null;
        }
        
        public void Update<T>(T dataModelBase) where T : class, IDataModelBase
        {
            if (!DbLockFlag)
            {
                using (var transactionScope = new DbTransactions.TransactionScope())
                {
                    Cnn.Update<T>(dataModelBase);
                    transactionScope.Complete();
                }
            }
        }

        public void Delete<T>(T dataModelBase) where T : class, IDataModelBase
        {
            Logger.Debug("Delete an entry from table {0}.", typeof(T).Name);
            if (!DbLockFlag)
            {
                using (var transactionScope = new DbTransactions.TransactionScope())
                {
                    Cnn.Delete(dataModelBase);
                    transactionScope.Complete();
                }
            }
        }

        protected string ConnectionString => $"Data Source={DbPath};Version=3;foreign keys=true;";

        protected bool DbLockFlag;

        private const string TheTitName = "The Tit";

        private readonly Func<object[], ITitBase> _theTitMap = objArr =>
        {
            TitType type;
            Enum.TryParse((string)objArr[8], true, out type);
            DateTime date;
            if (objArr[4] is DateTime)
                date = (DateTime)objArr[4];
            else if (
                !DateTime.TryParseExact((string)objArr[4], "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None,
                    out date))
                throw new InvalidCastException();
            // todo: Maybe find out why in some cases the date is pre-casted to Date and in others it is still a string
            long? categoryId = (long?) objArr[3];
            ITitBase ret;
            switch (type)
            {
                case TitType.Transaction:
                    ret = new Transaction((long) objArr[0], (long) objArr[1], date, (long) objArr[2], (long) categoryId,
                                          (string) objArr[5], (long) objArr[6], (long) objArr[7] == 1);
                    break;
                case TitType.Income:
                    ret = new Income((long)objArr[0], (long)objArr[1], date, (long)objArr[2], (long)categoryId, 
                        (string)objArr[5], (long)objArr[6], (long)objArr[7] == 1);
                    break;
                case TitType.Transfer:
                    ret = new Transfer((long)objArr[0], (long)objArr[2], (long)objArr[3], date, (string)objArr[5], (long)objArr[6], (long)objArr[7] == 1);
                    break;
                case TitType.ParentTransaction:
                    ret = new ParentTransaction((long)objArr[0], (long)objArr[1], date, (long)objArr[2], (string)objArr[5], (long)objArr[7] == 1);
                    break;
                case TitType.ParentIncome:
                    ret = new ParentIncome((long)objArr[0], (long)objArr[1], date, (long)objArr[2], (string)objArr[5], (long)objArr[7] == 1);
                    break;
                default:
                    ret = new Transaction (DateTime.Today) { Memo = "ERROR ERROR In the custom mapping ERROR ERROR ERROR ERROR" };
                    break;
            }
            return ret;
        };

        public SqLiteBffOrm(string dbPath)
        {
            _dbPath = dbPath;
            if (Cnn.State != ConnectionState.Closed)
                Cnn.Close();
            if (File.Exists(DbPath))
            {
                Cnn.ConnectionString = ConnectionString;
                Cnn.Open();
            }

            CommonPropertyProvider = new CommonPropertyProvider(this);
        }

        public IEnumerable<T> GetPage<T>(int offset, int pageSize, object specifyingObject = null) //todo: sorting options
        {
            Logger.Debug("Getting a page of entries from table {0} of page size {1} by offsett {2}.", typeof(T).Name, pageSize, offset);
            DbLockFlag = true;
            IEnumerable<T> ret;
            using (var cnnTransaction = new DbTransactions.TransactionScope())
            {
                if (typeof(T) != typeof(ITitBase))
                {
                    string query = $"SELECT * FROM [{typeof(T).Name}s] LIMIT {offset}, {pageSize};";
                    ret = Cnn.Query<T>(query);
                }
                else
                {
                    string specifyingAddition = "";
                    IAccount account = specifyingObject as IAccount;
                    if(account != null && !(account is ISummaryAccount))
                        specifyingAddition = $"WHERE {nameof(ITransIncBase.AccountId)} = {account.Id} OR {nameof(ITransIncBase.AccountId)} = -69 AND ({nameof(ITransIncBase.PayeeId)} = {account.Id} OR {nameof(ITransInc.CategoryId)} = {account.Id})";
                    string query = $"SELECT * FROM [{TheTitName}] {specifyingAddition} ORDER BY {nameof(ITitBase.Date)} LIMIT {offset}, {pageSize};";
                    Type[] types =
                    {
                        typeof (long), typeof (long), typeof (long), typeof (long),
                        typeof (DateTime), typeof (string), typeof (long), typeof (bool), typeof(string)
                    };
                    ret = Cnn.Query(query, types, _theTitMap, splitOn: "*").Cast<T>();
                }
                cnnTransaction.Complete();
            }
            DbLockFlag = false;
            return ret;
        }

        public int GetCount<T>(object specifyingObject = null)
        {
            Logger.Debug("Getting the count of table {0}.", typeof(T).Name);
            int ret;
            string query;
            if(typeof(T) != typeof(ITitBase))
                query = $"SELECT COUNT(*) FROM {typeof(T).Name};";
            else
            {
                string specifyingAddition = "";
                IAccount account = specifyingObject as IAccount;
                if (account != null && !(account is ISummaryAccount))
                    specifyingAddition = $"WHERE {nameof(ITransIncBase.AccountId)} = {account.Id} OR {nameof(ITransIncBase.AccountId)} = -69 AND ({nameof(ITransIncBase.PayeeId)} = {account.Id} OR {nameof(ITransInc.CategoryId)} = {account.Id})";
                query = $"SELECT COUNT(*) FROM [{TheTitName}] {specifyingAddition};";
            }

            using (var cnnTransaction = new DbTransactions.TransactionScope())
            {
                ret = Cnn.Query<int>(query).First();
                cnnTransaction.Complete();
            }
            return ret;
        }
    }
}
