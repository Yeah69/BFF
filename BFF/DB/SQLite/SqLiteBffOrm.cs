using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using BFF.Model.Native;
using BFF.Model.Native.Structure;
using BFF.Properties;
using Dapper;
using Dapper.Contrib.Extensions;

namespace BFF.DB.SQLite
{
    class SqLiteBffOrm : IBffOrm
    {
        public string DbPath {
            get { return Settings.Default.DBLocation; }
            set
            {
                Settings.Default.DBLocation = value;
                Settings.Default.Save();
                //Reset();
                DbPathChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DbPath)));
            }
        }

        public event PropertyChangedEventHandler DbPathChanged;

        public void CreateNewDatabase()
        {
            if (File.Exists(DbPath))
                File.Delete(DbPath);
            SQLiteConnection.CreateFile(DbPath);
            using (var cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();

                cnn.Execute(SqLiteQueries.CreatePayeeTableStatement);
                cnn.Execute(SqLiteQueries.CreateCategoryTableStatement);
                cnn.Execute(SqLiteQueries.CreateAccountTableStatement);
                cnn.Execute(SqLiteQueries.CreateTransferTableStatement);
                cnn.Execute(SqLiteQueries.CreateTransactionTableStatement);
                cnn.Execute(SqLiteQueries.CreateSubTransactionTableStatement);
                cnn.Execute(SqLiteQueries.CreateIncomeTableStatement);
                cnn.Execute(SqLiteQueries.CreateSubIncomeTableStatement);

                cnn.Execute(SqLiteQueries.CreateDbSettingTableStatement);
                cnn.Insert(new DbSetting { CurrencyCultrureName = "de-DE", DateCultureName = "de-DE"});

                cnn.Execute($"PRAGMA user_version = {Assembly.GetExecutingAssembly().GetName().Version.Build};");

                cnn.Execute(SqLiteQueries.CreateTheTitViewStatement);

                cnn.Close();
            }
        }

        public void PopulateDatabase(IEnumerable<Transaction> transactions, IEnumerable<SubTransaction> subTransactions, IEnumerable<Income> incomes, IEnumerable<SubIncome> subIncomes,
            IEnumerable<Transfer> transfers, IEnumerable<Account> accounts, IEnumerable<Payee> payees, IEnumerable<Category> categories)
        {
            _dbLockFlag = true;
            using (var cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();

                /*  
                Hierarchical Category Inserting (which means that the ParentId is set right) is done automatically,
                because the structure of the imported csv-Entry of Categories allowes to get the master category first and
                then the sub category. Thus, the parents id is known beforehand.
                */
                foreach (Category category in categories)
                    category.Id = cnn.Insert(category);
                foreach (Payee payee in payees)
                    payee.Id = cnn.Insert(payee);
                foreach (Account account in accounts)
                    account.Id = cnn.Insert(account);
                foreach (Transaction transaction in transactions)
                    transaction.Id = cnn.Insert(transaction);
                foreach (SubTransaction subTransaction in subTransactions)
                    subTransaction.Id = cnn.Insert(subTransaction);
                foreach (Income income in incomes)
                    income.Id = cnn.Insert(income);
                foreach (SubIncome subIncome in subIncomes)
                    subIncome.Id = cnn.Insert(subIncome);
                foreach (Transfer transfer in transfers)
                    transfer.Id = cnn.Insert(transfer);

                cnn.Close();
            }
            _dbLockFlag = false;
        }

        public IEnumerable<TitBase> GetAllTits(DateTime startDate, DateTime endDate, Account account = null)
        {
            _dbLockFlag = true;
            IEnumerable<TitBase> results;

            using (var cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();

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
                results = cnn.Query(sql, types, _theTitMap, account == null ? null : new { accountId = account.Id }, splitOn: "*");

                cnn.Close();
            }

            _dbLockFlag = false;
            return results;
        }

        public IEnumerable<Category> GetAllCache()
        {
            throw new NotImplementedException();
        }

        public long? GetAccountBalance(Account account = null)
        {
            long ret;

            using (var cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();

                try
                {
                    ret = account == null
                        ? cnn.Query<long>(SqLiteQueries.AllAccountsBalanceStatement).First()
                        : cnn.Query<long>(SqLiteQueries.AccountSpecificBalanceStatement, new { accountId = account.Id }).First();
                }
                catch (OverflowException)
                {
                    return null;
                }

                cnn.Close();
            }

            return ret;
        }

        public IEnumerable<T> GetSubTransInc<T>(long parentId) where T : SubTitBase
        {
            IEnumerable<T> ret;
            using (var cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();

                string query = $"SELECT * FROM [{typeof(T).Name}s] WHERE {nameof(SubTitBase.ParentId)} = @id;";
                ret = cnn.Query<T>(query, new { id = parentId });
                //foreach (T element in ret) element.Database = this;
                cnn.Close();
            }
            return ret;
        }

        public IEnumerable<T> GetAll<T>() where T : DataModelBase
        {
            if (!_dbLockFlag)
            {
                IEnumerable<T> ret;
                using (var cnn = new SQLiteConnection(ConnectionString))
                {
                    ret = cnn.OpenAndReturn().GetAll<T>();
                    cnn.Close();
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
                using (var cnn = new SQLiteConnection(ConnectionString))
                {
                    ret = cnn.OpenAndReturn().Insert<T>(dataModelBase);
                    dataModelBase.Id = ret;
                    cnn.Close();
                }
                if (dataModelBase is CommonProperty)
                {
                    if(dataModelBase is Account)
                        AllAccounts.Add(dataModelBase as Account);
                    else if (dataModelBase is Payee)
                        AllPayees.Add(dataModelBase as Payee);
                    else if (dataModelBase is Category)
                        AllCategories.Add(dataModelBase as Category);
                }
                if (dataModelBase is TitNoTransfer)
                {
                    TitNoTransfer titNoTransfer = dataModelBase as TitNoTransfer;
                    titNoTransfer.Account?.Tits.Add(titNoTransfer);
                    Account.allAccounts?.Tits.Add(titNoTransfer);
                }
                if (dataModelBase is Transfer)
                {
                    Transfer transfer = dataModelBase as Transfer;
                    transfer.FromAccount?.Tits.Add(transfer);
                    transfer.ToAccount?.Tits.Add(transfer);
                    Account.allAccounts?.Tits.Add(transfer);
                }
            }
            return ret;
        }

        public T Get<T>(long id) where T : DataModelBase
        {
            if (!_dbLockFlag)
            {
                T ret;
                using (var cnn = new SQLiteConnection(ConnectionString))
                {
                    ret = cnn.OpenAndReturn().Get<T>(id);
                    cnn.Close();
                }
                return ret;
            }
            return null;
        }
        
        public void Update<T>(T dataModelBase) where T : DataModelBase
        {
            if (!_dbLockFlag)
            {
                using (var cnn = new SQLiteConnection(ConnectionString))
                {
                    cnn.OpenAndReturn().Update<T>(dataModelBase);
                    cnn.Close();
                }
            }
        }

        public void Delete<T>(T dataModelBase) where T : DataModelBase
        {
            if (!_dbLockFlag)
            {
                using (var cnn = new SQLiteConnection(ConnectionString))
                {
                    cnn.OpenAndReturn().Delete(dataModelBase);
                    cnn.Close();
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
                if (dataModelBase is TitNoTransfer)
                {
                    TitNoTransfer titNoTransfer = dataModelBase as TitNoTransfer;
                    if (titNoTransfer.Account?.Tits.Contains(titNoTransfer) ?? false)
                        titNoTransfer.Account.Tits.Remove(titNoTransfer);
                    if (Account.allAccounts?.Tits.Contains(titNoTransfer) ?? false)
                        Account.allAccounts.Tits.Remove(titNoTransfer);
                }
                if (dataModelBase is Transfer)
                {
                    Transfer transfer = dataModelBase as Transfer;
                    if (transfer.FromAccount?.Tits.Contains(transfer) ?? false)
                        transfer.FromAccount.Tits.Remove(transfer);
                    if (transfer.ToAccount?.Tits.Contains(transfer) ?? false)
                        transfer.ToAccount.Tits.Remove(transfer);
                    if (Account.allAccounts?.Tits.Contains(transfer) ?? false)
                        Account.allAccounts.Tits.Remove(transfer);
                }
            }
        }

        private string ConnectionString => $"Data Source={DbPath};Version=3;foreign keys=true;";

        private bool _dbLockFlag;

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
                    ((Transaction) ret).Account?.Tits.Add(ret);
                    Account.allAccounts?.Tits.Add(ret);
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
                    ((Income) ret).Account.Tits?.Add(ret);
                    Account.allAccounts?.Tits.Add(ret);
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
                    if(!(((Transfer)ret).FromAccount?.Tits.Contains(ret) ?? true)) ((Transfer)ret).FromAccount?.Tits.Add(ret);
                    if (!(((Transfer)ret).ToAccount?.Tits.Contains(ret) ?? true)) ((Transfer)ret).ToAccount?.Tits.Add(ret);
                    Account.allAccounts?.Tits.Add(ret);
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
            AllAccounts.Clear();
            AllPayees.Clear();
            AllCategories.Clear();
            Account.allAccounts?.Tits.Clear();
            Account.allAccounts?.NewTits.Clear();
            if (File.Exists(DbPath))
            {
                foreach (Account account in GetAll<Account>())
                    AllAccounts.Add(account);
                foreach (Payee payee in GetAll<Payee>())
                    AllPayees.Add(payee);
                Dictionary<long, Category> catagoryDictionary = new Dictionary<long, Category>();
                using (var cnn = new SQLiteConnection(ConnectionString))
                {
                    //Catagories may reference itself, so a custom mapping is responsible for delaying referencing the Parent per dummy with the Parent-Id
                    //This is mandatory because the Parent may not be loaded first
                    IEnumerable<Category> categories = cnn.OpenAndReturn().Query($"SELECT * FROM [{nameof(Category)}s];", 
                        new[] { typeof(long), typeof(long?), typeof(string) },
                        objArray =>
                        {
                            Category ret = new Category(objArray[1] == null ? null : new Category { Id = (long)objArray[1] } /*dummy*/, (string)objArray[2]) { Id = (long)objArray[0]};
                            catagoryDictionary.Add(ret.Id, ret);
                            return ret;
                        }, splitOn: "*");
                    //Now after every Category is loaded the Parent-Child relations are established
                    foreach (Category category in categories)
                    {
                        if (category.ParentId != null)
                        {
                            category.Parent = catagoryDictionary[(long)category.ParentId];
                            category.Parent.Categories.Add(category);
                        }
                    }
                    foreach(Category category in categories)
                        AllCategories.Add(category);

                    cnn.Close();
                }
                GetAllTits(DateTime.MinValue, DateTime.MaxValue);
                Account.allAccounts?.RefreshStartingBalance(); //todo: Rather use the DbPathChanged event in AllAccounts
                Account.allAccounts?.RefreshBalance();
            }
        }

        public Account GetAccount(long id) => AllAccounts.FirstOrDefault(account => account.Id == id);
        public Payee GetPayee(long id) => AllPayees.FirstOrDefault(payee => payee.Id == id);
        public Category GetCategory(long id) => AllCategories.FirstOrDefault(category => category.Id == id);

    }
}
