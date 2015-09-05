using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using Dapper;
using YNAB = BFF.Model.Conversion.YNAB;
using Native = BFF.Model.Native;

namespace BFF.Helper.Conversion
{
    class YNABConversion
    {
        public static void ImportYNABTransactionsCSVToDB(string filePathTransaction, string filePathBudget, string dbName)
        {
            //First step: Parse CSV data into conversion objects
            List<YNAB.Transaction> transactions = parseTransactionCsv(filePathTransaction);
            List<YNAB.BudgetEntry> budgets = parseBudgetCsv(filePathBudget);

            //Second step: Convert conversion objects into native models
            //Todo: !!!
            List<Native.Transaction> nativeTransactions = transactions.Select(transaction => (Native.Transaction)transaction).ToList();
            //Todo: List<Native.Budget> nativeBudgets = budgets.Select(budget => (Native.Budget)budget).ToList();

            int hallo = 1;
            //Third step: Create new database for imported data
            //Todo: !!!
            SQLiteConnection.CreateFile(string.Format("{0}.sqlite", dbName));

            var cnn = new SQLiteConnection(string.Format("Data Source={0}.sqlite;Version=3;", dbName));
            cnn.Open();

            List<Native.Payee> payees = Native.Payee.GetAllCache();
            List<Native.Category> categories = Native.Category.GetAllCache();
            List<Native.Account> accounts = Native.Account.GetAllCache();

            string sql = payees[0].CreateTableStatement;
            SQLiteCommand cmd = new SQLiteCommand(sql, cnn);
            cmd.ExecuteNonQuery();
            sql = nativeTransactions[0].CreateTableStatement;
            cmd = new SQLiteCommand(sql, cnn);
            cmd.ExecuteNonQuery();
            sql = categories[0].CreateTableStatement;
            cmd = new SQLiteCommand(sql, cnn);
            cmd.ExecuteNonQuery();
            sql = accounts[0].CreateTableStatement;
            cmd = new SQLiteCommand(sql, cnn);
            cmd.ExecuteNonQuery();
            foreach (Native.Transaction transaction in nativeTransactions)
                transaction.InsertCommand(cnn);
            foreach (Native.Account account in accounts)
                account.InsertCommand(cnn);
            foreach (Native.Category category in categories)
                category.InsertCommand(cnn);
            foreach (Native.Payee payee in payees)
               payee.InsertCommand(cnn);

            cnn.Close();
        }

        private static List<YNAB.Transaction> parseTransactionCsv(string filePath)
        {
            var ret = new List<YNAB.Transaction>();
            if (File.Exists(filePath))
            {
                using (StreamReader streamReader = new StreamReader(new FileStream(filePath, FileMode.Open)))
                {
                    string header = streamReader.ReadLine();
                    if (header != YNAB.Transaction.CSVHeader)
                    {
                        Output.WriteLine(string.Format("The file of path '{0}' is not a valid YNAB transactions CSV.", filePath));
                        return null;
                    }
                    Output.WriteLine("Starting to import YNAB transactions from the CSV file.");
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    while (!streamReader.EndOfStream)
                    {
                        ret.Add(streamReader.ReadLine());
                    }
                    YNAB.Transaction.ToOutput(ret.Last());
                    stopwatch.Stop();
                    TimeSpan ts = stopwatch.Elapsed;
                    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                    Output.WriteLine(string.Format("End of transaction import. Elapsed time was: {0}", elapsedTime));
                }
            }
            else
            {
                Output.WriteLine(string.Format("The file of path '{0}' does not exist!", filePath));
                return null;
            }
            return ret;
        }

        private static List<YNAB.BudgetEntry> parseBudgetCsv(string filePath)
        {
            var ret = new List<YNAB.BudgetEntry>();
            if (File.Exists(filePath))
            {
                using (StreamReader streamReader = new StreamReader(new FileStream(filePath, FileMode.Open)))
                {
                    string header = streamReader.ReadLine();
                    if (header != YNAB.Transaction.CSVHeader)
                    {
                        Output.WriteLine(string.Format("The file of path '{0}' is not a valid YNAB transactions CSV.", filePath));
                        return null;
                    }
                    Output.WriteLine("Starting to import YNAB transactions from the CSV file.");
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    while (!streamReader.EndOfStream)
                    {
                        ret.Add(streamReader.ReadLine());
                    }
                    YNAB.BudgetEntry.ToOutput(ret.Last());
                    stopwatch.Stop();
                    TimeSpan ts = stopwatch.Elapsed;
                    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                    Output.WriteLine(string.Format("End of transaction import. Elapsed time was: {0}", elapsedTime));
                }
            }
            else
            {
                Output.WriteLine(string.Format("The file of path '{0}' does not exist!", filePath));
                return null;
            }
            return ret;
        }
    }
}
