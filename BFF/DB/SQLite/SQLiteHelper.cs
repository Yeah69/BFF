﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        
        /*public static void OpenDatabase(string fileName)
        {
            DbLockFlag = true;
            CurrentDbFileName = fileName;
            
            AccountCache.Clear();
            PayeeCache.Clear();
            CategoryCache.Clear();

            AllAccounts.Clear();
            AllPayees.Clear();
            AllCategories.Clear();

            using (var cnn = new SQLiteConnection(CurrentDbConnectionString))
            {
                cnn.Open();
                AccountCache = cnn.GetAll<Account>().ToDictionary(account => account.Id);
                AllAccounts = new ObservableCollection<Account>(AccountCache.Values);
                PayeeCache = cnn.GetAll<Payee>().ToDictionary(payee => payee.Id);
                AllPayees = new ObservableCollection<Payee>(PayeeCache.Values.OrderBy(payee => payee.Name));
                
                IEnumerable<Category> categories = cnn.Query<Category>($"SELECT * FROM [{nameof(Category)}s]",new[] { typeof(long), typeof(long?), typeof(string) },
                    objArray =>
                    {
                        Category ret = new Category {Id = (long)objArray[0], Parent = (objArray[1] == null) ? null : new Category {Id = (long)objArray[1]}, Name = (string)objArray[2]};
                        CategoryCache.Add(ret.Id, ret);
                        return ret;
                    }, splitOn: "*");

                CategoryCache = cnn.GetAll<Category>().ToDictionary(category => category.Id);
                foreach (Category category in categories)
                {
                    if (category.ParentId != null)
                    {
                        category.Parent = CategoryCache[(long) category.ParentId];
                        category.Parent.Categories.Add(category);
                    }
                }
                AllCategories = new ObservableCollection<Category>(categories);//categories.Where(category => category.ParentId == null));
                cnn.Close();
            }
            DbLockFlag = false;
        }*/

        #region BalanceStatements

        internal static string AllAccountsBalanceStatement =>
$@"SELECT Total({nameof(TitBase.Sum)}) FROM (
SELECT {nameof(TitBase.Sum)} FROM {nameof(Transaction)}s UNION ALL 
SELECT {nameof(TitBase.Sum)} FROM {nameof(Income)}s UNION ALL 
SELECT {nameof(TitBase.Sum)} FROM {nameof(SubTransaction)}s UNION ALL 
SELECT {nameof(TitBase.Sum)} FROM {nameof(SubIncome)}s UNION ALL 
SELECT {nameof(Account.StartingBalance)} FROM {nameof(Account)}s);";

        internal static string AccountSpecificBalanceStatement =>
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

        internal static string CreateTheTitViewStatement =>
                    $@"CREATE VIEW IF NOT EXISTS [The Tit] AS
SELECT Id, AccountId, PayeeId, CategoryId, Date, Memo, Sum, Cleared, Type FROM [{nameof(Transaction)}s]
UNION ALL
SELECT Id, AccountId, PayeeId, CategoryId, Date, Memo, Sum, Cleared, Type FROM [{nameof(Income)}s]
UNION ALL
SELECT Id, FillerId, FromAccountId, ToAccountId, Date, Memo, Sum, Cleared, Type FROM [{nameof(Transfer)}s];";

        internal static string CreateAccountTableStatement
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

        internal static string CreateCategoryTableStatement
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

        internal static string CreateIncomeTableStatement
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

        internal static string CreatePayeeTableStatement
            =>
                $@"CREATE TABLE [{nameof(Payee)}s](
                        {nameof(Payee.Id)
                    } INTEGER PRIMARY KEY,
                        {nameof(Payee.Name)} VARCHAR(100));";

        internal static string CreateSubTransactionTableStatement
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

        internal static string CreateSubIncomeTableStatement
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

        internal static string CreateTransactionTableStatement
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

        internal static string CreateTransferTableStatement
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

        internal static string CreateDbSettingTableStatement
            =>
                $@"CREATE TABLE [{nameof(DbSetting)}s]({nameof(DbSetting.Id)} INTEGER PRIMARY KEY, {nameof(DbSetting.CurrencyCultrureName)} VARCHAR(10),
{nameof(DbSetting.DateCultureName)} VARCHAR(10));";

        #endregion

    }
}
