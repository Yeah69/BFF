using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using BFF.Helper;
using BFF.Model.Native;
using BFF.Model.Native.Structure;
using Dapper;
using Dapper.Contrib.Extensions;
using System.IO;

namespace BFF.DB.SQLite
{
    class SqLiteHelper
    {
        private static void ExecuteOnDb(Action executeAction)
        {
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString))
            {
                cnn.Open();

                executeAction.Invoke();

                cnn.Close();
            }
        }

        private static string CurrentDbFileName { get; set; }

        private static string CurrentDbConnectionString
        {
            get { return $"Data Source={CurrentDbFileName};Version=3;foreign keys=true;"; }
        }

        private static Func<object[], TitBase> TheTitMap = objArr =>
        {
            string type = (string) objArr[8];
            DateTime date = DateTime.MinValue;
            if (objArr[4] is DateTime)
                date = (DateTime) objArr[4];
            else if (
                !DateTime.TryParseExact((string) objArr[4], "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None,
                    out date))
                throw new InvalidCastException();
            // todo: Maybe find out why in some cases the date is pre-casted to DateTime and in others it is still a string
            TitBase ret;
            switch (type)
            {
                case "SingleTrans":
                case "ParentTrans":
                    ret = new Transaction
                    {
                        Id = (long) objArr[0],
                        AccountId = (long) objArr[1],
                        PayeeId = (long) objArr[2],
                        CategoryId = (long?) objArr[3],
                        Date = date,
                        Memo = (string) objArr[5],
                        Sum = (long?) objArr[6],
                        Cleared = (long) objArr[7] == 1 ? true : false,
                        Type = type
                    };
                    break;
                case "SingleIncome":
                case "ParentIncome":
                    ret = new Income
                    {
                        Id = (long) objArr[0],
                        AccountId = (long) objArr[1],
                        PayeeId = (long) objArr[2],
                        CategoryId = (long?) objArr[3],
                        Date = date,
                        Memo = (string) objArr[5],
                        Sum = (long?) objArr[6],
                        Cleared = (long) objArr[7] == 1 ? true : false,
                        Type = type
                    };
                    break;
                case "Transfer":
                    ret = new Transfer
                    {
                        Id = (long) objArr[0],
                        FromAccountId = (long) objArr[2],
                        ToAccountId = (long) objArr[3],
                        Date = date,
                        Memo = (string) objArr[5],
                        Sum = (long) objArr[6],
                        Cleared = (long) objArr[7] == 1 ? true : false,
                        Type = type
                    };
                    break;
                default:
                    ret = new Transaction {Memo = "ERROR ERROR In the custom mapping ERROR ERROR ERROR ERROR"};
                    break;
            }
            return ret;
        };

        private static readonly string TheTitName = "The Tit";

        public static IEnumerable<TitBase> GetAllTransactions()
        {
            IEnumerable<TitBase> results;

            ClearCache();
            _cachingMode = true;
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString))
            {
                cnn.Open();

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                //Fill the caches
                AccountCache = cnn.GetAll<Account>().ToDictionary(x => x.Id);
                PayeeCache = cnn.GetAll<Payee>().ToDictionary(x => x.Id);
                CategoryCache = cnn.GetAll<Category>().ToDictionary(x => x.Id);

                string sql = $@"SELECT * FROM [{TheTitName}] ORDER BY Date;";

                Type[] types =
                {
                    typeof (long), typeof (long), typeof (long), typeof (long),
                    typeof (DateTime), typeof (string), typeof (double), typeof (bool), typeof(string)
                };
                results = cnn.Query(sql, types, TheTitMap, splitOn: "*");

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
                Console.WriteLine($"The elapsed time is {elapsedTime}");

                cnn.Close();
            }
            _cachingMode = false;
            ClearCache();
            return results;
        }

        public static IEnumerable<TitBase> GetAllTransactions(Account account)
        {
            IEnumerable<TitBase> results;

            ClearCache();
            _cachingMode = true;
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString))
            {
                cnn.Open();

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                //Fill the caches
                AccountCache = cnn.GetAll<Account>().ToDictionary(x => x.Id);
                PayeeCache = cnn.GetAll<Payee>().ToDictionary(x => x.Id);
                CategoryCache = cnn.GetAll<Category>().ToDictionary(x => x.Id);

                string sql = $@"SELECT * FROM [{TheTitName}] WHERE AccountId = @accountId OR AccountId = -1 AND (PayeeId = @accountId OR CategoryId = @accountId) ORDER BY Date;";

                Type[] types =
                {
                    typeof (long), typeof (long), typeof (long), typeof (long),
                    typeof (DateTime), typeof (string), typeof (long), typeof (bool), typeof(string)
                };
                results = cnn.Query(sql, types, TheTitMap, new {accountId = account.Id}, splitOn: "*");

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
                Console.WriteLine($"The elapsed time is {elapsedTime}");

                cnn.Close();
            }
            _cachingMode = false;
            ClearCache();
            return results;
        }

        public static void AddAccount(Account newAccount)
        {
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString))
            {
                cnn.Open();

                newAccount.Id = cnn.Insert<Account>(newAccount);

                cnn.Close();
            }
        }


        private static bool _cachingMode;

        private static void ClearCache()
        {
            AccountCache.Clear();
            CategoryCache.Clear();
            PayeeCache.Clear();
        }
        
        private static Dictionary<long, Account> AccountCache = new Dictionary<long, Account>();
        public static Account GetAccount(long id)
        {
            if (_cachingMode && AccountCache.ContainsKey(id)) return AccountCache[id];
            Account ret;
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString))
            {
                cnn.Open();

                ret = cnn.Get<Account>(id);

                cnn.Close();
            }
            if(_cachingMode) AccountCache.Add(id, ret);
            return ret;
        }

        public static IEnumerable<Account> GetAllAccounts()
        {
            IEnumerable<Account> ret;

            using (var cnn = new SQLiteConnection(CurrentDbConnectionString))
            {
                cnn.Open();

                ret = cnn.GetAll<Account>();

                if (_cachingMode)
                {
                    AccountCache.Clear();
                    foreach(Account account in ret)
                        AccountCache.Add(account.Id, account);
                }

                cnn.Close();
            }

            return ret;
        }

        private static Dictionary<long, Category> CategoryCache = new Dictionary<long, Category>();
        public static Category GetCategory(long? id)
        {
            if (id == null) return null;
            if (_cachingMode && CategoryCache.ContainsKey((long)id)) return CategoryCache[(long)id];
            Category ret;
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString))
            {
                cnn.Open();

                ret = cnn.Get<Category>(id);

                cnn.Close();
            }
            if (_cachingMode) CategoryCache.Add((long)id, ret);
            return ret;
        }

        private static Dictionary<long, Payee> PayeeCache = new Dictionary<long, Payee>();
        public static Payee GetPayee(long id)
        {
            if (_cachingMode && PayeeCache.ContainsKey(id)) return PayeeCache[id];
            Payee ret;
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString))
            {
                cnn.Open();

                ret = cnn.Get<Payee>(id);

                cnn.Close();
            }
            if (_cachingMode) PayeeCache.Add(id, ret);
            return ret;
        }

        public static IEnumerable<SubTransaction> GetSubTransactions(long parentId)
        {
            IEnumerable<SubTransaction> ret;
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString))
            {
                cnn.Open();

                string query = $"SELECT * FROM [{nameof(SubTransaction)}s] WHERE ParentId = @id;";
                ret = cnn.Query<SubTransaction>(query, new { id = parentId });

                cnn.Close();
            }
            return ret;
        }

        public static IEnumerable<SubIncome> GetSubIncomes(long parentId)
        {
            IEnumerable<SubIncome> ret;
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString))
            {
                cnn.Open();

                string query = $"SELECT * FROM [{nameof(SubIncome)}s] WHERE ParentId = @id;";
                ret = cnn.Query<SubIncome>(query, new { id = parentId });

                cnn.Close();
            }
            return ret;
        }

        public static DbSetting GetDbSetting()
        {
            DbSetting ret;
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString))
            {
                cnn.Open();

                ret = cnn.Get<DbSetting>(1);

                cnn.Close();
            }
            return ret;
        }

        public static void SetDbSetting(DbSetting dbSetting)
        {
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString))
            {
                cnn.Open();

                cnn.Update<DbSetting>(dbSetting);

                cnn.Close();
            }
        }

        public static void CreateNewDatabase(string fileName, CultureInfo currencyFormat)
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
            CurrentDbFileName = fileName;
            SQLiteConnection.CreateFile(CurrentDbFileName);
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString))
            {
                cnn.Open();
                
                cnn.Execute(CreatePayeeTableStatement);
                cnn.Execute(CreateCategoryTableStatement);
                cnn.Execute(CreateAccountTableStatement);
                cnn.Execute(CreateTransferTableStatement);
                cnn.Execute(CreateTransactionTableStatement);
                cnn.Execute(CreateSubTransactionTableStatement);
                cnn.Execute(CreateIncomeTableStatement);
                cnn.Execute(CreateSubIncomeTableStatement);

                cnn.Execute(CreateDbSettingTableStatement);
                cnn.Insert(new DbSetting {CurrencyCultrureName = currencyFormat.Name});

                cnn.Execute(CreateTheTitViewStatement);
                
                cnn.Close();
            }
        }

        public static void OpenDatabase(string fileName)
        {
            CurrentDbFileName = fileName;
        }

        public static void PopulateDatabase(List<Transaction> transactions, List<SubTransaction> subTransactions, List<Transfer> transfers, List<Income> incomes)
        {
            Output.WriteLine("Beginning to populate database.");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString))
            {
                cnn.Open();

                List<Payee> payees = Payee.GetAllCache();
                List<Category> categories = Category.GetAllCache();
                List<Account> accounts = Account.GetAllCache();

                /*  
                Hierarchical Category Inserting (which means that the ParentId is set right) is done automatically,
                because the structure of the imported csv-Entry of Categories allowes to get the master category first and
                then the sub category. Thus, the parents id is known beforehand.
                */
                categories.ForEach(category => category.Id = cnn.Insert(category));
                payees.ForEach(payee => payee.Id = cnn.Insert(payee));
                accounts.ForEach(account => account.Id = cnn.Insert(account));
                transactions.ForEach(transaction => cnn.Insert(transaction));
                cnn.Insert(subTransactions);
                cnn.Insert(transfers);
                cnn.Insert(incomes);

                cnn.Close();
            }
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
            Output.WriteLine($"End of database population. Elapsed time was: {elapsedTime}");
        }

        public static long GetAccountBalance(Account account = null)
        {
            long ret;

            using (var cnn = new SQLiteConnection(CurrentDbConnectionString))
            {
                cnn.Open();
                
                if(account == null)
                    ret = cnn.Query<long>(AllAccountsBalanceStatement).First();
                else
                    ret = cnn.Query<long>(AccountSpecificBalanceStatement, new {accountId = account?.Id}).First();

                cnn.Close();
            }

            return ret;
        }

        #region BalanceStatements

        private static string AllAccountsBalanceStatement =>
$@"SELECT Total({nameof(TitBase.Sum)}) FROM (
SELECT {nameof(TitBase.Sum)} FROM {nameof(Transaction)}s UNION ALL 
SELECT {nameof(TitBase.Sum)} FROM {nameof(Income)}s UNION ALL 
SELECT {nameof(TitBase.Sum)} FROM {nameof(SubTransaction)}s UNION ALL 
SELECT {nameof(TitBase.Sum)} FROM {nameof(SubIncome)}s UNION ALL 
SELECT {nameof(Account.StartingBalance)} FROM {nameof(Account)}s);";

        private static string AccountSpecificBalanceStatement =>
$@"SELECT (SELECT Total({nameof(TitBase.Sum)}) FROM (
SELECT {nameof(TitBase.Sum)} FROM {nameof(Transaction)}s WHERE {nameof(Transaction.AccountId)} = @accountId UNION ALL 
SELECT {nameof(TitBase.Sum)} FROM {nameof(Income)}s WHERE {nameof(Income.AccountId)} = @accountId UNION ALL
SELECT {nameof(TitBase.Sum)} FROM (SELECT {nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)}, {nameof(Transaction)}s.{nameof(Transaction.AccountId)} FROM {nameof(SubTransaction)}s INNER JOIN {nameof(Transaction)}s ON {nameof(SubTransaction)}s.{nameof(SubTransaction.ParentId)} = {nameof(Transaction)}s.{nameof(Transaction.Id)}) WHERE {nameof(Transaction.AccountId)} = @accountId UNION ALL
SELECT {nameof(TitBase.Sum)} FROM (SELECT {nameof(SubIncome)}s.{nameof(SubIncome.Sum)}, {nameof(Income)}s.{nameof(Income.AccountId)} FROM {nameof(SubIncome)}s INNER JOIN {nameof(Income)}s ON {nameof(SubIncome)}s.{nameof(SubIncome.ParentId)} = {nameof(Income)}s.{nameof(Income.Id)}) WHERE {nameof(Income.AccountId)} = @accountId UNION ALL
SELECT {nameof(TitBase.Sum)} FROM {nameof(Transfer)}s WHERE {nameof(Transfer.ToAccountId)} = @accountId UNION ALL
SELECT {nameof(Account.StartingBalance)} FROM {nameof(Account)}s WHERE {nameof(Account.Id)} = @accountId)) 
- (SELECT Total({nameof(TitBase.Sum)}) FROM {nameof(Transfer)}s WHERE {nameof(Transfer.FromAccountId)} = @accountId);";

        #endregion BalanceStatements

        #region CreateStatements

        private static string CreateTheTitViewStatement =>
                    $@"CREATE VIEW IF NOT EXISTS [{TheTitName}] AS
SELECT Id, AccountId, PayeeId, CategoryId, Date, Memo, Sum, Cleared, Type FROM [{nameof(Transaction)}s]
UNION ALL
SELECT Id, AccountId, PayeeId, CategoryId, Date, Memo, Sum, Cleared, Type FROM [{nameof(Income)}s]
UNION ALL
SELECT Id, FillerId, FromAccountId, ToAccountId, Date, Memo, Sum, Cleared, Type FROM [{nameof(Transfer)}s];";

        private static string CreateAccountTableStatement
            =>
                $@"CREATE TABLE [{nameof(Account)}s](
                        {nameof(Account.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(Account.Name)
                    } VARCHAR(100),
                        {nameof(Account.StartingBalance)
                    } INTEGER NOT NULL DEFAULT 0);";

        // todo: Seems not legit the Budget Table Statement
        private static string CreateBudgetTableStatement
            =>
                $@"CREATE TABLE [{nameof(Budget)}s](
                        {nameof(Budget.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(Budget.MonthYear)} DATE);";

        private static string CreateCategoryTableStatement
            =>
                $@"CREATE TABLE [{nameof(Category)}s](
                        {nameof(Category.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(Category.ParentId)
                    } INTEGER,
                        {nameof(Category.Name)
                    } VARCHAR(100),
                        FOREIGN KEY({nameof(Category.ParentId)}) REFERENCES {
                    nameof(Category)}s({nameof(Category.Id)}) ON DELETE SET NULL);";

        private static string CreateIncomeTableStatement
            =>
                $@"CREATE TABLE [{nameof(Income)}s](
                        {nameof(Income.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(Income.AccountId)
                    } INTEGER,
                        {nameof(Income.PayeeId)} INTEGER,
                        {
                    nameof(Income.CategoryId)} INTEGER,
                        {nameof(Income.Date)
                    } DATE,
                        {nameof(Income.Memo)} TEXT,
                        {
                    nameof(Income.Sum)} INTEGER,
                        {nameof(Income.Cleared)
                    } INTEGER,
                        {nameof(Income.Type)
                    } VARCHAR(12),
                        FOREIGN KEY({nameof(Income.AccountId)}) REFERENCES {
                    nameof(Account)}s({nameof(Account.Id)}) ON DELETE CASCADE,
                        FOREIGN KEY({
                    nameof(Income.PayeeId)}) REFERENCES {nameof(Payee)}s({nameof(Payee.Id)
                    }) ON DELETE SET NULL,
                        FOREIGN KEY({nameof(Income.CategoryId)
                    }) REFERENCES {nameof(Category)}s({nameof(Category.Id)}) ON DELETE SET NULL);";

        private static string CreatePayeeTableStatement
            =>
                $@"CREATE TABLE [{nameof(Payee)}s](
                        {nameof(Payee.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(Payee.Name)} VARCHAR(100));";

        private static string CreateSubTransactionTableStatement
            =>
                $@"CREATE TABLE [{nameof(SubTransaction)}s](
                        {nameof(SubTransaction.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(SubTransaction.ParentId)
                    } INTEGER,
                        {nameof(SubTransaction.CategoryId)
                    } INTEGER,
                        {nameof(SubTransaction.Memo)} TEXT,
                        {
                    nameof(SubTransaction.Sum)} INTEGER,
                        FOREIGN KEY({
                    nameof(SubTransaction.ParentId)}) REFERENCES {nameof(Transaction)}s({nameof(Transaction.Id)
                    }) ON DELETE CASCADE);";

        private static string CreateSubIncomeTableStatement
            =>
                $@"CREATE TABLE [{nameof(SubIncome)}s](
                        {nameof(SubIncome.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(SubIncome.ParentId)
                    } INTEGER,
                        {nameof(SubIncome.CategoryId)
                    } INTEGER,
                        {nameof(SubIncome.Memo)} TEXT,
                        {
                    nameof(SubIncome.Sum)} INTEGER,
                        FOREIGN KEY({nameof(SubIncome.ParentId)
                    }) REFERENCES {nameof(Income)}s({nameof(Income.Id)}) ON DELETE CASCADE);";

        private static string CreateTransactionTableStatement
            =>
                $@"CREATE TABLE [{nameof(Transaction)}s](
                        {nameof(Transaction.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(Transaction.AccountId)
                    } INTEGER,
                        {nameof(Transaction.PayeeId)
                    } INTEGER,
                        {nameof(Transaction.CategoryId)
                    } INTEGER,
                        {nameof(Transaction.Date)} DATE,
                        {
                    nameof(Transaction.Memo)} TEXT,
                        {nameof(Transaction.Sum)
                    } INTEGER,
                        {nameof(Transaction.Cleared)} INTEGER,
                        {
                    nameof(Transaction.Type)} VARCHAR(12),
                        FOREIGN KEY({
                    nameof(Transaction.AccountId)}) REFERENCES {nameof(Account)}s({nameof(Account.Id)
                    }) ON DELETE CASCADE,
                        FOREIGN KEY({nameof(Transaction.PayeeId)
                    }) REFERENCES {nameof(Payee)}s({nameof(Payee.Id)
                    }) ON DELETE SET NULL,
                        FOREIGN KEY({nameof(Transaction.CategoryId)
                    }) REFERENCES {nameof(Category)}s({nameof(Category.Id)}) ON DELETE SET NULL);";

        private static string CreateTransferTableStatement
            =>
                $@"CREATE TABLE [{nameof(Transfer)}s](
                        {nameof(Transfer.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(Transfer.FillerId)
                    } INTEGER DEFAULT -1,
                        {nameof(Transfer.FromAccountId)
                    } INTEGER,
                        {nameof(Transfer.ToAccountId)
                    } INTEGER,
                        {nameof(Transfer.Date)} DATE,
                        {
                    nameof(Transfer.Memo)} TEXT,
                        {nameof(Transfer.Sum)
                    } INTEGER,
                        {nameof(Transfer.Cleared)} INTEGER,
                        {
                    nameof(Transfer.Type)} VARCHAR(12),
                        FOREIGN KEY({
                    nameof(Transfer.FromAccountId)}) REFERENCES {nameof(Account)}s({nameof(Account.Id)
                    }) ON DELETE RESTRICT,
                        FOREIGN KEY({nameof(Transfer.ToAccountId)
                    }) REFERENCES {nameof(Account)}s({nameof(Account.Id)}) ON DELETE RESTRICT);";

        private static string CreateDbSettingTableStatement
            =>
                $@"CREATE TABLE [{nameof(DbSetting)}s]({nameof(DbSetting.Id)} INTEGER PRIMARY KEY, {nameof(DbSetting.CurrencyCultrureName)} VARCHAR(10));";

        #endregion

    }
}
