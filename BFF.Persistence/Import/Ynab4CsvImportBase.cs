using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Core.Persistence;
using BFF.Persistence.Models.Import.YNAB;
using MoreLinq;
using NLog;

namespace BFF.Persistence.Import
{
    public interface IYnab4CsvImport : IImportable
    { }

    internal abstract class Ynab4CsvImportBase : IYnab4CsvImport
    {
        private readonly IYnab4ImportConfiguration _configuration;
        private readonly ILocalizer _localizer;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Ynab4CsvImportBase(
            IYnab4ImportConfiguration configuration,
            ILocalizer localizer)
        {
            _configuration = configuration;
            _localizer = localizer;
        }

        protected abstract IYnab4CsvImportContainer CreateContainer();

        public async Task<string> Import()
        {

            string exceptionTemplate = _localizer.Localize("Exception_FileNotFound");
            if (!File.Exists(_configuration.TransactionPath))
                throw new FileNotFoundException(string.Format(exceptionTemplate, _configuration.TransactionPath));
            if(!File.Exists(_configuration.BudgetPath))
                throw new FileNotFoundException(string.Format(exceptionTemplate, _configuration.BudgetPath));
            if (File.Exists(_configuration.SavePath))
                File.Delete(_configuration.SavePath); //todo: Exception handling
            await ImportYnabTransactionsCsvToDb().ConfigureAwait(false);
            return _configuration.SavePath;
         }

        internal static readonly Regex TransferPayeeRegex = new Regex(@"Transfer : (?<accountName>.+)$", RegexOptions.RightToLeft);
        internal static readonly Regex PayeePartsRegex = new Regex(@"^(?<payeeStr>.+)?(( / )?Transfer : (?<accountName>.+))?$", RegexOptions.RightToLeft);
        internal static readonly Regex SplitMemoRegex = new Regex(@"^\(Split (?<splitNumber>\d+)/(?<splitCount>\d+)\) ");
        internal static readonly Regex MemoPartsRegex = new Regex(@"^(\(Split (?<splitNumber>\d+)/(?<splitCount>\d+)\) )?(?<subTransMemo>.*)( \/ (?<parentTransMemo>.*))?$");

        internal static long ExtractLong(string text)
        {
            string number = text.ToCharArray().Where(c => char.IsDigit(c) || c == '-').Aggregate("", (current, character) => $"{current}{character}");
            return number == "" ? 0L : long.Parse(number);
        }

        private async Task ImportYnabTransactionsCsvToDb()
        {
            //Initialization
            _processedAccountsList.Clear();
            
            //First step: Parse CSV data into conversion objects
            Queue<Ynab4Transaction> ynabTransactions = new Queue<Ynab4Transaction>(ParseTransactionCsv(_configuration.TransactionPath));
            IEnumerable<Ynab4BudgetEntry> budgets = ParseBudgetCsv(_configuration.BudgetPath);

            IYnab4CsvImportContainer container = CreateContainer();

            //Second step: Convert conversion objects into native models
            ConvertTransactionsToNative(ynabTransactions, container);
            ConvertBudgetEntryToNative(budgets, container);

            //Third step: Create new database for imported data
            await container.SaveIntoDatabase().ConfigureAwait(false);
        }

        private static List<Ynab4Transaction> ParseTransactionCsv(string filePath)
        {
            List<Ynab4Transaction> ret = new List<Ynab4Transaction>();
            if (File.Exists(filePath))
            {
                using (StreamReader streamReader = new StreamReader(new FileStream(filePath, FileMode.Open)))
                {
                    string header = streamReader.ReadLine();
                    if (header != Ynab4Transaction.CsvHeader)
                    {
                        Logger.Error("The file of path '{0}' is not a valid YNAB transactions CSV.", filePath);
                        return null;
                    }
                    Logger.Info("Starting to import YNAB transactions from the CSV file.");
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    while (!streamReader.EndOfStream)
                    {
                        ret.Add(streamReader.ReadLine());
                    }
                    stopwatch.Stop();
                    TimeSpan ts = stopwatch.Elapsed;
                    string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds/10:00}";
                    Logger.Info("End of transaction import. Elapsed time was: {0}", elapsedTime);
                }
            }
            else
            {
                Logger.Error($"The file of path '{0}' does not exist!", filePath);
                return null;
            }
            return ret;
        }
        
        private static IEnumerable<Ynab4BudgetEntry> ParseBudgetCsv(string filePath)
        {
            IEnumerable<Ynab4BudgetEntry> ParseBudgetCsvInner()
            {
                using (StreamReader streamReader = new StreamReader(new FileStream(filePath, FileMode.Open)))
                {
                    string header = streamReader.ReadLine();
                    if (header != Ynab4BudgetEntry.CsvHeader)
                    {
                        var fileFormatException = new FileFormatException(new Uri(filePath), $"The budget file does not start with the YNAB budget header line: '{Ynab4BudgetEntry.CsvHeader}'");
                        Logger.Error(fileFormatException, "The budget file does not start with the YNAB budget header line: '{0}'", Ynab4BudgetEntry.CsvHeader);
                        throw fileFormatException;
                    }
                    while (!streamReader.EndOfStream)
                    {
                        string line = streamReader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                            yield return line;
                    }
                }
            }

            if (!File.Exists(filePath))
            {
                var fileNotFoundException = new FileNotFoundException($"YNAB budget export file '{filePath}' was not found", filePath);
                Logger.Error(fileNotFoundException, "The file of path {0} does not exist!", filePath);
                throw fileNotFoundException;
            }

            return ParseBudgetCsvInner();
        }

        private void ConvertTransactionsToNative(
            Queue<Ynab4Transaction> ynabTransactions, IYnab4CsvImportContainer container)
        {
            while (ynabTransactions.Count > 0)
            {
                Ynab4Transaction ynabTransaction = ynabTransactions.Dequeue();
                if (ynabTransaction.Payee == "Starting Balance")
                {
                    container.SetAccountStartingBalance(ynabTransaction.Account, ynabTransaction.Inflow - ynabTransaction.Outflow);
                    container.TrySetAccountStartingDate(ynabTransaction.Account, ynabTransaction.Date);
                }
                
                Match splitMatch = SplitMemoRegex.Match(ynabTransaction.Memo);
                if (splitMatch.Success)
                {
                    ProcessSplitTransactions(ynabTransactions, container, ynabTransaction, splitMatch);
                }
                else
                {
                    Match transferMatch = TransferPayeeRegex.Match(ynabTransaction.Payee);
                    if (transferMatch.Success)
                        AddTransfer(container, ynabTransaction);
                    else
                        container.AddAsTransaction(ynabTransaction);
                }
            }
        }

        private void ProcessSplitTransactions(
            Queue<Ynab4Transaction> ynabTransactions, 
            IYnab4CsvImportContainer container, 
            Ynab4Transaction ynabTransaction,
            Match splitMatch)
        {
            void CleanMemoFromSplitTag(Ynab4Transaction t)
            {
                var match = SplitMemoRegex.Match(t.Memo);
                if (match.Success)
                    t.Memo = t.Memo.Remove(0, match.Value.Length);
            }

            string CleanMemosFromParentMessage(Ynab4Transaction[] ts)
            {
                string parentMessage = "";


                if (ts.Any(t => t.Memo.Contains(" / ")))
                {
                    bool shouldContinue;
                    do
                    {
                        shouldContinue = false;
                        int index = ts.FirstOrDefault(t => t.Memo.Contains(" / "))?.Memo.LastIndexOf(" / ", StringComparison.Ordinal) ?? -1;
                        if (index == -1) break;
                        string potentialMessagePart = ts.First(t => t.Memo.Contains(" / ")).Memo.Substring(index);
                        var part = potentialMessagePart;
                        if (ts.All(t => t.Memo.EndsWith(part) || t.Memo == part.Substring(3)))
                        {
                            potentialMessagePart = potentialMessagePart.Substring(3);
                            parentMessage = parentMessage != ""
                                ? $"{potentialMessagePart} / {parentMessage}"
                                : potentialMessagePart;

                            foreach (Ynab4Transaction t in ts)
                            {
                                t.Memo = t.Memo.Remove(t.Memo.Length - Math.Min(potentialMessagePart.Length + 3, t.Memo.Length));
                            }

                            shouldContinue = true;
                        }
                    } while (shouldContinue);
                }

                return parentMessage;
            }

            int splitCount = int.Parse(splitMatch.Groups[nameof(splitCount)].Value);

            Ynab4Transaction[] splitTransactions = new Ynab4Transaction[splitCount];

            splitTransactions[0] = ynabTransaction;
            CleanMemoFromSplitTag(splitTransactions[0]);

            for (int i = 1; i < splitCount; i++)
            {
                splitTransactions[i] = ynabTransactions.Dequeue();
                CleanMemoFromSplitTag(splitTransactions[i]);
            }

            string parentMemo = CleanMemosFromParentMessage(splitTransactions);

            IList<Ynab4Transaction> subTransactions = new List<Ynab4Transaction>();
            foreach (var splitTransaction in splitTransactions)
            {
                Match transferMatch = TransferPayeeRegex.Match(splitTransaction.Payee);
                if (transferMatch.Success)
                    AddTransfer(container, splitTransaction);
                else
                {
                    subTransactions.Add(splitTransaction);
                }
            }
            
            if (subTransactions.Any())
            {
                container.AddParentAndSubTransactions(ynabTransaction, parentMemo, subTransactions);
            }
        }

        private void ConvertBudgetEntryToNative(IEnumerable<Ynab4BudgetEntry> ynabBudgetEntries, IYnab4CsvImportContainer container)
        {
            if(ynabBudgetEntries is null) throw new ArgumentNullException(nameof(ynabBudgetEntries));

            ynabBudgetEntries.ForEach(container.AddBudgetEntry);
        }

        /* The smart people of YNAB thought it would be a nice idea to put each Transfer two times into the export,
           one time for each Account. Fortunately, the Accounts are processed consecutively.
           That way if one of the Accounts of the Transfer points to an already processed Account,
           then it means that this Transfer is already created and can be skipped. */
        private readonly List<string> _processedAccountsList = new List<string>();
        private void AddTransfer(IYnab4CsvImportContainer container, 
                                 Ynab4Transaction ynabTransfer)
        {
            if (_processedAccountsList.Count == 0)
            {
                _processedAccountsList.Add(ynabTransfer.Account);
            }
            else if (_processedAccountsList.Last() != ynabTransfer.Account)
            {
                _processedAccountsList.Add(ynabTransfer.Account);
            }

            string otherAccount = PayeePartsRegex.Match(ynabTransfer.Payee).Groups["accountName"].Value;
            if (!_processedAccountsList.Contains(otherAccount))
            {
                container.AddAsTransfer(ynabTransfer);
            }
        }

        public static long FlagNameToColorNumber(string name)
        {
            switch (name)
            {
                case "Red":
                    return 0xffff0000;
                case "Green":
                    return 0xff00ff00;
                case "Blue":
                    return 0xff0000ff;
                case "Orange":
                    return 0xffffa500;
                case "Yellow":
                    return 0xffffff00;
                case "Purple":
                    return 0xff551A8B;
                default:
                    return 0x00000000;
            }
        }

        public static (string Master, string Sub) SplitCategoryNamePath(string namePath)
        {
            var names = namePath.Split(':');

            if(names.Length == 2) throw new ArgumentException("Name path should be split-able into master and sub category");

            return (names[0], names[1]);
        }

        public static string IncomeCategoryThisMonthSubName { get; } = "This Month";

        public static string IncomeCategoryNextMonthSubName { get; } = "Next Month";

        public static bool IsIncomeThisMonth(string master, string sub) => master == "Income" && sub == "Available this month";

        public static bool IsIncomeNextMonth(string master, string sub) => master == "Income" && sub == "Available next month";
    }
}
