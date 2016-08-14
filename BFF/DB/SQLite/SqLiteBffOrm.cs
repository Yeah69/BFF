﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
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
                    cnn.Execute(SqLiteQueries.CreateTransferTableStatement);
                    cnn.Execute(SqLiteQueries.CreateTransactionTableStatement);
                    cnn.Execute(SqLiteQueries.CreateSubTransactionTableStatement);
                    cnn.Execute(SqLiteQueries.CreateIncomeTableStatement);
                    cnn.Execute(SqLiteQueries.CreateSubIncomeTableStatement);

                    cnn.Execute(SqLiteQueries.CreateDbSettingTableStatement);
                    cnn.Insert(new DbSetting {CurrencyCultrureName = "de-DE", DateCultureName = "de-DE"});

                    cnn.Execute(SqLiteQueries.SetDatabaseSchemaVersion);

                    cnn.Execute(SqLiteQueries.CreateTheTitViewStatement);

                    transactionScope.Complete();
                }
                cnn.Close();
            }
        }

        public void PopulateDatabase(IEnumerable<Transaction> transactions, IEnumerable<SubTransaction> subTransactions, IEnumerable<Income> incomes, IEnumerable<SubIncome> subIncomes,
            IEnumerable<Transfer> transfers, IEnumerable<Account> accounts, IEnumerable<Payee> payees, IEnumerable<Category> categories)
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
                foreach (Category category in categories)
                    category.Id = Cnn.Insert(category);
                foreach (Payee payee in payees)
                    payee.Id = Cnn.Insert(payee);
                foreach (Account account in accounts)
                    account.Id = Cnn.Insert(account);
                foreach (Transaction transaction in transactions)
                    transaction.Id = Cnn.Insert(transaction);
                foreach (SubTransaction subTransaction in subTransactions)
                    subTransaction.Id = Cnn.Insert(subTransaction);
                foreach (Income income in incomes)
                    income.Id = Cnn.Insert(income);
                foreach (SubIncome subIncome in subIncomes)
                    subIncome.Id = Cnn.Insert(subIncome);
                foreach (Transfer transfer in transfers)
                    transfer.Id = Cnn.Insert(transfer);

                transactionScope.Complete();
            }
            DbLockFlag = false;
        }

        public IEnumerable<TitBase> GetAllTits(DateTime startDate, DateTime endDate, Account account = null)
        {
            Logger.Debug("Getting all TITs from {0} between {1} and {2}.", account?.Name ?? "AllAccounts", startDate, endDate);
            DbLockFlag = true;
            IEnumerable<TitBase> results;

            string accountAddition = $"WHERE date({nameof(TitBase.Date)}) BETWEEN date('{startDate.ToString("yyyy-MM-dd")}') AND date('{endDate.ToString("yyyy-MM-dd")}') ";
            accountAddition += account == null
                ? ""
                : $"AND ({nameof(TitNoTransfer.AccountId)} = @accountId OR {nameof(TitNoTransfer.AccountId)} = -69 AND ({nameof(TitNoTransfer.PayeeId)} = @accountId OR {nameof(TitNoTransfer.CategoryId)} = @accountId))";
            string sql = $"SELECT * FROM [{TheTitName}] {accountAddition} ORDER BY {nameof(TitBase.Date)};";

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

        public long? GetAccountBalance(Account account = null)
        {
            Logger.Debug("Getting account balance from {0}.", account?.Name ?? "AllAccounts");
            long ret;

            using (var transactionScope = new DbTransactions.TransactionScope())
            {
                try
                {
                    ret = account == null
                        ? Cnn.Query<long>(SqLiteQueries.AllAccountsBalanceStatement).First()
                        : Cnn.Query<long>(SqLiteQueries.AccountSpecificBalanceStatement, new { accountId = account.Id }).First();
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
                string query = $"SELECT * FROM [{typeof(T).Name}s] WHERE {nameof(SubTitBase.ParentId)} = @id;";
                ret = Cnn.Query<T>(query, new { id = parentId });

                transactionScope.Complete();
            }
            return ret;
        }

        public IEnumerable<T> GetAll<T>() where T : DataModelBase
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

        public long Insert<T>(T dataModelBase) where T : DataModelBase
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
                ManageIfPeriphery(dataModelBase);
            }
            return ret;
        }

        protected void ManageIfPeriphery<T>(T dataModelBase) where T : DataModelBase
        {
            if (dataModelBase is CommonProperty)
            {
                if (dataModelBase is Account)
                    CommonPropertyProvider.Add(dataModelBase as Account);
                else if (dataModelBase is Payee)
                    AllPayees.Add(dataModelBase as Payee);
                else if (dataModelBase is Category)
                {
                    Category newCategory = dataModelBase as Category;
                    if (newCategory.Parent == null)
                        AllCategories.Add(newCategory);
                    else
                    {
                        int index = newCategory.Parent.Categories.Count - 2;
                        index = index == -1 ? AllCategories.IndexOf(newCategory.Parent) + 1 :
                            AllCategories.IndexOf(newCategory.Parent.Categories[index]) + 1;
                        AllCategories.Insert(index, newCategory);
                    }
                }
            }
        }

        public T Get<T>(long id) where T : DataModelBase
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
        
        public void Update<T>(T dataModelBase) where T : DataModelBase
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

        public void Delete<T>(T dataModelBase) where T : DataModelBase
        {
            Logger.Debug("Delete an entry from table {0}.", typeof(T).Name);
            if (!DbLockFlag)
            {
                using (var transactionScope = new DbTransactions.TransactionScope())
                {
                    Cnn.Delete(dataModelBase);
                    transactionScope.Complete();
                }
                if (dataModelBase is CommonProperty)
                {
                    if (dataModelBase is Account && CommonPropertyProvider.Accounts.Contains(dataModelBase as Account))
                        CommonPropertyProvider.Remove(dataModelBase as Account);
                    else if (dataModelBase is Payee && AllPayees.Contains(dataModelBase as Payee))
                        AllPayees.Remove(dataModelBase as Payee);
                    else if (dataModelBase is Category && AllCategories.Contains(dataModelBase as Category))
                        AllCategories.Remove(dataModelBase as Category);
                }
            }
        }

        protected string ConnectionString => $"Data Source={DbPath};Version=3;foreign keys=true;";

        protected bool DbLockFlag;

        private const string TheTitName = "The Tit";

        private readonly Func<object[], TitBase> _theTitMap = objArr =>
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
            TitBase ret;
            switch (type)
            {
                case TitType.Transaction:
                    ret = categoryId != null ? new Transaction((long)objArr[0], (long)objArr[1], date, (long)objArr[2], (long)categoryId, (string)objArr[5], (long)objArr[6], (long)objArr[7] == 1) : new ParentTransaction((long)objArr[0], (long)objArr[1], date, (long)objArr[2], categoryId ?? -1, (string)objArr[5], (long)objArr[6], (long)objArr[7] == 1);
                    break;
                case TitType.Income:
                    if(categoryId != null)
                        ret = new Income((long)objArr[0], (long)objArr[1], date, (long)objArr[2], (long)categoryId, (string)objArr[5], (long)objArr[6], (long)objArr[7] == 1);
                    else
                        ret = new ParentIncome((long)objArr[0], (long)objArr[1], date, (long)objArr[2], categoryId ?? -1, (string)objArr[5], (long)objArr[6], (long)objArr[7] == 1);
                    break;
                case TitType.Transfer:
                    ret = new Transfer((long)objArr[0], (long)objArr[2], (long)objArr[3], date, (string)objArr[5], (long)objArr[6], (long)objArr[7] == 1);
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
            AllPayees.Clear();
            AllCategories.Clear();
            if (File.Exists(DbPath))
            {
                Cnn.ConnectionString = ConnectionString;
                Cnn.Open();
                InitializePeripherie();
            }

            CommonPropertyProvider = new CommonPropertyProvider(this);

            //Old
            DataModelBase.Database = this;
            if(File.Exists(DbPath)) Reset();
        }

        public ObservableCollection<Payee> AllPayees { get; } = new ObservableCollection<Payee>();
        public ObservableCollection<Category> AllCategories { get; } = new ObservableCollection<Category>(); 

        public void Reset()
        {
            
        }

        private void InitializePeripherie()
        {
            foreach (Payee payee in GetAll<Payee>())
                AllPayees.Add(payee);
            Dictionary<long, Category> catagoryDictionary = new Dictionary<long, Category>();
            IEnumerable<Category> categories;
            using (var cnnTransaction = new DbTransactions.TransactionScope())
            {
                //Catagories may reference itself, so a custom mapping is responsible for delaying referencing the Parent per dummy with the Parent-Id
                //This is mandatory because the Parent may not be loaded first
                categories = Cnn.Query($"SELECT * FROM [{nameof(Category)}s];",
                    new[] { typeof(long), typeof(long?), typeof(string) },
                    objArray =>
                    {
                        Category ret = new Category(objArray[1] == null ? null : new Category { Id = (long)objArray[1] } /*dummy*/, (string)objArray[2]) { Id = (long)objArray[0] };
                        catagoryDictionary.Add(ret.Id, ret);
                        return ret;
                    }, splitOn: "*");
                cnnTransaction.Complete();
            }
            //Now after every Category is loaded the Parent-Child relations are established
            List<Category> parentCategories = new List<Category>();
            foreach (Category category in categories)
            {
                if (category.ParentId != null)
                {
                    category.Parent = catagoryDictionary[(long)category.ParentId];
                    category.Parent.Categories.Add(category);
                }
                else parentCategories.Add(category);
            }
            foreach (Category parentCategory in parentCategories)
            {
                AllCategories.Add(parentCategory);
                FillCategoryWithDescandents(parentCategory, AllCategories);
            }
        }

        private void FillCategoryWithDescandents(Category parentCategory, IList<Category> list)
        {
            foreach (Category childCategory in parentCategory.Categories)
            {
                list.Add(childCategory);
                FillCategoryWithDescandents(childCategory, list);
            }
        }

        public Payee GetPayee(long id) => AllPayees.FirstOrDefault(payee => payee.Id == id);
        public Category GetCategory(long id) => AllCategories.FirstOrDefault(category => category.Id == id);

        public IEnumerable<T> GetPage<T>(int offset, int pageSize, object specifyingObject = null) //todo: sorting options
        {
            Logger.Debug("Getting a page of entries from table {0} of page size {1} by offsett {2}.", typeof(T).Name, pageSize, offset);
            DbLockFlag = true;
            IEnumerable<T> ret;
            using (var cnnTransaction = new DbTransactions.TransactionScope())
            {
                if (typeof(T) != typeof(TitBase))
                {
                    string query = $"SELECT * FROM [{typeof(T).Name}s] LIMIT {offset}, {pageSize};";
                    ret = Cnn.Query<T>(query);
                }
                else
                {
                    string specifyingAddition = "";
                    Account account = specifyingObject as Account;
                    if(account != null && !(account is AllAccounts))
                        specifyingAddition = $"WHERE {nameof(TitNoTransfer.AccountId)} = {account.Id} OR {nameof(TitNoTransfer.AccountId)} = -69 AND ({nameof(TitNoTransfer.PayeeId)} = {account.Id} OR {nameof(TitNoTransfer.CategoryId)} = {account.Id})";
                    string query = $"SELECT * FROM [{TheTitName}] {specifyingAddition} ORDER BY {nameof(TitBase.Date)} LIMIT {offset}, {pageSize};";
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
            if(typeof(T) != typeof(TitBase))
                query = $"SELECT COUNT(*) FROM {typeof(T).Name};";
            else
            {
                string specifyingAddition = "";
                Account account = specifyingObject as Account;
                if (account != null && !(account is AllAccounts))
                    specifyingAddition = $"WHERE {nameof(TitNoTransfer.AccountId)} = {account.Id} OR {nameof(TitNoTransfer.AccountId)} = -69 AND ({nameof(TitNoTransfer.PayeeId)} = {account.Id} OR {nameof(TitNoTransfer.CategoryId)} = {account.Id})";
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
