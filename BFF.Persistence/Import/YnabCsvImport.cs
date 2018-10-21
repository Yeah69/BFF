using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Core.Persistence;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;
using NLog;
using BudgetEntry = BFF.Persistence.Models.Import.YNAB.BudgetEntry;
using Transaction = BFF.Persistence.Models.Import.YNAB.Transaction;

namespace BFF.Persistence.Import
{
    public interface IYnab4Import : IImportable
    { }

    public class Ynab4Import : IYnab4Import
    {
        private readonly IYnab4ImportConfiguration _configuration;
        private readonly ILocalizer _localizer;
        private readonly IImportingOrm _importingOrm;
        private readonly ICreateBackendOrm _createBackendOrm;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Ynab4Import(
            IYnab4ImportConfiguration configuration,
            IImportingOrm createImportingOrm,
            ICreateBackendOrm createCreateBackendOrm,
            ILocalizer localizer)
        {
            _configuration = configuration;
            _localizer = localizer;
            _importingOrm = createImportingOrm;
            _createBackendOrm = createCreateBackendOrm;
        }

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
            ClearAccountCache(); 
            ClearPayeeCache();
            ClearCategoryCache();
            ClearFlagCache();
            
            //First step: Parse CSV data into conversion objects
            Queue<Transaction> ynabTransactions = new Queue<Transaction>(ParseTransactionCsv(_configuration.TransactionPath));
            IEnumerable<BudgetEntry> budgets = ParseBudgetCsv(_configuration.BudgetPath);

            ImportLists lists = new ImportLists
            {
                Accounts = new List<IAccountDto>(),
                Categories = new List<CategoryImportWrapper>(),
                Payees = new List<IPayeeDto>(),
                Flags = new List<IFlagDto>(),
                ParentTransactions = new List<ITransDto>(),
                SubTransactions = new List<ISubTransactionDto>(),
                Transactions = new List<ITransDto>(),
                Transfers = new List<ITransDto>()
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

            ITransDto parent = TransformToParentTransaction(ynabTransaction, parentMemo);

            bool createdSubTransactions = false;
            foreach (var splitTransaction in splitTransactions)
            {
                Match transferMatch = TransferPayeeRegex.Match(splitTransaction.Payee);
                if (transferMatch.Success)
                    AddTransfer(lists.Transfers, splitTransaction);
                else
                {
                    ISubTransactionDto subTransaction = TransformToSubTransaction(
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

        private IEnumerable<Persistence.Models.IBudgetEntryDto> ConvertBudgetEntryToNative(IEnumerable<BudgetEntry> ynabBudgetEntries)
        {
            IEnumerable<Persistence.Models.IBudgetEntryDto> ConvertBudgetEntryToNativeInner()
            {
                foreach(var ynabBudgetEntry in ynabBudgetEntries)
                {
                    if(ynabBudgetEntry.Budgeted != 0L)
                    {
                        var month = DateTime.ParseExact(ynabBudgetEntry.Month, "MMMM yyyy", CultureInfo.GetCultureInfo("de-DE")); // TODO make this customizable + exception handling
                        Persistence.Models.IBudgetEntryDto budgetEntry = new Persistence.Models.BudgetEntry
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

        /* The smart people of YNAB thought it would be a nice idea to put each Transfer two times into the export,
           one time for each Account. Fortunately, the Accounts are processed consecutively.
           That way if one of the Accounts of the Transfer points to an already processed Account,
           then it means that this Transfer is already created and can be skipped. */
        private readonly List<string> _processedAccountsList = new List<string>();
        private void AddTransfer(IList<ITransDto> transfers, 
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
        private ITransDto TransformToTransaction(Transaction ynabTransaction)
        {
            ITransDto ret = new Trans
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
        private ITransDto TransformToTransfer(Transaction ynabTransaction)
        {
            long tempSum = ynabTransaction.Inflow - ynabTransaction.Outflow;
            ITransDto ret = new Trans
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

        private readonly IDictionary<ITransDto, IList<ISubTransactionDto>> 
            _parentTransactionAssignment = new Dictionary<ITransDto, IList<ISubTransactionDto>>();

        /// <summary>
        /// Creates a Transaction-object depending on a YNAB-Transaction
        /// </summary>
        /// <param name="ynabTransaction">The YNAB-model</param>
        /// <param name="parentMemo">Parent memo which is extracted from the split transactions</param>
        private ITransDto TransformToParentTransaction(Transaction ynabTransaction, string parentMemo)
        {
            ITransDto ret = new Trans
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

        private ISubTransactionDto TransformToSubTransaction(
            Transaction ynabTransaction, ITransDto parent)
        {
            ISubTransactionDto ret =
                new SubTransaction
                {
                    Memo = ynabTransaction.Memo,
                    Sum = ynabTransaction.Inflow - ynabTransaction.Outflow
                };
            AssignCategory(ynabTransaction.Category, ret);
            if(!_parentTransactionAssignment.ContainsKey(parent))
                _parentTransactionAssignment.Add(parent, new List<ISubTransactionDto> {ret});
            else _parentTransactionAssignment[parent].Add(ret);
            return ret;
        }

        #endregion
        
        #region Accounts

        private readonly IDictionary<string, IAccountDto> _accountCache = new Dictionary<string, IAccountDto>();

        private readonly IDictionary<IAccountDto, IList<IHaveAccountDto>> _accountAssignment =
            new Dictionary<IAccountDto, IList<IHaveAccountDto>>();

        private readonly IDictionary<IAccountDto, IList<ITransDto>> _fromAccountAssignment =
            new Dictionary<IAccountDto, IList<ITransDto>>();

        private readonly IDictionary<IAccountDto, IList<ITransDto>> _toAccountAssignment =
            new Dictionary<IAccountDto, IList<ITransDto>>();

        private void CreateAccount(string name, long startingBalance, DateTime startingDateTime)
        {
            if(string.IsNullOrWhiteSpace(name)) return;

            IAccountDto account = new Account
            {
                Name = name,
                StartingBalance = startingBalance,
                StartingDate = startingDateTime
            };
            _accountCache.Add(name, account);
            _accountAssignment.Add(account, new List<IHaveAccountDto>());
            _fromAccountAssignment.Add(account, new List<ITransDto>());
            _toAccountAssignment.Add(account, new List<ITransDto>());
        }

        private void AssignAccount(string name, IHaveAccountDto transNoTransfer)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            _accountAssignment[_accountCache[name]].Add(transNoTransfer);
        }

        private void AssignToAccount(string name, ITransDto transfer)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            _toAccountAssignment[_accountCache[name]].Add(transfer);
        }

        private void AssignFormAccount(string name, ITransDto transfer)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            _fromAccountAssignment[_accountCache[name]].Add(transfer);
        }

        private List<IAccountDto> GetAllAccountCache()
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

        private readonly IDictionary<string, IPayeeDto> _payeeCache = new Dictionary<string, IPayeeDto>();
        private readonly IDictionary<IPayeeDto, IList<IHavePayeeDto>> _payeeAssignment =
            new Dictionary<IPayeeDto, IList<IHavePayeeDto>>();

        private void CreateAndOrAssignPayee(
            string name, IHavePayeeDto transBase)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;
            if (!_payeeCache.ContainsKey(name))
            {
                IPayeeDto payee = new Payee { Name = name };
                _payeeCache.Add(name, payee);
                _payeeAssignment.Add(payee, new List<IHavePayeeDto> { transBase });
            }
            else
            {
                _payeeAssignment[_payeeCache[name]].Add(transBase);
            }
        }

        private List<IPayeeDto> GetAllPayeeCache()
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

        private readonly IDictionary<string, IFlagDto> _flagCache = new Dictionary<string, IFlagDto>();
        private readonly IDictionary<IFlagDto, IList<IHaveFlagDto>> _flagAssignment =
            new Dictionary<IFlagDto, IList<IHaveFlagDto>>();

        private void CreateAndOrAssignFlag(
            string name, IHaveFlagDto transBase)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;
            if (!_flagCache.ContainsKey(name))
            {
                IFlagDto flag = new Flag { Name = name };
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
                _flagAssignment.Add(flag, new List<IHaveFlagDto> { transBase });
            }
            else
            {
                _flagAssignment[_flagCache[name]].Add(transBase);
            }
        }

        private List<IFlagDto> GetAllFlagCache()
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
            string namePath, IHaveCategoryDto transLike)
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
                    ICategoryDto category = new Category { Name = masterCategoryName };
                    masterCategoryWrapper = new CategoryImportWrapper { Parent = null, Category = category };
                    _categoryImportWrappers.Add(masterCategoryWrapper);
                }
                CategoryImportWrapper subCategoryWrapper =
                    masterCategoryWrapper.Categories.SingleOrDefault(c => c.Category.Name == subCategoryName);
                if (subCategoryWrapper is null)
                {
                    ICategoryDto category = new Category { Name = subCategoryName };
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
