using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Dapper;
using Dapper.Contrib.Extensions;
using YNAB = BFF.Model.Conversion.YNAB;
using Native = BFF.Model.Native;
using static BFF.DB.SQLite.Helper;

namespace BFF.Helper.Conversion
{
    class YnabConversion
    {
        internal static readonly Regex TransferPayeeRegex = new Regex(@"Transfer : (?<accountName>.+)$", RegexOptions.RightToLeft);
        internal static readonly Regex PayeePartsRegex = new Regex(@"^(?<payeeStr>.+)?(( / )?Transfer : (?<accountName>.+))?$", RegexOptions.RightToLeft);
        internal static readonly Regex SplitMemoRegex = new Regex(@"^\(Split (?<splitNumber>\d+)/(?<splitCount>\d+)\) ");
        internal static readonly Regex MemoPartsRegex = new Regex(@"^(\(Split (?<splitNumber>\d+)/(?<splitCount>\d+)\) )?((?<subTransMemo>.*) / )?(?<parentTransMemo>.*)$");


        public static void ImportYnabTransactionsCsvtoDb(string filePathTransaction, string filePathBudget, string dbName)
        {
            //First step: Parse CSV data into conversion objects
            Queue<YNAB.Transaction> ynabTransactions = new Queue<YNAB.Transaction>(ParseTransactionCsv(filePathTransaction));
            List<YNAB.BudgetEntry> budgets = ParseBudgetCsv(filePathBudget);

            //Second step: Convert conversion objects into native models
            List<Native.Transaction> transactions = new List<Native.Transaction>();
            List<Native.SubTransaction> subTransactions = new List<Native.SubTransaction>();
            List<Native.Transfer> transfers = new List<Native.Transfer>();
            ConvertTransactionsToNative(ynabTransactions, transactions, transfers, subTransactions);
            //Todo: List<Native.Budget> nativeBudgets = budgets.Select(budget => (Native.Budget)budget).ToList();
            
            //Third step: Create new database for imported data
            CurrentDbName = dbName;
            SQLiteConnection.CreateFile(CurrentDbFileName());
            PopulateDatabase(transactions, subTransactions, transfers);
        }

        private static List<YNAB.Transaction> ParseTransactionCsv(string filePath)
        {
            var ret = new List<YNAB.Transaction>();
            if (File.Exists(filePath))
            {
                using (StreamReader streamReader = new StreamReader(new FileStream(filePath, FileMode.Open)))
                {
                    string header = streamReader.ReadLine();
                    if (header != YNAB.Transaction.CsvHeader)
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

        private static List<YNAB.BudgetEntry> ParseBudgetCsv(string filePath)
        {
            var ret = new List<YNAB.BudgetEntry>();
            if (File.Exists(filePath))
            {
                using (StreamReader streamReader = new StreamReader(new FileStream(filePath, FileMode.Open)))
                {
                    string header = streamReader.ReadLine();
                    if (header != YNAB.Transaction.CsvHeader)
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

        private static void ConvertTransactionsToNative(Queue<YNAB.Transaction> ynabTransactions, List<Native.Transaction> transactions, List<Native.Transfer> transfers, List<Native.SubTransaction> subTransactions)
        {
            while (ynabTransactions.Count > 0)
            {
                YNAB.Transaction ynabTransaction = ynabTransactions.Dequeue();
                Match transferMatch = TransferPayeeRegex.Match(ynabTransaction.Payee);
                Match splitMatch = SplitMemoRegex.Match(ynabTransaction.Memo);
                if (transferMatch.Success || splitMatch.Success)
                {
                    if (splitMatch.Success)
                    {
                        //Maybe both have a match, but definitly the split
                        int splitCount = int.Parse(splitMatch.Groups[nameof(splitCount)].Value);
                        if (splitCount > 1)
                        {
                            Native.Transaction parentTransaction = ynabTransaction;
                            transactions.Add(parentTransaction);

                            if (transferMatch.Success)
                                transfers.Add(ynabTransaction);
                            else
                            {
                                Native.SubTransaction subTransaction = ynabTransaction;
                                subTransaction.Parent = parentTransaction;
                                subTransactions.Add(subTransaction);
                            }
                            for (int i = 1; i < splitCount; i++)
                            {
                                YNAB.Transaction newYnabTransaction = ynabTransactions.Dequeue();
                                Match newTrasferMatch = TransferPayeeRegex.Match(newYnabTransaction.Payee);
                                if (newTrasferMatch.Success)
                                    transfers.Add(newYnabTransaction);
                                else
                                {
                                    Native.SubTransaction subTransaction = newYnabTransaction;
                                    subTransaction.Parent = parentTransaction;
                                    subTransactions.Add(subTransaction);
                                }

                            }
                        }
                        else if (transferMatch.Success)
                        {
                            //Only one split item and its a transfer
                            transfers.Add(ynabTransaction);
                        }
                        else
                        {
                            //Only one split item and its no transfer
                            transactions.Add(ynabTransaction);
                        }

                    }
                    else
                    {
                        //todo: Fix the duplicated transfers problem
                        //Only transfer has a match
                        transfers.Add(ynabTransaction);
                    }
                }
                else
                {
                    //No split and no transfer
                    transactions.Add(ynabTransaction);
                }
            }
        }

        private static void PopulateDatabase(List<Native.Transaction> transactions, List<Native.SubTransaction> subTransactions, List<Native.Transfer> transfers)
        {
            Output.WriteLine("Beginning to populate database.");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString()))
            {
                cnn.Open();

                List<Native.Payee> payees = Native.Payee.GetAllCache();
                List<Native.Category> categories = Native.Category.GetAllCache();
                List<Native.Account> accounts = Native.Account.GetAllCache();

                cnn.Execute(Native.Transaction.CreateTableStatement);
                cnn.Execute(Native.Transfer.CreateTableStatement);
                cnn.Execute(Native.SubTransaction.CreateTableStatement);
                cnn.Execute(Native.Payee.CreateTableStatement);
                cnn.Execute(Native.Category.CreateTableStatement);
                cnn.Execute(Native.Account.CreateTableStatement);
                
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

                cnn.Close();
            }
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
            Output.WriteLine($"End of database population. Elapsed time was: {elapsedTime}");
        }
    }
}
