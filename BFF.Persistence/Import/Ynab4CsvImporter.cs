using BFF.Model.Contexts;
using BFF.Model.Import;
using BFF.Model.Import.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BFF.Model.ImportExport;
using BFF.Persistence.Common;
using BFF.Persistence.Import.Models.YNAB;
using MoreLinq;
using NLog;

namespace BFF.Persistence.Import
{
    public interface IYnab4CsvImporter : IImportContext
    {
    }

    internal class Ynab4CsvImporter : IYnab4CsvImporter
    {
        private static readonly Regex TransferPayeeRegex = new (@"Transfer : (?<accountName>.+)$", RegexOptions.RightToLeft);
        private static readonly Regex PayeePartsRegex = new (@"^(?<payeeStr>.+)?(( / )?Transfer : (?<accountName>.+))?$", RegexOptions.RightToLeft);
        private static readonly Regex SplitMemoRegex = new (@"^\(Split (?<splitNumber>\d+)/(?<splitCount>\d+)\) ");
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IDtoImportContainerBuilder _dtoImportContainerBuilder;
        private readonly IYnab4CsvImportConfiguration _ynab4CsvImportConfiguration;
        private readonly IDisposable _cleanUpContext;

        public Ynab4CsvImporter(
            IDtoImportContainerBuilder dtoImportContainerBuilder,
            IYnab4CsvImportConfiguration ynab4CsvImportConfiguration,
            IDisposable cleanUpContext)
        {
            _dtoImportContainerBuilder = dtoImportContainerBuilder;
            _ynab4CsvImportConfiguration = ynab4CsvImportConfiguration;
            _cleanUpContext = cleanUpContext;

            Title = ynab4CsvImportConfiguration.TransactionPath;
        }

        public Task<DtoImportContainer> Import()
        {
            //First step: Parse CSV data into conversion objects
            Queue<Ynab4Transaction> ynabTransactions = new Queue<Ynab4Transaction>(ParseTransactionCsv(_ynab4CsvImportConfiguration.TransactionPath));
            IEnumerable<Ynab4BudgetEntry> budgets = ParseBudgetCsv(_ynab4CsvImportConfiguration.BudgetPath);

            //Second step: Convert conversion objects into native models
            ConvertTransactionsToNative(ynabTransactions);
            ConvertBudgetEntryToNative(budgets);

            //Third step: Create new database for imported data
            return Task.FromResult(_dtoImportContainerBuilder.BuildContainer());
        }

        private void ConvertTransactionsToNative(
            Queue<Ynab4Transaction> ynabTransactions)
        {
            while (ynabTransactions.Count > 0)
            {
                Ynab4Transaction ynabTransaction = ynabTransactions.Dequeue();
                if (ynabTransaction.Payee == "Starting Balance")
                {
                    _dtoImportContainerBuilder.SetAccountStartingBalance(ynabTransaction.Account, ynabTransaction.Inflow - ynabTransaction.Outflow);
                    _dtoImportContainerBuilder.TrySetAccountStartingDate(ynabTransaction.Account, ynabTransaction.Date);
                }
                else
                {
                    Match splitMatch = SplitMemoRegex.Match(ynabTransaction.Memo);
                    if (splitMatch.Success)
                    {
                        ProcessSplitTransactions(ynabTransactions, ynabTransaction, splitMatch);
                    }
                    else
                    {
                        Match transferMatch = TransferPayeeRegex.Match(ynabTransaction.Payee);
                        if (transferMatch.Success)
                            AddTransfer(ynabTransaction);
                        else
                            _dtoImportContainerBuilder.AddAsTransaction(
                                DateTime.SpecifyKind(ynabTransaction.Date, DateTimeKind.Utc),
                                ynabTransaction.Account,
                                ynabTransaction.ParsePayeeFromPayee(),
                                GetOrCreateCategory(ynabTransaction.MasterCategory, ynabTransaction.SubCategory),
                                ynabTransaction.CheckNumber,
                                FlagNameToColorNumber(ynabTransaction.Flag),
                                ynabTransaction.Memo,
                                ynabTransaction.Inflow - ynabTransaction.Outflow,
                                ynabTransaction.Cleared);
                    }
                }
            }
        }

        private void ProcessSplitTransactions(
            Queue<Ynab4Transaction> ynabTransactions,
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
                    AddTransfer(splitTransaction);
                else
                {
                    subTransactions.Add(splitTransaction);
                }
            }

            if (subTransactions.Any())
            {
                _dtoImportContainerBuilder.AddParentAndSubTransactions(
                    DateTime.SpecifyKind(ynabTransaction.Date, DateTimeKind.Utc),
                    ynabTransaction.Account,
                    ynabTransaction.ParsePayeeFromPayee(),
                    ynabTransaction.CheckNumber,
                    FlagNameToColorNumber(ynabTransaction.Flag),
                    parentMemo,
                    ynabTransaction.Cleared,
                    subTransactions.Select(st => (GetOrCreateCategory(st.MasterCategory, st.SubCategory), st.Memo, st.Inflow - st.Outflow)));
            }
        }

        /* The smart people of YNAB thought it would be a nice idea to put each Transfer two times into the export,
           one time for each Account. Fortunately, the Accounts are processed consecutively.
           That way if one of the Accounts of the Transfer points to an already processed Account,
           then it means that this Transfer is already created and can be skipped. */
        private readonly List<string> _processedAccountsList = new ();
        private void AddTransfer(Ynab4Transaction ynabTransfer)
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
                _dtoImportContainerBuilder.AddAsTransfer(
                    DateTime.SpecifyKind(ynabTransfer.Date, DateTimeKind.Utc),
                    ynabTransfer.GetFromAccountName(),
                    ynabTransfer.GetToAccountName(),
                    ynabTransfer.CheckNumber,
                    FlagNameToColorNumber(ynabTransfer.Flag),
                    ynabTransfer.Memo,
                    Math.Abs(ynabTransfer.Inflow - ynabTransfer.Outflow),
                    ynabTransfer.Cleared);
            }
        }

        private void ConvertBudgetEntryToNative(IEnumerable<Ynab4BudgetEntry> ynabBudgetEntries) =>
            ynabBudgetEntries
                .Where(ybe => ybe.MasterCategory != "Uncategorized Transactions")
                .ForEach(ybe =>
                {
                    var month = DateTime.ParseExact(ybe.Month, "MMMM yyyy", CultureInfo.GetCultureInfo("de-DE")); // TODO make this customizable + exception handling
                    _dtoImportContainerBuilder.AddBudgetEntry(
                        DateTime.SpecifyKind(month, DateTimeKind.Utc),
                        GetOrCreateCategory(ybe.MasterCategory, ybe.SubCategory),
                        ybe.Budgeted);
                });

        private static List<Ynab4Transaction> ParseTransactionCsv(string filePath)
        {
            List<Ynab4Transaction> ret = new List<Ynab4Transaction>();
            if (File.Exists(filePath))
            {
                using var streamReader = new StreamReader(new FileStream(filePath, FileMode.Open));
                string? header = streamReader.ReadLine();
                if (header != Ynab4Transaction.CsvHeader)
                {
                    Logger.Error("The file of path '{0}' is not a valid YNAB transactions CSV.", filePath);
                    throw new FileFormatException($"The file of path '{filePath}' is not a valid YNAB transactions CSV.");
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
                string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
                Logger.Info("End of transaction import. Elapsed time was: {0}", elapsedTime);
            }
            else
            {
                Logger.Error("The file of path '{0}' does not exist!", filePath);
                throw new FileNotFoundException($"The file of path '{filePath}' does not exist!");
            }
            return ret;
        }

        private static IEnumerable<Ynab4BudgetEntry> ParseBudgetCsv(string filePath)
        {
            IEnumerable<Ynab4BudgetEntry> ParseBudgetCsvInner()
            {
                using var streamReader = new StreamReader(new FileStream(filePath, FileMode.Open));
                var header = streamReader.ReadLine();
                if (header != Ynab4BudgetEntry.CsvHeader)
                {
                    var fileFormatException = new FileFormatException(new Uri(filePath), $"The budget file does not start with the YNAB budget header line: '{Ynab4BudgetEntry.CsvHeader}'");
                    Logger.Error(fileFormatException, "The budget file does not start with the YNAB budget header line: '{0}'", Ynab4BudgetEntry.CsvHeader);
                    throw fileFormatException;
                }
                while (!(streamReader.EndOfStream))
                {
                    string? line = streamReader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                        yield return line;
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

        private static (string, Color)? FlagNameToColorNumber(string name)
        {
            return name switch
                {
                    "Red" => (name, Color.FromArgb(0xff, 0xff, 0x00, 0x00)),
                    "Green" => (name, Color.FromArgb(0xff, 0x00, 0xff, 0x00)),
                    "Blue" => (name, Color.FromArgb(0xff, 0x00, 0x00, 0xff)),
                    "Orange" => (name, Color.FromArgb(0xff, 0xff, 0xa5, 0x00)),
                    "Yellow" => (name, Color.FromArgb(0xff, 0xff, 0xff, 0x00)),
                    "Purple" => (name, Color.FromArgb(0xff, 0x55, 0x1a, 0x8b)),
                    _ => null
                };
        }

        private CategoryDto GetOrCreateCategory(string master, string sub)
        {
            return (master, sub) switch
                {
                (_, _) when IsIncomeNextMonth(master, sub) => _dtoImportContainerBuilder.GetOrCreateIncomeCategory(
                    IncomeCategoryNextMonthSubName, 1),
                (_, _) when IsIncomeThisMonth(master, sub) => _dtoImportContainerBuilder.GetOrCreateIncomeCategory(
                    IncomeCategoryThisMonthSubName, 0),
                _ => _dtoImportContainerBuilder.GetOrCreateCategory(new[] { master, sub })
                };
        }

        private static string IncomeCategoryThisMonthSubName { get; } = "This Month";

        private static string IncomeCategoryNextMonthSubName { get; } = "Next Month";

        private static bool IsIncomeThisMonth(string master, string sub) => master == "Income" && sub == "Available this month";

        private static bool IsIncomeNextMonth(string master, string sub) => master == "Income" && sub == "Available next month";

        public void Dispose() => 
            _cleanUpContext.Dispose();

        public string Title { get; }
    }
    
    internal class Ynab4CsvImportContextFactory : ContextFactoryBase<IYnab4CsvImportConfiguration, IYnab4CsvImporter>
    {
        public Ynab4CsvImportContextFactory(Func<IYnab4CsvImportConfiguration, IYnab4CsvImporter> factory) : base(factory)
        {}
    }
}
