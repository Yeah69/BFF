using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using BFF.Model.Native;
using BFF.Model.Native.Structure;
using Dapper;
using Dapper.Contrib.Extensions;

namespace BFF.DB.SQLite
{
    class SqLiteHelper
    {
        public static string CurrentDbName { get; set; }

        public static string CurrentDbFileName()
        {
            return $"{CurrentDbName}.sqlite";
        }

        public static string CurrentDbConnectionString()
        {
            return $"Data Source={CurrentDbName}.sqlite;Version=3;foreign keys=true;";
        }

        public static List<ITransactionLike> GetAllTransactions()
        {
            List<ITransactionLike> list = new List<ITransactionLike>();

            ClearCache();
            _cachingMode = true;
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString()))
            {
                cnn.Open();

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                string sql =
                    $@"
SELECT Id, AccountId, PayeeId, CategoryId, Date, Memo, Sum, Cleared, Type FROM [{nameof(Transaction)}s]
UNION ALL
SELECT Id, AccountId, PayeeId, CategoryId, Date, Memo, Sum, Cleared, Type FROM [{nameof(Income)}s]
UNION ALL
SELECT Id, FillerId, FromAccountId, ToAccountId, Date, Memo, Sum, Cleared, Type FROM [{nameof(Transfer)}s];";

                Type[] types =
                {
                    typeof (long), typeof (long), typeof (long), typeof (long),
                    typeof (DateTime), typeof (string), typeof (double), typeof (bool), typeof(string)
                };
                IEnumerable<TransItemBase> blah = cnn.Query<TransItemBase>(sql, types, objArr =>
                {
                    string type = (string)objArr[8];
                    DateTime date = DateTime.MinValue;
                    if (objArr[4] is DateTime)
                        date = (DateTime) objArr[4];
                    else if (!DateTime.TryParseExact((string)objArr[4], "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None, out date))
                        throw new InvalidCastException();
                    // todo: Maybe find out why in some cases the date is pre-casted to DateTime and in others it is still a string
                    TransItemBase ret;
                    switch (type)
                    {
                        case "SingleTrans":
                        case "ParentTrans":
                            ret = new Transaction
                            {
                                Id = (long)objArr[0],
                                AccountId = (long)objArr[1],
                                PayeeId = (long)objArr[2],
                                CategoryId = (long?)objArr[3],
                                Date = date,
                                Memo = (string)objArr[5],
                                Sum = (double?)objArr[6],
                                Cleared = (long)objArr[7] == 1 ? true : false,
                                Type = type
                            };
                            break;
                        case "SingleIncome":
                        case "ParentIncome":
                            ret = new Income
                            {
                                Id = (long)objArr[0],
                                AccountId = (long)objArr[1],
                                PayeeId = (long)objArr[2],
                                CategoryId = (long?)objArr[3],
                                Date = date,
                                Memo = (string)objArr[5],
                                Sum = (double?)objArr[6],
                                Cleared = (long)objArr[7] == 1 ? true : false,
                                Type = type
                            };
                            break;
                        case "Transfer":
                            ret = new Transfer
                            {
                                Id = (long)objArr[0],
                                FromAccountId = (long)objArr[2],
                                ToAccountId = (long)objArr[3],
                                Date = date,
                                Memo = (string)objArr[5],
                                Sum = (double)objArr[6],
                                Cleared = (long)objArr[7] == 1 ? true : false,
                                Type = type
                            };
                            break;
                        default:
                            ret = new Transaction { Memo = "ERROR ERROR In the custom mapping ERROR ERROR ERROR ERROR" };
                            break;
                    }
                    return ret;
                }, splitOn: "*");
                // todo: the construct above is still in idle mode. So get to work it productively

                
                IEnumerable<Transaction> transtactions = cnn.GetAll<Transaction>();
                IEnumerable<Transaction> nullSumsTransactions = transtactions?.Where(t => t.Sum == null);
                if (nullSumsTransactions != null)
                    foreach (Transaction t in nullSumsTransactions)
                    {
                        t.SubElements = GetSubTransactions(t.Id);
                    }
                IEnumerable<Income> incomes = cnn.GetAll<Income>();
                IEnumerable<Income> nullSumIncomes = incomes?.Where(i => i.Sum == null);
                if (nullSumIncomes != null)
                    foreach (Income i in nullSumIncomes)
                    {
                        i.SubElements = GetSubIncomes(i.Id);
                    }
                IEnumerable<Transfer> transfers = cnn.GetAll<Transfer>();

                list.AddRange(transtactions);
                list.AddRange(incomes);
                list.AddRange(transfers);

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
                Console.WriteLine($"The elapsed time is {elapsedTime}");

                cnn.Close();
            }
            _cachingMode = false;
            ClearCache();
            return list;
        }


        private static bool _cachingMode;

        private static void ClearCache()
        {
            AccountCache.Clear();
            CategoryCache.Clear();
            PayeeCache.Clear();
        }
        
        private static readonly Dictionary<long, Account> AccountCache = new Dictionary<long, Account>();
        public static Account GetAccount(long id)
        {
            if (_cachingMode && AccountCache.ContainsKey(id)) return AccountCache[id];
            Account ret;
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString()))
            {
                cnn.Open();

                ret = cnn.Get<Account>(id);

                cnn.Close();
            }
            AccountCache.Add(id, ret);
            return ret;
        }

        public static IEnumerable<Account> GetAllAccounts()
        {
            IEnumerable<Account> ret;

            using (var cnn = new SQLiteConnection(CurrentDbConnectionString()))
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

        private static readonly Dictionary<long, Category> CategoryCache = new Dictionary<long, Category>();
        public static Category GetCategory(long? id)
        {
            if (id == null) return null;
            if (_cachingMode && CategoryCache.ContainsKey((long)id)) return CategoryCache[(long)id];
            Category ret;
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString()))
            {
                cnn.Open();

                ret = cnn.Get<Category>(id);

                cnn.Close();
            }
            CategoryCache.Add((long)id, ret);
            return ret;
        }

        private static readonly Dictionary<long, Payee> PayeeCache = new Dictionary<long, Payee>();
        public static Payee GetPayee(long id)
        {
            if (_cachingMode && PayeeCache.ContainsKey(id)) return PayeeCache[id];
            Payee ret;
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString()))
            {
                cnn.Open();

                ret = cnn.Get<Payee>(id);

                cnn.Close();
            }
            PayeeCache.Add(id, ret);
            return ret;
        }

        public static IEnumerable<SubTransaction> GetSubTransactions(long parentId)
        {
            IEnumerable<SubTransaction> ret;
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString()))
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
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString()))
            {
                cnn.Open();

                string query = $"SELECT * FROM [{nameof(SubIncome)}s] WHERE ParentId = @id;";
                ret = cnn.Query<SubIncome>(query, new { id = parentId });

                cnn.Close();
            }
            return ret;
        }

        public static string CreateAccountTableStatement => $@"CREATE TABLE [{nameof(Account)}s](
                        {nameof(Account.Id)} INTEGER PRIMARY KEY,
                        {nameof(Account.Name)} VARCHAR(100),
                        {nameof(Account.StartingBalance)} FLOAT NOT NULL DEFAULT 0);";

        // todo: Seems not legit the Budget Table Statement
        public static string CreateBudgetTableStatement => $@"CREATE TABLE [{nameof(Budget)}s](
                        {nameof(Budget.Id)} INTEGER PRIMARY KEY,
                        {nameof(Budget.MonthYear)} DATE);";
        
        public static string CreateCategoryTableStatement => $@"CREATE TABLE [{nameof(Category)}s](
                        {nameof(Category.Id)} INTEGER PRIMARY KEY,
                        {nameof(Category.ParentId)} INTEGER,
                        {nameof(Category.Name)} VARCHAR(100),
                        FOREIGN KEY({nameof(Category.ParentId)}) REFERENCES {nameof(Category)}s({nameof(Category.Id)}) ON DELETE SET NULL);";
        
        public static string CreateIncomeTableStatement => $@"CREATE TABLE [{nameof(Income)}s](
                        {nameof(Income.Id)} INTEGER PRIMARY KEY,
                        {nameof(Income.AccountId)} INTEGER,
                        {nameof(Income.PayeeId)} INTEGER,
                        {nameof(Income.CategoryId)} INTEGER,
                        {nameof(Income.Date)} DATE,
                        {nameof(Income.Memo)} TEXT,
                        {nameof(Income.Sum)} FLOAT,
                        {nameof(Income.Cleared)} INTEGER,
                        {nameof(Income.Type)} VARCHAR(12),
                        FOREIGN KEY({ nameof(Income.AccountId)}) REFERENCES {nameof(Account)}s({ nameof(Account.Id)}) ON DELETE CASCADE,
                        FOREIGN KEY({ nameof(Income.PayeeId)}) REFERENCES {nameof(Payee)}s({ nameof(Payee.Id)}) ON DELETE SET NULL,
                        FOREIGN KEY({ nameof(Income.CategoryId)}) REFERENCES {nameof(Category)}s({ nameof(Category.Id)}) ON DELETE SET NULL);";
                
        public static string CreatePayeeTableStatement => $@"CREATE TABLE [{nameof(Payee)}s](
                        {nameof(Payee.Id)} INTEGER PRIMARY KEY,
                        {nameof(Payee.Name)} VARCHAR(100));";

        public static string CreateSubTransactionTableStatement => $@"CREATE TABLE [{nameof(SubTransaction)}s](
                        {nameof(SubTransaction.Id)} INTEGER PRIMARY KEY,
                        {nameof(SubTransaction.ParentId)} INTEGER,
                        {nameof(SubTransaction.CategoryId)} INTEGER,
                        {nameof(SubTransaction.Memo)} TEXT,
                        {nameof(SubTransaction.Sum)} FLOAT,
                        FOREIGN KEY({nameof(SubTransaction.ParentId)}) REFERENCES {nameof(Transaction)}s({nameof(Transaction.Id)}) ON DELETE CASCADE);";

        public static string CreateSubIncomeTableStatement => $@"CREATE TABLE [{nameof(SubIncome)}s](
                        {nameof(SubIncome.Id)} INTEGER PRIMARY KEY,
                        {nameof(SubIncome.ParentId)} INTEGER,
                        {nameof(SubIncome.CategoryId)} INTEGER,
                        {nameof(SubIncome.Memo)} TEXT,
                        {nameof(SubIncome.Sum)} FLOAT,
                        FOREIGN KEY({nameof(SubIncome.ParentId)}) REFERENCES {nameof(Income)}s({nameof(Income.Id)}) ON DELETE CASCADE);";

        public static string CreateTransactionTableStatement => $@"CREATE TABLE [{nameof(Transaction)}s](
                        {nameof(Transaction.Id)} INTEGER PRIMARY KEY,
                        {nameof(Transaction.AccountId)} INTEGER,
                        {nameof(Transaction.PayeeId)} INTEGER,
                        {nameof(Transaction.CategoryId)} INTEGER,
                        {nameof(Transaction.Date)} DATE,
                        {nameof(Transaction.Memo)} TEXT,
                        {nameof(Transaction.Sum)} FLOAT,
                        {nameof(Transaction.Cleared)} INTEGER,
                        {nameof(Transaction.Type)} VARCHAR(12),
                        FOREIGN KEY({ nameof(Transaction.AccountId)}) REFERENCES {nameof(Account)}s({ nameof(Account.Id)}) ON DELETE CASCADE,
                        FOREIGN KEY({ nameof(Transaction.PayeeId)}) REFERENCES {nameof(Payee)}s({ nameof(Payee.Id)}) ON DELETE SET NULL,
                        FOREIGN KEY({ nameof(Transaction.CategoryId)}) REFERENCES {nameof(Category)}s({ nameof(Category.Id)}) ON DELETE SET NULL);";

        public static string CreateTransferTableStatement => $@"CREATE TABLE [{nameof(Transfer)}s](
                        {nameof(Transfer.Id)} INTEGER PRIMARY KEY,
                        {nameof(Transfer.FillerId)} INTEGER DEFAULT -1,
                        {nameof(Transfer.FromAccountId)} INTEGER,
                        {nameof(Transfer.ToAccountId)} INTEGER,
                        {nameof(Transfer.Date)} DATE,
                        {nameof(Transfer.Memo)} TEXT,
                        {nameof(Transfer.Sum)} FLOAT,
                        {nameof(Transfer.Cleared)} INTEGER,
                        {nameof(Transfer.Type)} VARCHAR(12),
                        FOREIGN KEY({ nameof(Transfer.FromAccountId)}) REFERENCES {nameof(Account)}s({ nameof(Account.Id)}) ON DELETE RESTRICT,
                        FOREIGN KEY({ nameof(Transfer.ToAccountId)}) REFERENCES {nameof(Account)}s({ nameof(Account.Id)}) ON DELETE RESTRICT);";
    }
}
