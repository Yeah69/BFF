﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autofac.Features.OwnedInstances;
using BFF.Core;
using BFF.DB;
using BFF.Helper.Extensions;
using BFF.MVVM;
using BFF.Persistence;
using BFF.Persistence.Import;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;
using NLog;
using BudgetEntry = BFF.MVVM.Models.Conversion.YNAB.BudgetEntry;
using IHaveCategory = BFF.Persistence.Models.IHaveCategory;
using Transaction = BFF.MVVM.Models.Conversion.YNAB.Transaction;

namespace BFF.Helper.Import
{
    public interface IYnabCsvImport : IImportable
    { }

    class YnabCsvImport : ObservableObject, IYnabCsvImport
    {
        private readonly IImportingOrm _importingOrm;
        private readonly ICreateBackendOrm _createBackendOrm;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public string TransactionPath
        {
            get => _transactionPath;
            set
            {
                _transactionPath = value;
                OnPropertyChanged();
            }
        }

        public string BudgetPath
        {
            get => _budgetPath;
            set
            {
                _budgetPath = value;
                OnPropertyChanged();
            }
        }

        public string SavePath
        {
            get => _savePath;
            set
            {
                _savePath = value;
                OnPropertyChanged();
            }
        }

        public YnabCsvImport(
            (string TransactionPath, string BudgetPath, string SavePath) paths,
            Owned<Func<string, IProvideConnection>> ownedCreateProvideConnection,
            Func<IProvideConnection, IImportingOrm> createImportingOrm,
            Func<IProvideConnection, ICreateBackendOrm> createCreateBackendOrm)
        {
            var provideConnection = ownedCreateProvideConnection.Value(paths.SavePath);
            ownedCreateProvideConnection.Dispose();
            _importingOrm = createImportingOrm(provideConnection);
            _createBackendOrm = createCreateBackendOrm(provideConnection);
            
            _transactionPath = paths.TransactionPath;
            _budgetPath = paths.BudgetPath;
            _savePath = paths.SavePath;
        }

        public async Task<string> Import()
        {

            string exceptionTemplate = "Exception_FileNotFound".Localize();
            if (!File.Exists(TransactionPath))
                throw new FileNotFoundException(string.Format(exceptionTemplate, TransactionPath));
            if(!File.Exists(BudgetPath))
                throw new FileNotFoundException(string.Format(exceptionTemplate, BudgetPath));
            if (File.Exists(SavePath))
                File.Delete(SavePath); //todo: Exception handling
            await ImportYnabTransactionsCsvToDb(TransactionPath, BudgetPath, SavePath).ConfigureAwait(false);
            return SavePath;
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

        public async Task ImportYnabTransactionsCsvToDb(string filePathTransaction, string filePathBudget, string savePath)
        {
            //Initialization
            _processedAccountsList.Clear();
            ClearAccountCache(); 
            ClearPayeeCache();
            ClearCategoryCache();
            ClearFlagCache();
            
            //First step: Parse CSV data into conversion objects
            Queue<Transaction> ynabTransactions = new Queue<Transaction>(ParseTransactionCsv(filePathTransaction));
            IEnumerable<BudgetEntry> budgets = ParseBudgetCsv(filePathBudget);

            ImportLists lists = new ImportLists
            {
                Accounts = new List<Account>(),
                Categories = new List<CategoryImportWrapper>(),
                Payees = new List<Payee>(),
                Flags = new List<Flag>(),
                ParentTransactions = new List<Trans>(),
                SubTransactions = new List<SubTransaction>(),
                Transactions = new List<Trans>(),
                Transfers = new List<Trans>()
            };

            //Second step: Convert conversion objects into native models
            ConvertTransactionsToNative(ynabTransactions, lists);
            lists.BudgetEntries = ConvertBudgetEntryToNative(budgets).ToList();
            lists.Accounts = GetAllAccountCache();
            lists.Payees = GetAllPayeeCache();
            lists.Flags = GetAllFlagCache();
            _categoryImportWrappers.Add(_thisMonthCategoryImportWrapper);
            _categoryImportWrappers.Add(_nextMonthCategoryImportWrapper);
            lists.Categories = _categoryImportWrappers;

            ImportAssignments assignments = new ImportAssignments
            {
                AccountToTransactionBase = _accountAssignment,
                FromAccountToTransfer = _fromAccountAssignment,
                ToAccountToTransfer = _toAccountAssignment,
                PayeeToTransactionBase = _payeeAssignment,
                ParentTransactionToSubTransaction = _parentTransactionAssignment,
                FlagToTransBase = _flagAssignment
            };

            //Third step: Create new database for imported data
            await _createBackendOrm.CreateAsync().ConfigureAwait(false);
            await _importingOrm.PopulateDatabaseAsync(lists, assignments).ConfigureAwait(false);
        }

        private static List<Transaction> ParseTransactionCsv(string filePath)
        {
            List<Transaction> ret = new List<Transaction>();
            if (File.Exists(filePath))
            {
                using (StreamReader streamReader = new StreamReader(new FileStream(filePath, FileMode.Open)))
                {
                    string header = streamReader.ReadLine();
                    if (header != Transaction.CsvHeader)
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
                    Transaction.ToOutput(ret.Last());
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
        
        private static IEnumerable<BudgetEntry> ParseBudgetCsv(string filePath)
        {
            IEnumerable<BudgetEntry> ParseBudgetCsvInner()
            {
                using (StreamReader streamReader = new StreamReader(new FileStream(filePath, FileMode.Open)))
                {
                    string header = streamReader.ReadLine();
                    if (header != BudgetEntry.CsvHeader)
                    {
                        var fileFormatException = new FileFormatException(new Uri(filePath), $"The budget file does not start with the YNAB budget header line: '{BudgetEntry.CsvHeader}'");
                        Logger.Error(fileFormatException, "The budget file does not start with the YNAB budget header line: '{0}'", BudgetEntry.CsvHeader);
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
            Queue<Transaction> ynabTransactions, ImportLists lists)
        {
            //Account pre-processing
            //First create all available Accounts. The reason for this is to make the Account all assignable from the beginning
            foreach(Transaction ynabTransaction in ynabTransactions
                .Where(ynabTransaction => ynabTransaction.Payee == "Starting Balance"))
            {
                CreateAccount(
                    ynabTransaction.Account, 
                    ynabTransaction.Inflow - ynabTransaction.Outflow,
                    ynabTransactions.Min(yt => yt.Date).Date);
            }
            //Now process the queue
            while (ynabTransactions.Count > 0)
            {
                Transaction ynabTransaction = ynabTransactions.Dequeue();
                if (ynabTransaction.Payee == "Starting Balance")
                {
                    //These are Transactions in YNAB but not for BFF. They are only used to create Accounts as only there is the Starting Balance
                    continue;
                }
                
                Match splitMatch = SplitMemoRegex.Match(ynabTransaction.Memo);
                if (splitMatch.Success)
                {
                    ProcessSplitTransactions(ynabTransactions, lists, ynabTransaction, splitMatch);
                }
                else
                {
                    Match transferMatch = TransferPayeeRegex.Match(ynabTransaction.Payee);
                    if (transferMatch.Success)
                        AddTransfer(lists.Transfers, ynabTransaction);
                    else
                        lists.Transactions.Add(TransformToTransaction(ynabTransaction));
                }
            }
        }

        private void ProcessSplitTransactions(
            Queue<Transaction> ynabTransactions, 
            ImportLists lists, 
            Transaction ynabTransaction,
            Match splitMatch)
        {
            void CleanMemoFromSplitTag(Transaction t)
            {
                var match = SplitMemoRegex.Match(t.Memo);
                if (match.Success)
                    t.Memo = t.Memo.Remove(0, match.Value.Length);
            }

            string CleanMemosFromParentMessage(Transaction[] ts)
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

                            foreach (Transaction t in ts)
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

            Transaction[] splitTransactions = new Transaction[splitCount];

            splitTransactions[0] = ynabTransaction;
            CleanMemoFromSplitTag(splitTransactions[0]);

            for (int i = 1; i < splitCount; i++)
            {
                splitTransactions[i] = ynabTransactions.Dequeue();
                CleanMemoFromSplitTag(splitTransactions[i]);
            }

            string parentMemo = CleanMemosFromParentMessage(splitTransactions);

            Trans parent = TransformToParentTransaction(ynabTransaction, parentMemo);

            bool createdSubTransactions = false;
            foreach (var splitTransaction in splitTransactions)
            {
                Match transferMatch = TransferPayeeRegex.Match(splitTransaction.Payee);
                if (transferMatch.Success)
                    AddTransfer(lists.Transfers, splitTransaction);
                else
                {
                    SubTransaction subTransaction = TransformToSubTransaction(
                        splitTransaction, parent);
                    lists.SubTransactions.Add(subTransaction);
                    createdSubTransactions = true;
                }
            }
            
            if (createdSubTransactions)
            {
                lists.ParentTransactions.Add(parent);
            }
        }

        private IEnumerable<Persistence.Models.BudgetEntry> ConvertBudgetEntryToNative(IEnumerable<BudgetEntry> ynabBudgetEntries)
        {
            IEnumerable<Persistence.Models.BudgetEntry> ConvertBudgetEntryToNativeInner()
            {
                foreach(var ynabBudgetEntry in ynabBudgetEntries)
                {
                    if(ynabBudgetEntry.Budgeted != 0L)
                    {
                        var month = DateTime.ParseExact(ynabBudgetEntry.Month, "MMMM yyyy", CultureInfo.GetCultureInfo("de-DE")); // TODO make this customizable + exception handling
                        Persistence.Models.BudgetEntry budgetEntry = new Persistence.Models.BudgetEntry
                        {
                            Month = month,
                            Budget = ynabBudgetEntry.Budgeted
                        };

                        AssignCategory(ynabBudgetEntry.Category, budgetEntry);
                        yield return budgetEntry;
                    }
                }
            }
            
            if(ynabBudgetEntries is null) throw new ArgumentNullException(nameof(ynabBudgetEntries));

            return ConvertBudgetEntryToNativeInner();
        }

        private string _transactionPath;
        private string _budgetPath;
        private string _savePath;

        /* The smart people of YNAB thought it would be a nice idea to put each Transfer two times into the export,
           one time for each Account. Fortunately, the Accounts are processed consecutively.
           That way if one of the Accounts of the Transfer points to an already processed Account,
           then it means that this Transfer is already created and can be skipped. */
        private readonly List<string> _processedAccountsList = new List<string>();
        private void AddTransfer(IList<Trans> transfers, 
                                 Transaction ynabTransfer)
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
                transfers.Add(TransformToTransfer(ynabTransfer));
            }
        }

        #region Transformations

        /// <summary>
        /// Creates a Transaction-object depending on a YNAB-Transaction
        /// </summary>
        /// <param name="ynabTransaction">The YNAB-model</param>
        private Trans TransformToTransaction(Transaction ynabTransaction)
        {
            Trans ret = new Trans
            {
                CheckNumber = ynabTransaction.CheckNumber,
                Date = ynabTransaction.Date,
                Memo = ynabTransaction.Memo,
                Sum = ynabTransaction.Inflow - ynabTransaction.Outflow,
                Cleared = ynabTransaction.Cleared ? 1 : 0,
                Type = nameof(TransType.Transaction)
            };
            AssignAccount(ynabTransaction.Account, ret);
            CreateAndOrAssignPayee(PayeePartsRegex.Match(ynabTransaction.Payee).Groups["payeeStr"].Value, ret);
            AssignCategory(ynabTransaction.Category, ret);
            CreateAndOrAssignFlag(ynabTransaction.Flag, ret);
            return ret;
        }

        /// <summary>
        /// Creates a Transfer-object depending on a YNAB-Transaction
        /// </summary>
        /// <param name="ynabTransaction">The YNAB-model</param>
        private Trans TransformToTransfer(Transaction ynabTransaction)
        {
            long tempSum = ynabTransaction.Inflow - ynabTransaction.Outflow;
            Trans ret = new Trans
            {
                AccountId = -69,
                CheckNumber = ynabTransaction.CheckNumber,
                Date = ynabTransaction.Date,
                Memo = ynabTransaction.Memo,
                Sum = Math.Abs(tempSum),
                Cleared = ynabTransaction.Cleared ? 1 : 0,
                Type = nameof(TransType.Transfer)
            };
            if(tempSum < 0)
            {
                AssignFormAccount(ynabTransaction.Account, ret);
                AssignToAccount(PayeePartsRegex.Match(ynabTransaction.Payee).Groups["accountName"].Value, ret);
            }
            else
            {
                AssignFormAccount(PayeePartsRegex.Match(ynabTransaction.Payee).Groups["accountName"].Value, ret);
                AssignToAccount(ynabTransaction.Account, ret);
            }
            CreateAndOrAssignFlag(ynabTransaction.Flag, ret);
            return ret;
        }

        private readonly IDictionary<Trans, IList<SubTransaction>> 
            _parentTransactionAssignment = new Dictionary<Trans, IList<SubTransaction>>();

        /// <summary>
        /// Creates a Transaction-object depending on a YNAB-Transaction
        /// </summary>
        /// <param name="ynabTransaction">The YNAB-model</param>
        /// <param name="parentMemo">Parent memo which is extracted from the split transactions</param>
        private Trans TransformToParentTransaction(Transaction ynabTransaction, string parentMemo)
        {
            Trans ret = new Trans
            {
                CategoryId = -69,
                CheckNumber = ynabTransaction.CheckNumber,
                Date = ynabTransaction.Date,
                Memo = parentMemo,
                Sum = -69,
                Cleared = ynabTransaction.Cleared ? 1 : 0,
                Type = nameof(TransType.ParentTransaction)
            };
            AssignAccount(ynabTransaction.Account, ret);
            CreateAndOrAssignPayee(PayeePartsRegex.Match(ynabTransaction.Payee).Groups["payeeStr"].Value, ret);
            CreateAndOrAssignFlag(ynabTransaction.Flag, ret);
            return ret;
        }

        private SubTransaction TransformToSubTransaction(
            Transaction ynabTransaction, Trans parent)
        {
            SubTransaction ret =
                new SubTransaction
                {
                    Memo = ynabTransaction.Memo,
                    Sum = ynabTransaction.Inflow - ynabTransaction.Outflow
                };
            AssignCategory(ynabTransaction.Category, ret);
            if(!_parentTransactionAssignment.ContainsKey(parent))
                _parentTransactionAssignment.Add(parent, new List<SubTransaction> {ret});
            else _parentTransactionAssignment[parent].Add(ret);
            return ret;
        }

        #endregion
        
        #region Accounts

        private readonly IDictionary<string, Account> _accountCache = new Dictionary<string, Account>();

        private readonly IDictionary<Account, IList<IHaveAccount>> _accountAssignment =
            new Dictionary<Account, IList<IHaveAccount>>();

        private readonly IDictionary<Account, IList<Trans>> _fromAccountAssignment =
            new Dictionary<Account, IList<Trans>>();

        private readonly IDictionary<Account, IList<Trans>> _toAccountAssignment =
            new Dictionary<Account, IList<Trans>>();

        private void CreateAccount(string name, long startingBalance, DateTime startingDateTime)
        {
            if(string.IsNullOrWhiteSpace(name)) return;

            Account account = new Account
            {
                Name = name,
                StartingBalance = startingBalance,
                StartingDate = startingDateTime
            };
            _accountCache.Add(name, account);
            _accountAssignment.Add(account, new List<IHaveAccount>());
            _fromAccountAssignment.Add(account, new List<Trans>());
            _toAccountAssignment.Add(account, new List<Trans>());
        }

        private void AssignAccount(string name, IHaveAccount transNoTransfer)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            _accountAssignment[_accountCache[name]].Add(transNoTransfer);
        }

        private void AssignToAccount(string name, Trans transfer)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            _toAccountAssignment[_accountCache[name]].Add(transfer);
        }

        private void AssignFormAccount(string name, Trans transfer)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            _fromAccountAssignment[_accountCache[name]].Add(transfer);
        }

        private List<Account> GetAllAccountCache()
        {
            return _accountCache.Values.ToList();
        }

        private void ClearAccountCache()
        {
            _accountCache.Clear();
            _accountAssignment.Clear();
            _fromAccountAssignment.Clear();
            _toAccountAssignment.Clear();
        }

        #endregion

        #region Payees

        private readonly IDictionary<string, Payee> _payeeCache = new Dictionary<string, Payee>();
        private readonly IDictionary<Payee, IList<IHavePayee>> _payeeAssignment =
            new Dictionary<Payee, IList<IHavePayee>>();

        private void CreateAndOrAssignPayee(
            string name, IHavePayee transBase)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;
            if (!_payeeCache.ContainsKey(name))
            {
                Payee payee = new Payee { Name = name };
                _payeeCache.Add(name, payee);
                _payeeAssignment.Add(payee, new List<IHavePayee> { transBase });
            }
            else
            {
                _payeeAssignment[_payeeCache[name]].Add(transBase);
            }
        }

        private List<Payee> GetAllPayeeCache()
        {
            return _payeeCache.Values.ToList();
        }

        private void ClearPayeeCache()
        {
            _payeeCache.Clear();
            _payeeAssignment.Clear();
        }

        #endregion

        #region Flags

        private readonly IDictionary<string, Flag> _flagCache = new Dictionary<string, Flag>();
        private readonly IDictionary<Flag, IList<IHaveFlag>> _flagAssignment =
            new Dictionary<Flag, IList<IHaveFlag>>();

        private void CreateAndOrAssignFlag(
            string name, IHaveFlag transBase)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;
            if (!_flagCache.ContainsKey(name))
            {
                Flag flag = new Flag { Name = name };
                switch (name)
                {
                    case "Red":
                        flag.Color = 0xffff0000;
                        break;
                    case "Green":
                        flag.Color = 0xff00ff00;
                        break;
                    case "Blue":
                        flag.Color = 0xff0000ff;
                        break;
                    case "Orange":
                        flag.Color = 0xffffa500;
                        break;
                    case "Yellow":
                        flag.Color = 0xffffff00;
                        break;
                    case "Purple":
                        flag.Color = 0xff551A8B;
                        break;
                }
                _flagCache.Add(name, flag);
                _flagAssignment.Add(flag, new List<IHaveFlag> { transBase });
            }
            else
            {
                _flagAssignment[_flagCache[name]].Add(transBase);
            }
        }

        private List<Flag> GetAllFlagCache()
        {
            return _flagCache.Values.ToList();
        }

        private void ClearFlagCache()
        {
            _flagCache.Clear();
            _flagAssignment.Clear();
        }

        #endregion

        #region Categories

        private readonly IList<CategoryImportWrapper> _categoryImportWrappers = new List<CategoryImportWrapper>();

        private readonly CategoryImportWrapper _thisMonthCategoryImportWrapper = new CategoryImportWrapper
        {
            Category = new Category
            {
                Name = "This Month",
                IsIncomeRelevant = true,
                MonthOffset = 0,
                ParentId = null
            },
            Parent = null
        };

        private readonly CategoryImportWrapper _nextMonthCategoryImportWrapper = new CategoryImportWrapper
        {
            Category = new Category
            {
                Name = "Next Month",
                IsIncomeRelevant = true,
                MonthOffset = 1,
                ParentId = null
            },
            Parent = null
        };

        private void AssignCategory(
            string namePath, IHaveCategory transLike)
        {
            string masterCategoryName = namePath.Split(':').First();
            string subCategoryName = namePath.Split(':').Last();
            if (masterCategoryName == "Income")
            {
                if(subCategoryName == "Available this month")
                    _thisMonthCategoryImportWrapper.TransAssignments.Add(transLike);
                else
                    _nextMonthCategoryImportWrapper.TransAssignments.Add(transLike);
            }
            else
            {
                CategoryImportWrapper masterCategoryWrapper =
                    _categoryImportWrappers.SingleOrDefault(ciw => ciw.Category.Name == masterCategoryName);
                if (masterCategoryWrapper is null)
                {
                    Category category = new Category { Name = masterCategoryName };
                    masterCategoryWrapper = new CategoryImportWrapper { Parent = null, Category = category };
                    _categoryImportWrappers.Add(masterCategoryWrapper);
                }
                CategoryImportWrapper subCategoryWrapper =
                    masterCategoryWrapper.Categories.SingleOrDefault(c => c.Category.Name == subCategoryName);
                if (subCategoryWrapper is null)
                {
                    Category category = new Category { Name = subCategoryName };
                    subCategoryWrapper = new CategoryImportWrapper { Parent = masterCategoryWrapper, Category = category };
                    masterCategoryWrapper.Categories.Add(subCategoryWrapper);
                }
                subCategoryWrapper.TransAssignments.Add(transLike);
            }
        }

        private void ClearCategoryCache()
        {
            _categoryImportWrappers.Clear();
        }

        #endregion


    }
}
