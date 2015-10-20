using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
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
            CurrentDbName = "testDatabase";
            List<ITransactionLike> list = new List<ITransactionLike>();

            ClearCache();
            _cachingMode = true;
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString()))
            {
                cnn.Open();

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                IEnumerable<Transaction> transtactions = cnn.GetAll<Transaction>();
                IEnumerable<Transaction> nullSumsTransactions = transtactions?.Where(t => t.Sum == null);
                foreach (Transaction t in nullSumsTransactions)
                {
                    t.SubElements = GetSubTransactions(t.Id);
                }
                IEnumerable<Income> incomes = cnn.GetAll<Income>();
                IEnumerable<Income> nullSumIncomes = incomes?.Where(i => i.Sum == null);
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
                        FOREIGN KEY({nameof(Income.AccountId)}) REFERENCES {nameof(Account)}s({nameof(Account.Id)}) ON DELETE CASCADE,
                        FOREIGN KEY({nameof(Income.PayeeId)}) REFERENCES {nameof(Payee)}s({nameof(Payee.Id)}) ON DELETE SET NULL,
                        FOREIGN KEY({nameof(Income.CategoryId)}) REFERENCES {nameof(Category)}s({nameof(Category.Id)}) ON DELETE SET NULL);";
        
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
                        FOREIGN KEY({nameof(Transaction.AccountId)}) REFERENCES {nameof(Account)}s({nameof(Account.Id)}) ON DELETE CASCADE,
                        FOREIGN KEY({nameof(Transaction.PayeeId)}) REFERENCES {nameof(Payee)}s({nameof(Payee.Id)}) ON DELETE SET NULL,
                        FOREIGN KEY({nameof(Transaction.CategoryId)}) REFERENCES {nameof(Category)}s({nameof(Category.Id)}) ON DELETE SET NULL);";
        
        public static string CreateTransferTableStatement => $@"CREATE TABLE [{nameof(Transfer)}s](
                        {nameof(Transfer.Id)} INTEGER PRIMARY KEY,
                        {nameof(Transfer.FromAccountId)} INTEGER,
                        {nameof(Transfer.ToAccountId)} INTEGER,
                        {nameof(Transfer.Date)} DATE,
                        {nameof(Transfer.Memo)} TEXT,
                        {nameof(Transfer.Sum)} FLOAT,
                        {nameof(Transfer.Cleared)} INTEGER,
                        FOREIGN KEY({nameof(Transfer.FromAccountId)}) REFERENCES {nameof(Account)}s({nameof(Account.Id)}) ON DELETE RESTRICT,
                        FOREIGN KEY({nameof(Transfer.ToAccountId)}) REFERENCES {nameof(Account)}s({nameof(Account.Id)}) ON DELETE RESTRICT);";
    }
}
