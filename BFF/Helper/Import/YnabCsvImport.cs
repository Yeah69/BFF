using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using BFF.DB.SQLite;
using BFF.MVVM;
using BFF.MVVM.Models.Conversion.YNAB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.Properties;
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

            string exceptionTemplate = (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("Exception_FileNotFound", null, Settings.Default.Culture_DefaultLanguage);
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
        internal static readonly Regex MemoPartsRegex = new Regex(@"^(\(Split (?<splitNumber>\d+)/(?<splitCount>\d+)\) )?((?<subTransMemo>.*) / )?(?<parentTransMemo>.*)$");

        internal static long ExtractLong(string text)
        {
            string number = text.ToCharArray().Where(char.IsDigit).Aggregate("", (current, character) => $"{current}{character}");
            return number == "" ? 0L : long.Parse(number);
        }

        public YnabCsvImport(){ }

        public void ImportYnabTransactionsCsvToDb(string filePathTransaction, string filePathBudget, string savePath)
        {
            //Initialization
            _processedAccountsList.Clear();
            ClearAccountCache(); 
            ClearPayeeCache();
            ClearCategoryCache();
            
            //First step: Parse CSV data into conversion objects
            Queue<Transaction> ynabTransactions = new Queue<Transaction>(ParseTransactionCsv(filePathTransaction));
            IEnumerable<BudgetEntry> budgets = ParseBudgetCsv(filePathBudget);

            ImportLists lists = new ImportLists
            {
                Accounts = new List<IAccount>(),
                Categories = new List<CategoryImportWrapper>(),
                Payees = new List<IPayee>(),
                Incomes = new List<IIncome>(),
                ParentIncomes = new List<IParentIncome>(),
                ParentTransactions = new List<IParentTransaction>(),
                SubIncomes = new List<ISubIncome>(),
                SubTransactions = new List<ISubTransaction>(),
                Transactions = new List<ITransaction>(),
                Transfers = new List<ITransfer>()
            };

            //Second step: Convert conversion objects into native models
            ConvertTransactionsToNative(ynabTransactions, lists);
            lists.BudgetEntries = ConvertBudgetEntryToNative(budgets).ToList();
            lists.Accounts = GetAllAccountCache();
            lists.Payees = GetAllPayeeCache();
            lists.Categories = _categoryImportWrappers;

            ImportAssignments assignments = new ImportAssignments
            {
                AccountToTransIncBase = _accountAssignment,
                FromAccountToTransfer = _fromAccountAssignment,
                ToAccountToTransfer = _toAccountAssignment,
                PayeeToTransIncBase = _payeeAssingment,
                ParentTransactionToSubTransaction = _parentTransactionAssignment,
                ParentIncomeToSubIncome = new Dictionary<IParentIncome, IList<ISubIncome>>() //In YNAB4 are no ParentIncomes
            };

            //Third step: Create new database for imported data
            SqLiteBffOrm orm = new SqLiteBffOrm(savePath);
            orm.CreateNewDatabase();
            orm.PopulateDatabase(lists, assignments);
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

        private void ConvertTransactionsToNative(Queue<Transaction> ynabTransactions, ImportLists lists)
        {
            //Account pre-processing
            //First create all available Accounts. The reason for this is to make the Account all assignable from the beginning
            foreach(Transaction ynabTransaction in ynabTransactions.Where(ynabTransaction => ynabTransaction.Payee == "Starting Balance"))
            {
                CreateAccount(ynabTransaction.Account, ynabTransaction.Inflow - ynabTransaction.Outflow);
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
                    IParentTransaction parent = TransformToParentTransaction(ynabTransaction);
                    int splitCount = int.Parse(splitMatch.Groups[nameof(splitCount)].Value);
                    int count = 0;
                    for (int i = 0; i < splitCount; i++)
                    {
                        Transaction newYnabTransaction = i==0 ? ynabTransaction : ynabTransactions.Dequeue();
                        Match transferMatch = TransferPayeeRegex.Match(newYnabTransaction.Payee);
                        if (transferMatch.Success)
                            AddTransfer(lists.Transfers, newYnabTransaction);
                        else if (newYnabTransaction.MasterCategory == "Income")
                            lists.Incomes.Add(TransformToIncome(newYnabTransaction));
                        else
                        {
                            ISubTransaction subTransaction = TransformToSubTransaction(newYnabTransaction, parent);
                            lists.SubTransactions.Add(subTransaction);
                            count++;
                        }
                    }
                    if (count > 0)
                    {
                        lists.ParentTransactions.Add(parent);
                    }
                }
                else
                {
                    Match transferMatch = TransferPayeeRegex.Match(ynabTransaction.Payee);
                    if (transferMatch.Success)
                        AddTransfer(lists.Transfers, ynabTransaction);
                    else if (ynabTransaction.MasterCategory == "Income")
                        lists.Incomes.Add(TransformToIncome(ynabTransaction));
                    else
                        lists.Transactions.Add(TransformToTransaction(ynabTransaction));
                }
            }
        }

        private IEnumerable<IBudgetEntry> ConvertBudgetEntryToNative(IEnumerable<BudgetEntry> ynabBudgetEntries)
        {
            IEnumerable<MVVM.Models.Native.BudgetEntry> ConvertBudgetEntryToNativeInner()
            {
                foreach(var ynabBudgetEntry in ynabBudgetEntries)
                {
                    if(ynabBudgetEntry.Budgeted != 0L)
                    {
                        var month = DateTime.ParseExact(ynabBudgetEntry.Month, "MMMM yyyy", null);
                        var budgetEntry = new MVVM.Models.Native.BudgetEntry(month)
                        {
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
           one time for each Account. Fortunatelly, the Accounts are processed consecutively.
           That way if one of the Accounts of the Transfer points to an already processed Account,
           then it means that this Transfer is already created and can be skipped. */
        private readonly List<string> _processedAccountsList = new List<string>();
        private void AddTransfer(IList<ITransfer> transfers, Transaction ynabTransfer)
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
        private ITransaction TransformToTransaction(Transaction ynabTransaction)
        {
            ITransaction ret = new MVVM.Models.Native.Transaction(ynabTransaction.Date)
            {
                Memo = MemoPartsRegex.Match(ynabTransaction.Memo).Groups["parentTransMemo"].Value,
                Sum = ynabTransaction.Inflow - ynabTransaction.Outflow,
                Cleared = ynabTransaction.Cleared
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
        private IIncome TransformToIncome(Transaction ynabTransaction)
        {
            IIncome ret = new Income(ynabTransaction.Date)
            {
                Memo = MemoPartsRegex.Match(ynabTransaction.Memo).Groups["parentTransMemo"].Value,
                Sum = ynabTransaction.Inflow - ynabTransaction.Outflow,
                Cleared = ynabTransaction.Cleared
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
        private ITransfer TransformToTransfer(Transaction ynabTransaction)
        {
            long tempSum = ynabTransaction.Inflow - ynabTransaction.Outflow;
            ITransfer ret = new Transfer(ynabTransaction.Date)
            {
                Memo = MemoPartsRegex.Match(ynabTransaction.Memo).Groups["parentTransMemo"].Value,
                Sum = Math.Abs(tempSum),
                Cleared = ynabTransaction.Cleared
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

        private readonly IDictionary<IParentTransaction, IList<ISubTransaction>> _parentTransactionAssignment =
            new Dictionary<IParentTransaction, IList<ISubTransaction>>();

        /// <summary>
        /// Creates a Transaction-object depending on a YNAB-Transaction
        /// </summary>
        /// <param name="ynabTransaction">The YNAB-model</param>
        private IParentTransaction TransformToParentTransaction(Transaction ynabTransaction)
        {
            IParentTransaction ret = new ParentTransaction(ynabTransaction.Date)
            {
                Memo = MemoPartsRegex.Match(ynabTransaction.Memo).Groups["parentTransMemo"].Value,
                Cleared = ynabTransaction.Cleared
            };
            AssignAccount(ynabTransaction.Account, ret);
            CreateAndOrAssignPayee(PayeePartsRegex.Match(ynabTransaction.Payee).Groups["payeeStr"].Value, ret);
            return ret;
        }

        private ISubTransaction TransformToSubTransaction(Transaction ynabTransaction, IParentTransaction parent)
        {
            ISubTransaction ret = new SubTransaction
            {
                Memo = MemoPartsRegex.Match(ynabTransaction.Memo).Groups["subTransMemo"].Value,
                Sum = ynabTransaction.Inflow - ynabTransaction.Outflow
            };
            AssignCategory(ynabTransaction.Category, ret);
            if(!_parentTransactionAssignment.ContainsKey(parent))
                _parentTransactionAssignment.Add(parent, new List<ISubTransaction> {ret});
            else _parentTransactionAssignment[parent].Add(ret);
            return ret;
        }

        #endregion
        
        #region Accounts

        private readonly IDictionary<string, IAccount> _accountCache = new Dictionary<string, IAccount>();

        private readonly IDictionary<IAccount, IList<ITransIncBase>> _accountAssignment =
            new Dictionary<IAccount, IList<ITransIncBase>>();

        private readonly IDictionary<IAccount, IList<ITransfer>> _fromAccountAssignment =
            new Dictionary<IAccount, IList<ITransfer>>();

        private readonly IDictionary<IAccount, IList<ITransfer>> _toAccountAssignment =
            new Dictionary<IAccount, IList<ITransfer>>();

        private void CreateAccount(string name, long startingBalance)
        {
            if(string.IsNullOrWhiteSpace(name)) return;
            IAccount account = new Account {Name = name, StartingBalance = startingBalance};
            _accountCache.Add(name, account);
            _accountAssignment.Add(account, new List<ITransIncBase>());
            _fromAccountAssignment.Add(account, new List<ITransfer>());
            _toAccountAssignment.Add(account, new List<ITransfer>());
        }

        private void AssignAccount(string name, ITransIncBase titNoTransfer)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            _accountAssignment[_accountCache[name]].Add(titNoTransfer);
        }

        private void AssignToAccount(string name, ITransfer transfer)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            _toAccountAssignment[_accountCache[name]].Add(transfer);
        }

        private void AssignFormAccount(string name, ITransfer transfer)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            _fromAccountAssignment[_accountCache[name]].Add(transfer);
        }

        private List<IAccount> GetAllAccountCache()
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

        private readonly IDictionary<string, IPayee> _payeeCache = new Dictionary<string, IPayee>();
        private readonly IDictionary<IPayee, IList<ITransIncBase>> _payeeAssingment = new Dictionary<IPayee, IList<ITransIncBase>>();

        private void CreateAndOrAssignPayee(string name, ITransIncBase titBase)
        {
            if(string.IsNullOrWhiteSpace(name))
                return;
            if(!_payeeCache.ContainsKey(name))
            {
                IPayee payee = new Payee(name: name);
                _payeeCache.Add(name, payee);
                _payeeAssingment.Add(payee, new List<ITransIncBase> {titBase});
            }
            else
            {
                _payeeAssingment[_payeeCache[name]].Add(titBase);
            }
        }

        private List<IPayee> GetAllPayeeCache()
        {
            return _payeeCache.Values.ToList();
        }

        private void ClearPayeeCache()
        {
            _payeeCache.Clear();
            _payeeAssingment.Clear();
        }

        #endregion

        #region Categories

        private readonly IList<CategoryImportWrapper> _categoryImportWrappers = new List<CategoryImportWrapper>();

        private void AssignCategory(string namePath, IHaveCategory titLike)
        {
            string masterCategoryName = namePath.Split(':').First();
            string subCategoryName = namePath.Split(':').Last();
            CategoryImportWrapper masterCategoryWrapper =
                _categoryImportWrappers.SingleOrDefault(ciw => ciw.Category.Name == masterCategoryName);
            if(masterCategoryWrapper == null)
            {
                masterCategoryWrapper = new CategoryImportWrapper { Parent = null, Category = new Category(masterCategoryName) };
                _categoryImportWrappers.Add(masterCategoryWrapper);
            }
            CategoryImportWrapper subCategoryWrapper =
                masterCategoryWrapper.Categories.SingleOrDefault(c => c.Category.Name == subCategoryName);
            if(subCategoryWrapper == null)
            {
                subCategoryWrapper = new CategoryImportWrapper {Parent = masterCategoryWrapper, Category = new Category(subCategoryName)};
                masterCategoryWrapper.Categories.Add(subCategoryWrapper);
            }
            subCategoryWrapper.TitAssignments.Add(titLike);
        }

        private void ClearCategoryCache()
        {
            _categoryImportWrappers.Clear();
        }

        #endregion


    }
}
