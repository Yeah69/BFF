using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BFF.DB;
using BFF.DB.Dapper;
using BFF.DB.SQLite;
using BFF.Helper.Extensions;
using BFF.MVVM;
using Persistence = BFF.DB.PersistenceModels;
using NLog;
using BudgetEntry = BFF.MVVM.Models.Conversion.YNAB.BudgetEntry;
using Transaction = BFF.MVVM.Models.Conversion.YNAB.Transaction;

namespace BFF.Helper.Import
{
    class YnabCsvImport : ObservableObject, IImportable
    {
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

        public YnabCsvImport(string transactionPath, string budgetPath, string savePath)
        {
            _transactionPath = transactionPath;
            _budgetPath = budgetPath;
            _savePath = savePath;
        }

        public string Import()
        {

            string exceptionTemplate = "Exception_FileNotFound".Localize<string>();
            if (!File.Exists(TransactionPath))
                throw new FileNotFoundException(string.Format(exceptionTemplate, TransactionPath));
            if(!File.Exists(BudgetPath))
                throw new FileNotFoundException(string.Format(exceptionTemplate, BudgetPath));
            if (File.Exists(SavePath))
                File.Delete(SavePath); //todo: Exception handling
            ImportYnabTransactionsCsvToDb(TransactionPath, BudgetPath, SavePath);
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

        public YnabCsvImport(){ }

        public void ImportYnabTransactionsCsvToDb(string filePathTransaction, string filePathBudget, string savePath)
        {
            //Initialization
            IProvideConnection provideConnection = new CreateSqLiteDatabase(savePath).Create();
            IBffRepository bffRepository = new SqLiteBffOrm(provideConnection).BffRepository;
            _processedAccountsList.Clear();
            ClearAccountCache(); 
            ClearPayeeCache();
            ClearCategoryCache();
            
            //First step: Parse CSV data into conversion objects
            Queue<Transaction> ynabTransactions = new Queue<Transaction>(ParseTransactionCsv(filePathTransaction));
            IEnumerable<BudgetEntry> budgets = ParseBudgetCsv(filePathBudget);

            ImportLists lists = new ImportLists
            {
                Accounts = new List<Persistence.Account>(),
                Categories = new List<CategoryImportWrapper>(),
                Payees = new List<Persistence.Payee>(),
                Incomes = new List<Persistence.Income>(),
                ParentIncomes = new List<Persistence.ParentIncome>(),
                ParentTransactions = new List<Persistence.ParentTransaction>(),
                SubIncomes = new List<Persistence.SubIncome>(),
                SubTransactions = new List<Persistence.SubTransaction>(),
                Transactions = new List<Persistence.Transaction>(),
                Transfers = new List<Persistence.Transfer>()
            };

            //Second step: Convert conversion objects into native models
            ConvertTransactionsToNative(ynabTransactions, lists);
            lists.BudgetEntries = ConvertBudgetEntryToNative(budgets).ToList();
            lists.Accounts = GetAllAccountCache();
            lists.Payees = GetAllPayeeCache();
            _categoryImportWrappers.Add(_thisMonthCategoryImportWrapper);
            _categoryImportWrappers.Add(_nextMonthCategoryImportWrapper);
            lists.Categories = _categoryImportWrappers;

            ImportAssignments assignments = new ImportAssignments
            {
                AccountToTransIncBase = _accountAssignment,
                FromAccountToTransfer = _fromAccountAssignment,
                ToAccountToTransfer = _toAccountAssignment,
                PayeeToTransIncBase = _payeeAssignment,
                ParentTransactionToSubTransaction = _parentTransactionAssignment,
                ParentIncomeToSubIncome = new Dictionary<Persistence.ParentIncome, IList<Persistence.SubIncome>>() //In YNAB4 are no ParentIncomes
            };

            //Third step: Create new database for imported data
            bffRepository.PopulateDatabase(lists, assignments);
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
                    Transaction.ToOutput(ret.Last());
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
                    else if (ynabTransaction.MasterCategory == "Income")
                        lists.Transactions.Add(TransformToIncome(ynabTransaction));
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

            Persistence.ParentTransaction parent = TransformToParentTransaction(ynabTransaction, parentMemo);

            bool createdSubTransactions = false;
            foreach (var splitTransaction in splitTransactions)
            {
                Match transferMatch = TransferPayeeRegex.Match(splitTransaction.Payee);
                if (transferMatch.Success)
                    AddTransfer(lists.Transfers, splitTransaction);
                else if (splitTransaction.MasterCategory == "Income")
                    lists.Transactions.Add(TransformToIncome(splitTransaction));
                else
                {
                    Persistence.SubTransaction subTransaction = TransformToSubTransaction(
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

        private IEnumerable<Persistence.BudgetEntry> ConvertBudgetEntryToNative(IEnumerable<BudgetEntry> ynabBudgetEntries)
        {
            IEnumerable<Persistence.BudgetEntry> ConvertBudgetEntryToNativeInner()
            {
                foreach(var ynabBudgetEntry in ynabBudgetEntries)
                {
                    if(ynabBudgetEntry.Budgeted != 0L)
                    {
                        var month = DateTime.ParseExact(ynabBudgetEntry.Month, "MMMM yyyy", null);
                        Persistence.BudgetEntry budgetEntry = new Persistence.BudgetEntry
                        {
                            Month = month,
                            Budget = ynabBudgetEntry.Budgeted
                        };

                        AssignCategory(ynabBudgetEntry.Category, budgetEntry);
                        yield return budgetEntry;
                    }
                }
            }
            
            if(ynabBudgetEntries == null) throw new ArgumentNullException(nameof(ynabBudgetEntries));

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
        private void AddTransfer(IList<Persistence.Transfer> transfers, 
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
        private Persistence.Transaction TransformToTransaction(Transaction ynabTransaction)
        {
            Persistence.Transaction ret = new Persistence.Transaction
            {
                Date = ynabTransaction.Date,
                Memo = ynabTransaction.Memo,
                Sum = ynabTransaction.Inflow - ynabTransaction.Outflow,
                Cleared = ynabTransaction.Cleared ? 1 : 0
            };
            AssignAccount(ynabTransaction.Account, ret);
            CreateAndOrAssignPayee(PayeePartsRegex.Match(ynabTransaction.Payee).Groups["payeeStr"].Value, ret);
            AssignCategory(ynabTransaction.Category, ret);
            return ret;
        }

        /// <summary>
        /// Creates a Income-object depending on a YNAB-Transaction
        /// </summary>
        /// <param name="ynabTransaction">The YNAB-model</param>
        private Persistence.Transaction TransformToIncome(Transaction ynabTransaction)
        {
            Persistence.Transaction ret = new Persistence.Transaction
            {
                Date = ynabTransaction.Date,
                Memo = ynabTransaction.Memo,
                Sum = ynabTransaction.Inflow - ynabTransaction.Outflow,
                Cleared = ynabTransaction.Cleared ? 1 : 0
            };
            AssignAccount(ynabTransaction.Account, ret);
            CreateAndOrAssignPayee(PayeePartsRegex.Match(ynabTransaction.Payee).Groups["payeeStr"].Value, ret);
            AssignCategory(ynabTransaction.Category, ret);
            return ret;
        }

        /// <summary>
        /// Creates a Transfer-object depending on a YNAB-Transaction
        /// </summary>
        /// <param name="ynabTransaction">The YNAB-model</param>
        private Persistence.Transfer TransformToTransfer(Transaction ynabTransaction)
        {
            long tempSum = ynabTransaction.Inflow - ynabTransaction.Outflow;
            Persistence.Transfer ret = new Persistence.Transfer
            {
                Date = ynabTransaction.Date,
                Memo = ynabTransaction.Memo,
                Sum = Math.Abs(tempSum),
                Cleared = ynabTransaction.Cleared ? 1 : 0
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
            return ret;
        }

        private readonly IDictionary<Persistence.ParentTransaction, IList<Persistence.SubTransaction>> 
            _parentTransactionAssignment = new Dictionary<Persistence.ParentTransaction, IList<Persistence.SubTransaction>>();

        /// <summary>
        /// Creates a Transaction-object depending on a YNAB-Transaction
        /// </summary>
        /// <param name="ynabTransaction">The YNAB-model</param>
        /// <param name="parentMemo">Parent memo which is extracted from the split transactions</param>
        private Persistence.ParentTransaction TransformToParentTransaction(Transaction ynabTransaction, string parentMemo)
        {
            Persistence.ParentTransaction ret = new Persistence.ParentTransaction
            {
                Date = ynabTransaction.Date,
                Memo = parentMemo,
                Cleared = ynabTransaction.Cleared ? 1 : 0
            };
            AssignAccount(ynabTransaction.Account, ret);
            CreateAndOrAssignPayee(PayeePartsRegex.Match(ynabTransaction.Payee).Groups["payeeStr"].Value, ret);
            return ret;
        }

        private Persistence.SubTransaction TransformToSubTransaction(
            Transaction ynabTransaction, Persistence.ParentTransaction parent)
        {
            Persistence.SubTransaction ret =
                new Persistence.SubTransaction
                {
                    Memo = ynabTransaction.Memo,
                    Sum = ynabTransaction.Inflow - ynabTransaction.Outflow
                };
            AssignCategory(ynabTransaction.Category, ret);
            if(!_parentTransactionAssignment.ContainsKey(parent))
                _parentTransactionAssignment.Add(parent, new List<Persistence.SubTransaction> {ret});
            else _parentTransactionAssignment[parent].Add(ret);
            return ret;
        }

        #endregion
        
        #region Accounts

        private readonly IDictionary<string, Persistence.Account> _accountCache = new Dictionary<string, Persistence.Account>();

        private readonly IDictionary<Persistence.Account, IList<Persistence.IHaveAccount>> _accountAssignment =
            new Dictionary<Persistence.Account, IList<Persistence.IHaveAccount>>();

        private readonly IDictionary<Persistence.Account, IList<Persistence.Transfer>> _fromAccountAssignment =
            new Dictionary<Persistence.Account, IList<Persistence.Transfer>>();

        private readonly IDictionary<Persistence.Account, IList<Persistence.Transfer>> _toAccountAssignment =
            new Dictionary<Persistence.Account, IList<Persistence.Transfer>>();

        private void CreateAccount(string name, long startingBalance, DateTime startingDateTime)
        {
            if(string.IsNullOrWhiteSpace(name)) return;

            Persistence.Account account = new Persistence.Account
            {
                Name = name,
                StartingBalance = startingBalance,
                StartingDate = startingDateTime
            };
            _accountCache.Add(name, account);
            _accountAssignment.Add(account, new List<Persistence.IHaveAccount>());
            _fromAccountAssignment.Add(account, new List<Persistence.Transfer>());
            _toAccountAssignment.Add(account, new List<Persistence.Transfer>());
        }

        private void AssignAccount(string name, Persistence.IHaveAccount titNoTransfer)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            _accountAssignment[_accountCache[name]].Add(titNoTransfer);
        }

        private void AssignToAccount(string name, Persistence.Transfer transfer)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            _toAccountAssignment[_accountCache[name]].Add(transfer);
        }

        private void AssignFormAccount(string name, Persistence.Transfer transfer)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            _fromAccountAssignment[_accountCache[name]].Add(transfer);
        }

        private List<Persistence.Account> GetAllAccountCache()
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

        private readonly IDictionary<string, Persistence.Payee> _payeeCache = new Dictionary<string, Persistence.Payee>();
        private readonly IDictionary<Persistence.Payee, IList<Persistence.IHavePayee>> _payeeAssignment = 
            new Dictionary<Persistence.Payee, IList<Persistence.IHavePayee>>();

        private void CreateAndOrAssignPayee(
            string name, Persistence.IHavePayee titBase)
        {
            if(string.IsNullOrWhiteSpace(name))
                return;
            if(!_payeeCache.ContainsKey(name))
            {
                Persistence.Payee payee = new Persistence.Payee {Name = name};
                _payeeCache.Add(name, payee);
                _payeeAssignment.Add(payee, new List<Persistence.IHavePayee> {titBase});
            }
            else
            {
                _payeeAssignment[_payeeCache[name]].Add(titBase);
            }
        }

        private List<Persistence.Payee> GetAllPayeeCache()
        {
            return _payeeCache.Values.ToList();
        }

        private void ClearPayeeCache()
        {
            _payeeCache.Clear();
            _payeeAssignment.Clear();
        }

        #endregion

        #region Categories

        private readonly IList<CategoryImportWrapper> _categoryImportWrappers = new List<CategoryImportWrapper>();

        private readonly CategoryImportWrapper _thisMonthCategoryImportWrapper = new CategoryImportWrapper
        {
            Category = new Persistence.Category
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
            Category = new Persistence.Category
            {
                Name = "Next Month",
                IsIncomeRelevant = true,
                MonthOffset = 1,
                ParentId = null
            },
            Parent = null
        };

        private void AssignCategory(
            string namePath, Persistence.IHaveCategory titLike)
        {
            string masterCategoryName = namePath.Split(':').First();
            string subCategoryName = namePath.Split(':').Last();
            if (masterCategoryName == "Income")
            {
                if(subCategoryName == "Available this month")
                    _thisMonthCategoryImportWrapper.TitAssignments.Add(titLike);
                else
                    _nextMonthCategoryImportWrapper.TitAssignments.Add(titLike);
            }
            else
            {
                CategoryImportWrapper masterCategoryWrapper =
                    _categoryImportWrappers.SingleOrDefault(ciw => ciw.Category.Name == masterCategoryName);
                if (masterCategoryWrapper == null)
                {
                    Persistence.Category category = new Persistence.Category { Name = masterCategoryName };
                    masterCategoryWrapper = new CategoryImportWrapper { Parent = null, Category = category };
                    _categoryImportWrappers.Add(masterCategoryWrapper);
                }
                CategoryImportWrapper subCategoryWrapper =
                    masterCategoryWrapper.Categories.SingleOrDefault(c => c.Category.Name == subCategoryName);
                if (subCategoryWrapper == null)
                {
                    Persistence.Category category = new Persistence.Category { Name = subCategoryName };
                    subCategoryWrapper = new CategoryImportWrapper { Parent = masterCategoryWrapper, Category = category };
                    masterCategoryWrapper.Categories.Add(subCategoryWrapper);
                }
                subCategoryWrapper.TitAssignments.Add(titLike);
            }
        }

        private void ClearCategoryCache()
        {
            _categoryImportWrappers.Clear();
        }

        #endregion


    }
}
