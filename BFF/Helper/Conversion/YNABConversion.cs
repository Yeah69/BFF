using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using Dapper;
using Dapper.Contrib.Extensions;
using YNAB = BFF.Model.Conversion.YNAB;
using Native = BFF.Model.Native;
using static BFF.DB.SQLite.Helper;

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
            List<Native.Transaction> nativeTransactions = transactions.Select(transaction => (Native.Transaction)transaction).ToList();
            //Todo: List<Native.Budget> nativeBudgets = budgets.Select(budget => (Native.Budget)budget).ToList();
            
            //Third step: Create new database for imported data
            CurrentDBName = dbName;
            SQLiteConnection.CreateFile(CurrentDBFileName());
            populateDatabase(nativeTransactions);
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
                        Output.WriteLine($"The file of path '{filePath}' is not a valid YNAB transactions CSV.");
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
                    string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds/10:00}";
                    Output.WriteLine($"End of transaction import. Elapsed time was: {elapsedTime}");
                }
            }
            else
            {
                Output.WriteLine($"The file of path '{filePath}' does not exist!");
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
                        Output.WriteLine($"The file of path '{filePath}' is not a valid YNAB transactions CSV.");
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
                    string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds/10:00}";
                    Output.WriteLine($"End of transaction import. Elapsed time was: {elapsedTime}");
                }
            }
            else
            {
                Output.WriteLine($"The file of path '{filePath}' does not exist!");
                return null;
            }
            return ret;
        }

        private static void populateDatabase(List<Native.Transaction> transactions)
        {
            Output.WriteLine($"Beginning to populate database.");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            using (var cnn = new SQLiteConnection(CurrentDBConnectionString()))
            {
                cnn.Open();

                List<Native.Payee> payees = Native.Payee.GetAllCache();
                List<Native.Category> categories = Native.Category.GetAllCache();
                List<Native.Account> accounts = Native.Account.GetAllCache();

                cnn.Execute(transactions.First().CreateTableStatement);
                cnn.Execute(payees.First().CreateTableStatement);
                cnn.Execute(categories.First().CreateTableStatement);
                cnn.Execute(accounts.First().CreateTableStatement);
                
                payees.ForEach(payee => payee.ID = (int) cnn.Insert<Native.Payee>(payee));
                //ToDo: Hierarchical Category Inserting
                categories.ForEach(category => category.ID = (int)cnn.Insert<Native.Category>(category));
                accounts.ForEach(account => account.ID = (int)cnn.Insert<Native.Account>(account));
                //ToDo: Split Transactions
                cnn.InsertAsync<List<Native.Transaction>>(transactions); //Can be inserted as a whole list, because all IDs of the other models are set already

            }
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
            Output.WriteLine($"End of database population. Elapsed time was: {elapsedTime}");
        }
    }
}
