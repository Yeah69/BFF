using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BFF.DB;
using BFF.Model.Native.Structure;
using BFF.WPFStuff;
using YNAB = BFF.Model.Conversion.YNAB;
using Native = BFF.Model.Native;

namespace BFF.Helper.Import
{
    class YnabCsvImport : ObservableObject, IImportable
    {
        private readonly IBffOrm _orm;

        public string TransactionPath
        {
            get { return _transactionPath; }
            set
            {
                _transactionPath = value;
                OnPropertyChanged();
            }
        }

        public string BudgetPath
        {
            get { return _budgetPath; }
            set
            {
                _budgetPath = value;
                OnPropertyChanged();
            }
        }

        public string SavePath
        {
            get { return _savePath; }
            set
            {
                _savePath = value;
                OnPropertyChanged();
            }
        }

        public string Import()
         {
            if(!File.Exists(TransactionPath))
                throw new FileNotFoundException($"Localize this!!!!!"); //todo: Localize error message
            if(!File.Exists(BudgetPath))
                throw new FileNotFoundException($"Localize this!!!!!"); //todo: Localize error message
            if (File.Exists(SavePath))
                File.Delete(SavePath); //todo: Exception handling
            ImportYnabTransactionsCsvtoDb(TransactionPath, BudgetPath, SavePath);
            return SavePath;
         }

        internal static readonly Regex TransferPayeeRegex = new Regex(@"Transfer : (?<accountName>.+)$", RegexOptions.RightToLeft);
        internal static readonly Regex PayeePartsRegex = new Regex(@"^(?<payeeStr>.+)?(( / )?Transfer : (?<accountName>.+))?$", RegexOptions.RightToLeft);
        internal static readonly Regex SplitMemoRegex = new Regex(@"^\(Split (?<splitNumber>\d+)/(?<splitCount>\d+)\) ");
        internal static readonly Regex MemoPartsRegex = new Regex(@"^(\(Split (?<splitNumber>\d+)/(?<splitCount>\d+)\) )?((?<subTransMemo>.*) / )?(?<parentTransMemo>.*)$");
        internal static readonly Regex NumberExtractRegex = new Regex(@"\d+");

        internal static long ExtractLong(string text)
        {
            if (NumberExtractRegex.IsMatch(text))
            {
                string concatedNumber = NumberExtractRegex.Matches(text).Cast<Match>().Aggregate("", (current, match) => current + match.Value);
                return long.Parse(concatedNumber); // todo: there are exceptions to the divide through hundred
            }
            return 0L;
        }

        public YnabCsvImport(IBffOrm orm)
        {
            _orm = orm;
        }

        public void ImportYnabTransactionsCsvtoDb(string filePathTransaction, string filePathBudget, string savePath)
        {
            //Initialization
            ProcessedAccountsList.Clear();
            Native.Account.ClearCache();
            Native.Payee.ClearCache();
            Native.Category.ClearCache();

            DataModelBase.Database = null; //switches off OR mapping

            //First step: Parse CSV data into conversion objects
            Queue<YNAB.Transaction> ynabTransactions = new Queue<YNAB.Transaction>(ParseTransactionCsv(filePathTransaction));
            List<YNAB.BudgetEntry> budgets = ParseBudgetCsv(filePathBudget);

            //Second step: Convert conversion objects into native models
            List<Native.Transaction> transactions = new List<Native.Transaction>();
            List<Native.SubTransaction> subTransactions = new List<Native.SubTransaction>();
            List<Native.Transfer> transfers = new List<Native.Transfer>();
            List<Native.Income> incomes = new List<Native.Income>();
            ConvertTransactionsToNative(ynabTransactions, transactions, transfers, subTransactions, incomes);
            //Todo: List<Native.Budget> nativeBudgets = budgets.Select(budget => (Native.Budget)budget).ToList();

            //Third step: Create new database for imported data
            DataModelBase.Database = _orm; //turn on OR mapping
            _orm.DbPath = savePath;
            _orm.CreateNewDatabase();
            _orm.PopulateDatabase(transactions, subTransactions, incomes, new List<Native.SubIncome>(), 
                transfers, Native.Account.GetAllCache(), Native.Payee.GetAllCache(), Native.Category.GetAllCache());
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

        private static void ConvertTransactionsToNative(Queue<YNAB.Transaction> ynabTransactions, List<Native.Transaction> transactions, List<Native.Transfer> transfers, List<Native.SubTransaction> subTransactions, List<Native.Income> incomes )
        {
            while (ynabTransactions.Count > 0)
            {
                YNAB.Transaction ynabTransaction = ynabTransactions.Dequeue();
                if (ynabTransaction.Payee == "Starting Balance")
                {
                    Native.Account.GetOrCreate(ynabTransaction.Account).StartingBalance = ynabTransaction.Inflow - ynabTransaction.Outflow;
                    continue;
                }
                
                Match splitMatch = SplitMemoRegex.Match(ynabTransaction.Memo);
                if (splitMatch.Success)
                {
                    Native.ParentTransaction parent = (Native.ParentTransaction)ynabTransaction;
                    int splitCount = int.Parse(splitMatch.Groups[nameof(splitCount)].Value);
                    int count = 0;
                    for (int i = 0; i < splitCount; i++)
                    {
                        YNAB.Transaction newYnabTransaction = i==0 ? ynabTransaction : ynabTransactions.Dequeue();
                        Match transferMatch = TransferPayeeRegex.Match(newYnabTransaction.Payee);
                        if (transferMatch.Success)
                            AddTransfer(transfers, newYnabTransaction);
                        else if (newYnabTransaction.MasterCategory == "Income")
                            incomes.Add(newYnabTransaction);
                        else
                        {
                            Native.SubTransaction subTransaction = newYnabTransaction;
                            subTransaction.Parent = parent;
                            subTransactions.Add(subTransaction);
                            count++;
                        }
                    }
                    if (count > 0)
                    {
                        transactions.Add(parent);
                    }
                }
                else
                {
                    Match transferMatch = TransferPayeeRegex.Match(ynabTransaction.Payee);
                    if (transferMatch.Success)
                        AddTransfer(transfers, ynabTransaction);
                    else if (ynabTransaction.MasterCategory == "Income")
                        incomes.Add(ynabTransaction);
                    else
                        transactions.Add(ynabTransaction);
                }
            }
        }
        private string _transactionPath;
        private string _budgetPath;
        private string _savePath;

        /* The smart people of YNAB thought it would be a nice idea to put each Transfer two times into the export,
           one time for each Account. Fortunatelly, the Accounts are processed consecutively.
           That way if one of the Accounts of the Transfer points to an already processed Account,
           then it means that this Transfer is already created and can be skipped. */
        private static readonly List<string> ProcessedAccountsList = new List<string>();
        private static void AddTransfer(List<Native.Transfer> transfers, YNAB.Transaction ynabTransfer)
        {
            if (ProcessedAccountsList.Count == 0)
            {
                ProcessedAccountsList.Add(ynabTransfer.Account);
            }
            else if (ProcessedAccountsList.Last() != ynabTransfer.Account)
            {
                ProcessedAccountsList.Add(ynabTransfer.Account);
            }

            string otherAccount = PayeePartsRegex.Match(ynabTransfer.Payee).Groups["accountName"].Value;
            if (!ProcessedAccountsList.Contains(otherAccount))
            {
                transfers.Add(ynabTransfer);
            }
        }
    }
}
