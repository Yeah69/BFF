﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
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
                Reset();
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

        public IEnumerable<TitBase> GetAllTits(Account account = null)
        {
            _dbLockFlag = true;
            IEnumerable<TitBase> results;

            using (var cnn = new SQLiteConnection(ConnectionString))
            {
                cnn.Open();

                string accountAddition = account == null
                    ? ""
                    : "WHERE AccountId = @accountId OR AccountId = -1 AND (PayeeId = @accountId OR CategoryId = @accountId)";
                string sql = $"SELECT * FROM [{TheTitName}] {accountAddition} ORDER BY Date;";

                Type[] types =
                {
                    typeof (long), typeof (long), typeof (long), typeof (long),
                    typeof (DateTime), typeof (string), typeof (long), typeof (bool), typeof(string)
                };
                results = cnn.Query(sql, types, _theTitMap, account == null ? null : new { accountId = account.Id }, splitOn: "*");
                //foreach (TitBase result in results) result.Database = this;
                
                cnn.Close();
            }

            _dbLockFlag = false;
            return results;
        }

        public IEnumerable<Category> GetAllCache()
        {
            throw new NotImplementedException();
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

        public IEnumerable<T> GetSubTransInc<T>(long parentId) where T : SubTitBase
        {
            IEnumerable<T> ret = new List<T>();
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
                    //foreach (T element in ret) element.Database = this;
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
                if (dataModelBase is CommonProperties)
                {
                    if(dataModelBase is Account)
                        AllAccounts.Add(dataModelBase as Account);
                    else if (dataModelBase is Payee)
                        AllPayees.Add(dataModelBase as Payee);
                    else if (dataModelBase is Category)
                        AllCategories.Add(dataModelBase as Category);
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
                if (dataModelBase is CommonProperties)
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

        public SqLiteBffOrm()
        {
            DataModelBase.Database = this;
            if(File.Exists(DbPath)) Reset();
        }

        public ObservableCollection<Account> AllAccounts { get; private set; } = new ObservableCollection<Account>();
        public ObservableCollection<Payee> AllPayees { get; private set; } = new ObservableCollection<Payee>();
        public ObservableCollection<Category> AllCategories { get; private set; } = new ObservableCollection<Category>(); 

        public void Reset()
        {
            AllAccounts.Clear();
            AllPayees.Clear();
            AllCategories.Clear();
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
                            Category ret = new Category { Id = (long)objArray[0], Parent = (objArray[1] == null) ? null : new Category { Id = (long)objArray[1] } /*dummy*/, Name = (string)objArray[2] };
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
            }
        }

        public Account GetAccount(long id) => AllAccounts.FirstOrDefault(account => account.Id == id);
        public Payee GetPayee(long id) => AllPayees.FirstOrDefault(payee => payee.Id == id);
        public Category GetCategory(long id) => AllCategories.FirstOrDefault(category => category.Id == id);

    }
}
