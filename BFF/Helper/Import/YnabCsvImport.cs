using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BFF.DB;
using BFF.DB.Dapper;
using BFF.DB.SQLite;
using BFF.MVVM;
using Domain = BFF.MVVM.Models.Native;
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
            IProvideConnection provideConnection = new CreateSqLiteDatabase(savePath).Create();
            BffRepository bffRepository = new SqLiteBffOrm(provideConnection).BffRepository;
            _processedAccountsList.Clear();
            ClearAccountCache(); 
            ClearPayeeCache();
            ClearCategoryCache();
            
            //First step: Parse CSV data into conversion objects
            Queue<Transaction> ynabTransactions = new Queue<Transaction>(ParseTransactionCsv(filePathTransaction));
            IEnumerable<BudgetEntry> budgets = ParseBudgetCsv(filePathBudget);

            ImportLists lists = new ImportLists
            {
                Accounts = new List<Domain.IAccount>(),
                Categories = new List<CategoryImportWrapper>(),
                Payees = new List<Domain.IPayee>(),
                Incomes = new List<Domain.IIncome>(),
                ParentIncomes = new List<Domain.IParentIncome>(),
                ParentTransactions = new List<Domain.IParentTransaction>(),
                SubIncomes = new List<Domain.ISubIncome>(),
                SubTransactions = new List<Domain.ISubTransaction>(),
                Transactions = new List<Domain.ITransaction>(),
                Transfers = new List<Domain.ITransfer>()
            };

            //Second step: Convert conversion objects into native models
            ConvertTransactionsToNative(ynabTransactions, lists, bffRepository);
            lists.BudgetEntries = ConvertBudgetEntryToNative(budgets, bffRepository).ToList();
            lists.Accounts = GetAllAccountCache();
            lists.Payees = GetAllPayeeCache();
            lists.Categories = _categoryImportWrappers;

            ImportAssignments assignments = new ImportAssignments
            {
                AccountToTransIncBase = _accountAssignment,
                FromAccountToTransfer = _fromAccountAssignment,
                ToAccountToTransfer = _toAccountAssignment,
                PayeeToTransIncBase = _payeeAssignment,
                ParentTransactionToSubTransaction = _parentTransactionAssignment,
                ParentIncomeToSubIncome = new Dictionary<Domain.IParentIncome, IList<Domain.ISubIncome>>() //In YNAB4 are no ParentIncomes
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
            Queue<Transaction> ynabTransactions, ImportLists lists, BffRepository bffRepository)
        {
            //Account pre-processing
            //First create all available Accounts. The reason for this is to make the Account all assignable from the beginning
            foreach(Transaction ynabTransaction in ynabTransactions
                .Where(ynabTransaction => ynabTransaction.Payee == "Starting Balance"))
            {
                CreateAccount(ynabTransaction.Account, 
                              ynabTransaction.Inflow - ynabTransaction.Outflow, 
                              bffRepository.AccountRepository);
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
                    Domain.IParentTransaction parent = TransformToParentTransaction(ynabTransaction, bffRepository);
                    int splitCount = int.Parse(splitMatch.Groups[nameof(splitCount)].Value);
                    int count = 0;
                    for (int i = 0; i < splitCount; i++)
                    {
                        Transaction newYnabTransaction = i==0 ? ynabTransaction : ynabTransactions.Dequeue();
                        Match transferMatch = TransferPayeeRegex.Match(newYnabTransaction.Payee);
                        if (transferMatch.Success)
                            AddTransfer(lists.Transfers, newYnabTransaction, bffRepository.TransferRepository);
                        else if (newYnabTransaction.MasterCategory == "Income")
                            lists.Incomes.Add(TransformToIncome(newYnabTransaction, bffRepository));
                        else
                        {
                            Domain.ISubTransaction subTransaction = TransformToSubTransaction(
                                newYnabTransaction, parent, bffRepository);
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
                        AddTransfer(lists.Transfers, ynabTransaction, bffRepository.TransferRepository);
                    else if (ynabTransaction.MasterCategory == "Income")
                        lists.Incomes.Add(TransformToIncome(ynabTransaction, bffRepository));
                    else
                        lists.Transactions.Add(TransformToTransaction(ynabTransaction, bffRepository));
                }
            }
        }

        private IEnumerable<Domain.IBudgetEntry> ConvertBudgetEntryToNative(IEnumerable<BudgetEntry> ynabBudgetEntries, 
                                                                            BffRepository bffRepository)
        {
            IEnumerable<MVVM.Models.Native.IBudgetEntry> ConvertBudgetEntryToNativeInner()
            {
                foreach(var ynabBudgetEntry in ynabBudgetEntries)
                {
                    if(ynabBudgetEntry.Budgeted != 0L)
                    {
                        var month = DateTime.ParseExact(ynabBudgetEntry.Month, "MMMM yyyy", null);
                        Domain.IBudgetEntry budgetEntry = bffRepository.BudgetEntryRepository.Create();
                        budgetEntry.Month = month;
                        budgetEntry.Budget = ynabBudgetEntry.Budgeted;

                        AssignCategory(ynabBudgetEntry.Category, budgetEntry, bffRepository.CategoryRepository);
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
        private void AddTransfer(IList<Domain.ITransfer> transfers, 
                                 Transaction ynabTransfer,
                                 IRepository<Domain.ITransfer> transferRepository)
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
                transfers.Add(TransformToTransfer(ynabTransfer, transferRepository));
            }
        }

        #region Transformations

        /// <summary>
        /// Creates a Transaction-object depending on a YNAB-Transaction
        /// </summary>
        /// <param name="ynabTransaction">The YNAB-model</param>
        private Domain.ITransaction TransformToTransaction(Transaction ynabTransaction, BffRepository bffRepository)
        {
            Domain.ITransaction ret = bffRepository.TransactionRepository.Create();
            ret.Date = ynabTransaction.Date;
            ret.Memo = MemoPartsRegex.Match(ynabTransaction.Memo).Groups["parentTransMemo"].Value;
            ret.Sum = ynabTransaction.Inflow - ynabTransaction.Outflow;
            ret.Cleared = ynabTransaction.Cleared;
            AssignAccount(ynabTransaction.Account, ret);
            CreateAndOrAssignPayee(PayeePartsRegex.Match(ynabTransaction.Payee).Groups["payeeStr"].Value, 
                                   ret,
                                   bffRepository.PayeeRepository);
            AssignCategory(ynabTransaction.Category, ret, bffRepository.CategoryRepository);
            return ret;
        }

        /// <summary>
        /// Creates a Income-object depending on a YNAB-Transaction
        /// </summary>
        /// <param name="ynabTransaction">The YNAB-model</param>
        private Domain.IIncome TransformToIncome(Transaction ynabTransaction, BffRepository bffRepository)
        {
            Domain.IIncome ret = bffRepository.IncomeRepository.Create();
            ret.Date = ynabTransaction.Date;
            ret.Memo = MemoPartsRegex.Match(ynabTransaction.Memo).Groups["parentTransMemo"].Value;
            ret.Sum = ynabTransaction.Inflow - ynabTransaction.Outflow;
            ret.Cleared = ynabTransaction.Cleared;
            AssignAccount(ynabTransaction.Account, ret);
            CreateAndOrAssignPayee(PayeePartsRegex.Match(ynabTransaction.Payee).Groups["payeeStr"].Value, 
                                   ret,
                                   bffRepository.PayeeRepository);
            AssignCategory(ynabTransaction.Category, ret, bffRepository.CategoryRepository);
            return ret;
        }

        /// <summary>
        /// Creates a Transfer-object depending on a YNAB-Transaction
        /// </summary>
        /// <param name="ynabTransaction">The YNAB-model</param>
        private Domain.ITransfer TransformToTransfer(Transaction ynabTransaction, 
                                                     IRepository<Domain.ITransfer> transferRepository)
        {
            long tempSum = ynabTransaction.Inflow - ynabTransaction.Outflow;
            Domain.ITransfer ret = transferRepository.Create();
            ret.Date = ynabTransaction.Date;
            ret.Memo = MemoPartsRegex.Match(ynabTransaction.Memo).Groups["parentTransMemo"].Value;
            ret.Sum = Math.Abs(tempSum);
            ret.Cleared = ynabTransaction.Cleared;
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

        private readonly IDictionary<Domain.IParentTransaction, IList<Domain.ISubTransaction>> 
            _parentTransactionAssignment = new Dictionary<Domain.IParentTransaction, IList<Domain.ISubTransaction>>();

        /// <summary>
        /// Creates a Transaction-object depending on a YNAB-Transaction
        /// </summary>
        /// <param name="ynabTransaction">The YNAB-model</param>
        private Domain.IParentTransaction TransformToParentTransaction(Transaction ynabTransaction, 
                                                                       BffRepository bffRepository)
        {
            Domain.IParentTransaction ret = bffRepository.ParentTransactionRepository.Create();
            ret.Date = ynabTransaction.Date;
            ret.Memo = MemoPartsRegex.Match(ynabTransaction.Memo).Groups["parentTransMemo"].Value;
            ret.Cleared = ynabTransaction.Cleared;
            AssignAccount(ynabTransaction.Account, ret);
            CreateAndOrAssignPayee(PayeePartsRegex.Match(ynabTransaction.Payee).Groups["payeeStr"].Value,
                                   ret, 
                                   bffRepository.PayeeRepository);
            return ret;
        }

        private Domain.ISubTransaction TransformToSubTransaction(
            Transaction ynabTransaction, Domain.IParentTransaction parent, BffRepository bffRepository)
        {
            Domain.ISubTransaction ret = bffRepository.SubTransactionRepository.Create();
            ret.Memo = MemoPartsRegex.Match(ynabTransaction.Memo).Groups["subTransMemo"].Value;
            ret.Sum = ynabTransaction.Inflow - ynabTransaction.Outflow;
            AssignCategory(ynabTransaction.Category, ret, bffRepository.CategoryRepository);
            if(!_parentTransactionAssignment.ContainsKey(parent))
                _parentTransactionAssignment.Add(parent, new List<Domain.ISubTransaction> {ret});
            else _parentTransactionAssignment[parent].Add(ret);
            return ret;
        }

        #endregion
        
        #region Accounts

        private readonly IDictionary<string, Domain.IAccount> _accountCache = new Dictionary<string, Domain.IAccount>();

        private readonly IDictionary<Domain.IAccount, IList<ITransIncBase>> _accountAssignment =
            new Dictionary<Domain.IAccount, IList<ITransIncBase>>();

        private readonly IDictionary<Domain.IAccount, IList<Domain.ITransfer>> _fromAccountAssignment =
            new Dictionary<Domain.IAccount, IList<Domain.ITransfer>>();

        private readonly IDictionary<Domain.IAccount, IList<Domain.ITransfer>> _toAccountAssignment =
            new Dictionary<Domain.IAccount, IList<Domain.ITransfer>>();

        private void CreateAccount(string name, long startingBalance, IRepository<Domain.IAccount> accountRepository)
        {
            if(string.IsNullOrWhiteSpace(name)) return;

            Domain.IAccount account = accountRepository.Create();
            account.Name = name;
            account.StartingBalance = startingBalance;
            _accountCache.Add(name, account);
            _accountAssignment.Add(account, new List<ITransIncBase>());
            _fromAccountAssignment.Add(account, new List<Domain.ITransfer>());
            _toAccountAssignment.Add(account, new List<Domain.ITransfer>());
        }

        private void AssignAccount(string name, ITransIncBase titNoTransfer)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            _accountAssignment[_accountCache[name]].Add(titNoTransfer);
        }

        private void AssignToAccount(string name, Domain.ITransfer transfer)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            _toAccountAssignment[_accountCache[name]].Add(transfer);
        }

        private void AssignFormAccount(string name, Domain.ITransfer transfer)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            _fromAccountAssignment[_accountCache[name]].Add(transfer);
        }

        private List<Domain.IAccount> GetAllAccountCache()
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

        private readonly IDictionary<string, Domain.IPayee> _payeeCache = new Dictionary<string, Domain.IPayee>();
        private readonly IDictionary<Domain.IPayee, IList<ITransIncBase>> _payeeAssignment = 
            new Dictionary<Domain.IPayee, IList<ITransIncBase>>();

        private void CreateAndOrAssignPayee(
            string name, ITransIncBase titBase, IRepository<Domain.IPayee> payeeRepository)
        {
            if(string.IsNullOrWhiteSpace(name))
                return;
            if(!_payeeCache.ContainsKey(name))
            {
                Domain.IPayee payee = payeeRepository.Create();
                payee.Name = name;
                _payeeCache.Add(name, payee);
                _payeeAssignment.Add(payee, new List<ITransIncBase> {titBase});
            }
            else
            {
                _payeeAssignment[_payeeCache[name]].Add(titBase);
            }
        }

        private List<Domain.IPayee> GetAllPayeeCache()
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

        private void AssignCategory(
            string namePath, IHaveCategory titLike, IRepository<Domain.ICategory> categoryRepository)
        {
            string masterCategoryName = namePath.Split(':').First();
            string subCategoryName = namePath.Split(':').Last();
            CategoryImportWrapper masterCategoryWrapper =
                _categoryImportWrappers.SingleOrDefault(ciw => ciw.Category.Name == masterCategoryName);
            if(masterCategoryWrapper == null)
            {
                Domain.ICategory category = categoryRepository.Create();
                category.Name = masterCategoryName;
                masterCategoryWrapper = new CategoryImportWrapper { Parent = null, Category = category };
                _categoryImportWrappers.Add(masterCategoryWrapper);
            }
            CategoryImportWrapper subCategoryWrapper =
                masterCategoryWrapper.Categories.SingleOrDefault(c => c.Category.Name == subCategoryName);
            if(subCategoryWrapper == null)
            {
                Domain.ICategory category = categoryRepository.Create();
                category.Name = subCategoryName;
                subCategoryWrapper = new CategoryImportWrapper {Parent = masterCategoryWrapper, Category = category};
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
