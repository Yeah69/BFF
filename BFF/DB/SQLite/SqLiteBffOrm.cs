using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using DbTransactions = System.Transactions;
using BFF.Model.Native;
using BFF.Model.Native.Structure;
using BFF.Properties;
using Dapper;
using Dapper.Contrib.Extensions;

namespace BFF.DB.SQLite
{

    class SqLiteBffOrm : IBffOrm
    {
        protected SQLiteConnection Cnn = new SQLiteConnection();

        public string DbPath {
            get { return Settings.Default.DBLocation; }
            set
            {
                Settings.Default.DBLocation = value;
                Settings.Default.Save();
                Reset();
                DbPathChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DbPath)));
            }
        }

        public event PropertyChangedEventHandler DbPathChanged;

        public void CreateNewDatabase()
        {
            if (File.Exists(DbPath)) //todo: This will make problems
                File.Delete(DbPath);
            SQLiteConnection.CreateFile(DbPath);
            using (var transactionScope = new DbTransactions.TransactionScope())
            {
                Cnn.Execute(SqLiteQueries.CreatePayeeTableStatement);
                Cnn.Execute(SqLiteQueries.CreateCategoryTableStatement);
                Cnn.Execute(SqLiteQueries.CreateAccountTableStatement);
                Cnn.Execute(SqLiteQueries.CreateTransferTableStatement);
                Cnn.Execute(SqLiteQueries.CreateTransactionTableStatement);
                Cnn.Execute(SqLiteQueries.CreateSubTransactionTableStatement);
                Cnn.Execute(SqLiteQueries.CreateIncomeTableStatement);
                Cnn.Execute(SqLiteQueries.CreateSubIncomeTableStatement);

                Cnn.Execute(SqLiteQueries.CreateDbSettingTableStatement);
                Cnn.Insert(new DbSetting { CurrencyCultrureName = "de-DE", DateCultureName = "de-DE"});

                Cnn.Execute(SqLiteQueries.SetDatabaseSchemaVersion);

                Cnn.Execute(SqLiteQueries.CreateTheTitViewStatement);

                transactionScope.Complete();
            }
        }

        public void PopulateDatabase(IEnumerable<Transaction> transactions, IEnumerable<SubTransaction> subTransactions, IEnumerable<Income> incomes, IEnumerable<SubIncome> subIncomes,
            IEnumerable<Transfer> transfers, IEnumerable<Account> accounts, IEnumerable<Payee> payees, IEnumerable<Category> categories)
        {
            _dbLockFlag = true;
            using (var transactionScope = new DbTransactions.TransactionScope())
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
            _dbLockFlag = false;
        }

        public IEnumerable<TitBase> GetAllTits(DateTime startDate, DateTime endDate, Account account = null)
        {
            _dbLockFlag = true;
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

            _dbLockFlag = false;
            return results;
        }

        public long? GetAccountBalance(Account account = null)
        {
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

        public IEnumerable<T> GetSubTransInc<T>(long parentId) where T : SubTitBase
        {
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
            if (!_dbLockFlag)
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
            long ret = -1L;
            if (!_dbLockFlag)
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
                    AllAccounts.Add(dataModelBase as Account);
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
            if (!_dbLockFlag)
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
            if (!_dbLockFlag)
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
            if (!_dbLockFlag)
            {
                using (var transactionScope = new DbTransactions.TransactionScope())
                {
                    Cnn.Delete(dataModelBase);
                    transactionScope.Complete();
                }
                if (dataModelBase is CommonProperty)
                {
                    if (dataModelBase is Account && AllAccounts.Contains(dataModelBase as Account))
                        AllAccounts.Remove(dataModelBase as Account);
                    else if (dataModelBase is Payee && AllPayees.Contains(dataModelBase as Payee))
                        AllPayees.Remove(dataModelBase as Payee);
                    else if (dataModelBase is Category && AllCategories.Contains(dataModelBase as Category))
                        AllCategories.Remove(dataModelBase as Category);
                }
            }
        }

        protected string ConnectionString => $"Data Source={DbPath};Version=3;foreign keys=true;";

        protected bool _dbLockFlag;

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
            // todo: Maybe find out why in some cases the date is pre-casted to DateTime and in others it is still a string
            long? categoryId = (long?) objArr[3];
            TitBase ret;
            switch (type)
            {
                case TitType.Transaction:
                    if (categoryId != null)
                        ret = new Transaction(date)
                        {
                            Id = (long)objArr[0],
                            AccountId = (long)objArr[1],
                            PayeeId = (long)objArr[2],
                            CategoryId = categoryId,
                            Memo = (string)objArr[5],
                            Sum = (long)objArr[6],
                            Cleared = (long)objArr[7] == 1,
                        };
                    else
                        ret = new ParentTransaction(date)
                        {
                            Id = (long)objArr[0],
                            AccountId = (long)objArr[1],
                            PayeeId = (long)objArr[2],
                            CategoryId = null,
                            Memo = (string)objArr[5],
                            Cleared = (long)objArr[7] == 1,
                        };
                    break;
                case TitType.Income:
                    if(categoryId != null)
                        ret = new Income (date)
                        {
                            Id = (long)objArr[0],
                            AccountId = (long)objArr[1],
                            PayeeId = (long)objArr[2],
                            CategoryId = categoryId,
                            Memo = (string)objArr[5],
                            Sum = (long)objArr[6],
                            Cleared = (long)objArr[7] == 1
                        };
                    else
                        ret = new ParentIncome(date)
                        {
                            Id = (long)objArr[0],
                            AccountId = (long)objArr[1],
                            PayeeId = (long)objArr[2],
                            CategoryId = categoryId,
                            Memo = (string)objArr[5],
                            Cleared = (long)objArr[7] == 1
                        };
                    break;
                case TitType.Transfer:
                    ret = new Transfer(date)
                    {
                        Id = (long)objArr[0],
                        FromAccountId = (long)objArr[2],
                        ToAccountId = (long)objArr[3],
                        Memo = (string)objArr[5],
                        Sum = (long)objArr[6],
                        Cleared = (long)objArr[7] == 1
                    };
                    break;
                default:
                    ret = new Transaction (DateTime.Today) { Memo = "ERROR ERROR In the custom mapping ERROR ERROR ERROR ERROR" };
                    break;
            }
            return ret;
        };

        public SqLiteBffOrm()
        {
            DataModelBase.Database = this;
            if(File.Exists(DbPath)) Reset();
        }

        public ObservableCollection<Account> AllAccounts { get; } = new ObservableCollection<Account>();
        public ObservableCollection<Payee> AllPayees { get; } = new ObservableCollection<Payee>();
        public ObservableCollection<Category> AllCategories { get; } = new ObservableCollection<Category>(); 

        public void Reset()
        {
            if(Cnn.State != ConnectionState.Closed)
                Cnn.Close();
            AllAccounts.Clear();
            AllPayees.Clear();
            AllCategories.Clear();
            Account.allAccounts?.Tits.Clear();
            Account.allAccounts?.NewTits.Clear();
            if (File.Exists(DbPath))
            {
                Cnn.ConnectionString = ConnectionString;
                Cnn.Open();
                foreach (Account account in GetAll<Account>())
                    AllAccounts.Add(account);
                foreach (Payee payee in GetAll<Payee>())
                    AllPayees.Add(payee);
                Dictionary<long, Category> catagoryDictionary = new Dictionary<long, Category>();
                IEnumerable<Category> categories = new List<Category>();
                using (var cnnTransaction = new DbTransactions.TransactionScope())
                {
                    //Catagories may reference itself, so a custom mapping is responsible for delaying referencing the Parent per dummy with the Parent-Id
                    //This is mandatory because the Parent may not be loaded first
                    categories = Cnn.Query($"SELECT * FROM [{nameof(Category)}s];", 
                        new[] { typeof(long), typeof(long?), typeof(string) },
                        objArray =>
                        {
                            Category ret = new Category(objArray[1] == null ? null : new Category { Id = (long)objArray[1] } /*dummy*/, (string)objArray[2]) { Id = (long)objArray[0]};
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

                Account.allAccounts?.RefreshStartingBalance(); //todo: Rather use the DbPathChanged event in AllAccounts
                Account.allAccounts?.RefreshBalance();
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

        public Account GetAccount(long id) => AllAccounts.FirstOrDefault(account => account.Id == id);
        public Payee GetPayee(long id) => AllPayees.FirstOrDefault(payee => payee.Id == id);
        public Category GetCategory(long id) => AllCategories.FirstOrDefault(category => category.Id == id);

        public IEnumerable<T> GetPage<T>(int offset, int pageSize, object specifyingObject = null) //todo: sorting options
        {
            _dbLockFlag = true;
            IEnumerable<T> ret;
            using (var cnnTransaction = new DbTransactions.TransactionScope())
            {
                if (typeof(T) != typeof(TitBase))
                {
                    string query = $"SELECT * FROM [{nameof(T)}s] LIMIT {offset}, {pageSize};";
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
            _dbLockFlag = false;
            return ret;
        }

        public int GetCount<T>(object specifyingObject = null)
        {
            int ret;
            string query;
            if(typeof(T) != typeof(TitBase))
                query = $"SELECT COUNT(*) FROM {nameof(T)};";
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
