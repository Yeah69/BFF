using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using BFF.Model.Native;
using BFF.Model.Native.Structure;
using Dapper;
using Dapper.Contrib.Extensions;

namespace BFF.DB.SQLite
{
    class SqLiteDb : IDb
    {
        public string DbPath { get; }

        public void CreateNewDatabase()
        {
            if (File.Exists(DbPath))
                File.Delete(DbPath);
            SQLiteConnection.CreateFile(DbPath);
            using (var cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();

                cnn.Execute(SqLiteHelper.CreatePayeeTableStatement);
                cnn.Execute(SqLiteHelper.CreateCategoryTableStatement);
                cnn.Execute(SqLiteHelper.CreateAccountTableStatement);
                cnn.Execute(SqLiteHelper.CreateTransferTableStatement);
                cnn.Execute(SqLiteHelper.CreateTransactionTableStatement);
                cnn.Execute(SqLiteHelper.CreateSubTransactionTableStatement);
                cnn.Execute(SqLiteHelper.CreateIncomeTableStatement);
                cnn.Execute(SqLiteHelper.CreateSubIncomeTableStatement);

                cnn.Execute(SqLiteHelper.CreateDbSettingTableStatement);
                cnn.Insert(new DbSetting { CurrencyCultrureName = "en-US", DateCultureName = "en-US"});

                cnn.Execute(SqLiteHelper.CreateTheTitViewStatement);

                cnn.Close();
            }
        }

        public void OpenDatabase()
        {
            throw new NotImplementedException();
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
                cnn.Insert(categories);
                cnn.Insert(payees);
                cnn.Insert(accounts);
                cnn.Insert(transactions);
                cnn.Insert(subTransactions);
                cnn.Insert(incomes);
                cnn.Insert(subIncomes);
                cnn.Insert(transfers);

                cnn.Close();
            }
            _dbLockFlag = false;
        }

        public IEnumerable<TitBase> GetAllTits(Account account = null)
        {
            _dbLockFlag = true;
            IEnumerable<TitBase> results;

            using (var cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();

                string accountAddition = (account == null)
                    ? ""
                    : "WHERE AccountId = @accountId OR AccountId = -1 AND (PayeeId = @accountId OR CategoryId = @accountId)";
                string sql = $"SELECT * FROM [{TheTitName}] {accountAddition} ORDER BY Date;";

                Type[] types =
                {
                    typeof (long), typeof (long), typeof (long), typeof (long),
                    typeof (DateTime), typeof (string), typeof (long), typeof (bool), typeof(string)
                };
                results = cnn.Query(sql, types, _theTitMap, (account == null) ? null : new { accountId = account.Id }, splitOn: "*");
                
                cnn.Close();
            }

            _dbLockFlag = false;
            return results;
        }

        public long GetAccountBalance(Account account = null)
        {
            long ret;

            using (var cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();

                ret = account == null 
                    ? cnn.Query<long>(SqLiteHelper.AllAccountsBalanceStatement).First() 
                    : cnn.Query<long>(SqLiteHelper.AccountSpecificBalanceStatement, new { accountId = account.Id }).First();

                cnn.Close();
            }

            return ret;
        }

        public IEnumerable<T> GetSubTransInc<T>(long parentId) where T : SubTransInc
        {
            IEnumerable<T> ret;
            using (var cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();

                string query = $"SELECT * FROM [{nameof(T)}s] WHERE {nameof(SubTransInc.ParentId)} = @id;";
                ret = cnn.Query<T>(query, new { id = parentId });

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
                    ret = cnn.OpenAndReturn().Insert(dataModelBase);
                    cnn.Close();
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
            }
        }

        private string ConnectionString => $"Data Source={DbPath};Version=3;foreign keys=true;";

        private bool _dbLockFlag;

        private const string TheTitName = "The Tit";

        private readonly Func<object[], TitBase> _theTitMap = objArr =>
        {
            string type = (string)objArr[8];
            DateTime date;
            if (objArr[4] is DateTime)
                date = (DateTime)objArr[4];
            else if (
                !DateTime.TryParseExact((string)objArr[4], "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None,
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
                        Id = (long)objArr[0],
                        AccountId = (long)objArr[1],
                        PayeeId = (long)objArr[2],
                        CategoryId = (long?)objArr[3],
                        Date = date,
                        Memo = (string)objArr[5],
                        Sum = (long?)objArr[6],
                        Cleared = (long)objArr[7] == 1,
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
                        Sum = (long?)objArr[6],
                        Cleared = (long)objArr[7] == 1,
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
                        Sum = (long)objArr[6],
                        Cleared = (long)objArr[7] == 1,
                        Type = type
                    };
                    break;
                default:
                    ret = new Transaction { Memo = "ERROR ERROR In the custom mapping ERROR ERROR ERROR ERROR" };
                    break;
            }
            return ret;
        };

        public SqLiteDb(string dbPath)
        {
            DbPath = dbPath;
        }
    }
}
