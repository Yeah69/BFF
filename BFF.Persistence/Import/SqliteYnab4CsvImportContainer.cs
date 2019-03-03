using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.Persistence.Import.Models.YNAB;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;
using MoreLinq;

namespace BFF.Persistence.Import
{
    public interface ISqliteYnab4CsvImportContainerData
    {
        IReadOnlyList<IAccountSql> Accounts { get; }
        IReadOnlyList<IPayeeSql> Payees { get; }
        IReadOnlyList<CategoryImportWrapper> Categories { get; }
        IReadOnlyList<IFlagSql> Flags { get; }

        IReadOnlyList<ITransSql> Transactions { get; }
        IReadOnlyList<ITransSql> Transfers { get; }

        IReadOnlyList<ITransSql> ParentTransactions { get; }

        IReadOnlyList<ISubTransactionSql> SubTransactions { get; }

        IReadOnlyList<IBudgetEntrySql> BudgetEntries { get; }

        ISqliteYnab4CsvImportAssignments ImportAssignments { get; }
    }

    public interface ISqliteYnab4CsvImportAssignments
    {
        IReadOnlyDictionary<IAccountSql, IList<IHaveAccountSql>> AccountToTransactionBase { get; }
        IReadOnlyDictionary<IAccountSql, IList<ITransSql>> FromAccountToTransfer { get; }
        IReadOnlyDictionary<IAccountSql, IList<ITransSql>> ToAccountToTransfer { get; }
        IReadOnlyDictionary<IPayeeSql, IList<IHavePayeeSql>> PayeeToTransactionBase { get; }
        IReadOnlyDictionary<ITransSql, IList<ISubTransactionSql>> ParentTransactionToSubTransaction { get; }
        IReadOnlyDictionary<IFlagSql, IList<IHaveFlagSql>> FlagToTransBase { get; }
    }

    internal class SqliteYnab4CsvImportContainer : IYnab4CsvImportContainer, ISqliteYnab4CsvImportContainerData
    {
        private readonly IImportingOrm _importingOrm;
        private readonly ICreateBackendOrm _createBackendOrm;
        private readonly Func<Trans> _transFactory;
        private readonly Func<SubTransaction> _subTransactionFactory;
        private readonly Func<BudgetEntry> _budgetEntryFactory;
        private readonly Func<Account> _accountFactory;
        private readonly Func<Category> _categoryFactory;
        private readonly Func<Payee> _payeeFactory;
        private readonly Func<Flag> _flagFactory;

        private readonly IList<ITransSql> _transactions = new List<ITransSql>();
        private readonly IList<ITransSql> _transfers = new List<ITransSql>();
        private readonly IList<ITransSql> _parentTransactions = new List<ITransSql>();
        private readonly IList<ISubTransactionSql> _subTransactions = new List<ISubTransactionSql>();
        private readonly IList<IBudgetEntrySql> _budgetEntries = new List<IBudgetEntrySql>();

        public SqliteYnab4CsvImportContainer(
            IImportingOrm importingOrm,
            ICreateBackendOrm createBackendOrm,
            Func<Trans> transFactory,
            Func<SubTransaction> subTransactionFactory,
            Func<BudgetEntry> budgetEntryFactory,
            Func<Account> accountFactory,
            Func<Category> categoryFactory,
            Func<Payee> payeeFactory,
            Func<Flag> flagFactory)
        {
            _importingOrm = importingOrm;
            _createBackendOrm = createBackendOrm;
            _transFactory = transFactory;
            _subTransactionFactory = subTransactionFactory;
            _budgetEntryFactory = budgetEntryFactory;
            _accountFactory = accountFactory;
            _categoryFactory = categoryFactory;
            _payeeFactory = payeeFactory;
            _flagFactory = flagFactory;
            Transactions       = new List<ITransSql>();
            Transfers          = new List<ITransSql>();
            ParentTransactions = new List<ITransSql>();
            SubTransactions    = new List<ISubTransactionSql>();
            BudgetEntries      = new List<IBudgetEntrySql>();
            _categoryImportWrappers.Add(_thisMonthCategoryImportWrapper);
            _categoryImportWrappers.Add(_nextMonthCategoryImportWrapper);

            Transactions = _transactions.ToReadOnlyList();
            Transfers = _transfers.ToReadOnlyList();
            ParentTransactions = _parentTransactions.ToReadOnlyList();
            SubTransactions = _subTransactions.ToReadOnlyList();
            BudgetEntries = _budgetEntries.ToReadOnlyList();
                
            var thisMonthIncomeCategory = categoryFactory();
                thisMonthIncomeCategory.Name = Ynab4CsvImportBase.IncomeCategoryThisMonthSubName;
                thisMonthIncomeCategory.IsIncomeRelevant = true;
                thisMonthIncomeCategory.MonthOffset = 0;
                thisMonthIncomeCategory.ParentId = null;

            _thisMonthCategoryImportWrapper = new CategoryImportWrapper
            {
                Category = thisMonthIncomeCategory,
                Parent = null
            };

            var nextMonthIncomeCategory = categoryFactory();
            nextMonthIncomeCategory.Name = Ynab4CsvImportBase.IncomeCategoryNextMonthSubName;
            nextMonthIncomeCategory.IsIncomeRelevant = true;
            nextMonthIncomeCategory.MonthOffset = 1;
            nextMonthIncomeCategory.ParentId = null;

            _thisMonthCategoryImportWrapper = new CategoryImportWrapper
            {
                Category = nextMonthIncomeCategory,
                Parent = null
            };
        }

        public IReadOnlyList<IAccountSql> Accounts => _accountCache.Values.ToReadOnlyList();
        public IReadOnlyList<IPayeeSql> Payees => _payeeCache.Values.ToReadOnlyList();
        public IReadOnlyList<CategoryImportWrapper> Categories => _categoryImportWrappers.ToReadOnlyList();
        public IReadOnlyList<IFlagSql> Flags => _flagCache.Values.ToReadOnlyList();

        public IReadOnlyList<ITransSql> Transactions { get; }
        public IReadOnlyList<ITransSql> Transfers { get; }

        public IReadOnlyList<ITransSql> ParentTransactions { get; }

        public IReadOnlyList<ISubTransactionSql> SubTransactions { get; }

        public IReadOnlyList<IBudgetEntrySql> BudgetEntries { get; }

        public ISqliteYnab4CsvImportAssignments ImportAssignments =>
            new ImportAssignmentsInner(
                _accountAssignment.ToReadOnlyDictionary(),
                _fromAccountAssignment.ToReadOnlyDictionary(),
                _toAccountAssignment.ToReadOnlyDictionary(),
                _payeeAssignment.ToReadOnlyDictionary(),
                _parentTransactionAssignment.ToReadOnlyDictionary(),
                _flagAssignment.ToReadOnlyDictionary());

        public void SetAccountStartingBalance(string name, long balance)
        {
            var account = GetOrCreateAccount(name);
            account.StartingBalance = balance;
        }

        public void TrySetAccountStartingDate(string name, DateTime date)
        {
            var account = GetOrCreateAccount(name);
            if(date < account.StartingDate) account.StartingDate = date;
        }

        public void AddAsTransfer(Ynab4Transaction ynab4Transaction)
        {
            long tempSum = ynab4Transaction.Inflow - ynab4Transaction.Outflow;
            ITransSql transfer = _transFactory();
            transfer.AccountId = -69;
            transfer.CheckNumber = ynab4Transaction.CheckNumber;
            transfer.Date = ynab4Transaction.Date;
            transfer.Memo = ynab4Transaction.Memo;
            transfer.Sum = Math.Abs(tempSum);
            transfer.Cleared = ynab4Transaction.Cleared ? 1 : 0;
            transfer.Type = nameof(TransType.Transfer);

            string fromAccount = ynab4Transaction.GetFromAccountName();
            string toAccount = ynab4Transaction.GetToAccountName();
            AssignFromAccount(fromAccount, transfer);
            AssignToAccount(toAccount, transfer);
            TrySetAccountStartingDate(fromAccount, transfer.Date);
            TrySetAccountStartingDate(toAccount, transfer.Date);

            AssignFlag(ynab4Transaction.Flag, transfer);
            _transfers.Add(transfer);
        }
        
        public void AddAsTransaction(Ynab4Transaction ynabTransaction)
        {
            ITransSql transaction = _transFactory();
            transaction.CheckNumber = ynabTransaction.CheckNumber;
            transaction.Date = ynabTransaction.Date;
            transaction.Memo = ynabTransaction.Memo;
            transaction.Sum = ynabTransaction.Inflow - ynabTransaction.Outflow;
            transaction.Cleared = ynabTransaction.Cleared ? 1 : 0;
            transaction.Type = nameof(TransType.Transaction);

            AssignAccount(ynabTransaction.Account, transaction);
            TrySetAccountStartingDate(ynabTransaction.Account, ynabTransaction.Date);
            AssignPayee(ynabTransaction.ParsePayeeFromPayee(), transaction);
            AssignCategory(ynabTransaction.Category, transaction);
            AssignFlag(ynabTransaction.Flag, transaction);
            _transactions.Add(transaction);
        }

        public void AddParentAndSubTransactions(Ynab4Transaction parent, string parentMemo, IEnumerable<Ynab4Transaction> subTransactions)
        {
            var parentTrans = AddAsParentTransaction(parent, parentMemo);
            subTransactions.ForEach(st => AddAsSubTransaction(st, parentTrans));
        }

        public void AddBudgetEntry(Ynab4BudgetEntry budgetEntry)
        {
            if (budgetEntry.Budgeted != 0L)
            {
                var month = DateTime.ParseExact(budgetEntry.Month, "MMMM yyyy", CultureInfo.GetCultureInfo("de-DE")); // TODO make this customizable + exception handling
                IBudgetEntrySql budgetEntrySql = _budgetEntryFactory();
                budgetEntrySql.Month = month;
                budgetEntrySql.Budget = budgetEntry.Budgeted;

                AssignCategory(budgetEntry.Category, budgetEntrySql);
                _budgetEntries.Add(budgetEntrySql);
            }
        }

        public async Task SaveIntoDatabase()
        {
            await _createBackendOrm.CreateAsync().ConfigureAwait(false);
            await _importingOrm.PopulateDatabaseAsync(this).ConfigureAwait(false);
        }


        private ITransSql AddAsParentTransaction(Ynab4Transaction ynabTransaction, string parentMemo)
        {
            ITransSql parentTransaction = _transFactory();
            parentTransaction.CategoryId = -69;
            parentTransaction.CheckNumber = ynabTransaction.CheckNumber;
            parentTransaction.Date = ynabTransaction.Date;
            parentTransaction.Memo = parentMemo;
            parentTransaction.Sum = -69;
            parentTransaction.Cleared = ynabTransaction.Cleared ? 1 : 0;
            parentTransaction.Type = nameof(TransType.ParentTransaction);

            AssignAccount(ynabTransaction.Account, parentTransaction);
            TrySetAccountStartingDate(ynabTransaction.Account, ynabTransaction.Date);
            AssignPayee(ynabTransaction.ParsePayeeFromPayee(), parentTransaction);
            AssignFlag(ynabTransaction.Flag, parentTransaction);
            _parentTransactions.Add(parentTransaction);
            return parentTransaction;
        }

        private readonly IDictionary<ITransSql, IList<ISubTransactionSql>>
            _parentTransactionAssignment = new Dictionary<ITransSql, IList<ISubTransactionSql>>();


        private void AddAsSubTransaction(
            Ynab4Transaction ynabTransaction, ITransSql parent)
        {
            ISubTransactionSql subTransaction = _subTransactionFactory();
            subTransaction.Memo = ynabTransaction.Memo;
            subTransaction.Sum = ynabTransaction.Inflow - ynabTransaction.Outflow;

            AssignCategory(ynabTransaction.Category, subTransaction);
            if (!_parentTransactionAssignment.ContainsKey(parent))
                _parentTransactionAssignment.Add(parent, new List<ISubTransactionSql> { subTransaction });
            else _parentTransactionAssignment[parent].Add(subTransaction);
            _subTransactions.Add(subTransaction);
        }

        #region Accounts

        private readonly IDictionary<string, IAccountSql> _accountCache = new Dictionary<string, IAccountSql>();

        private readonly IDictionary<IAccountSql, IList<IHaveAccountSql>> _accountAssignment =
            new Dictionary<IAccountSql, IList<IHaveAccountSql>>();

        private readonly IDictionary<IAccountSql, IList<ITransSql>> _fromAccountAssignment =
            new Dictionary<IAccountSql, IList<ITransSql>>();

        private readonly IDictionary<IAccountSql, IList<ITransSql>> _toAccountAssignment =
            new Dictionary<IAccountSql, IList<ITransSql>>();

        private void AssignAccount(string name, IHaveAccountSql transNoTransfer)
        {
            _accountAssignment.AddToKey(GetOrCreateAccount(name), transNoTransfer);
        }

        private void AssignToAccount(string name, ITransSql transfer)
        {
            _toAccountAssignment.AddToKey(GetOrCreateAccount(name), transfer);
        }

        private void AssignFromAccount(string name, ITransSql transfer)
        {
            _fromAccountAssignment.AddToKey(GetOrCreateAccount(name), transfer);
        }

        private IAccountSql GetOrCreateAccount(string name)
        {
            if(name.IsNullOrEmpty()) throw new ArgumentException("Account name cannot be empty!", nameof(name));

            if (_accountCache.TryGetValue(name, out var account)) return account;

            account = _accountFactory();
            account.Name = name;

            _accountCache[name] = account;

            return account;
        }

        #endregion
        
        #region Categories

        private readonly IList<CategoryImportWrapper> _categoryImportWrappers = new List<CategoryImportWrapper>();

        private readonly CategoryImportWrapper _thisMonthCategoryImportWrapper;

        private readonly CategoryImportWrapper _nextMonthCategoryImportWrapper;

        private void AssignCategory(
            string namePath, IHaveCategorySql transLike)
        {
            (string master, string sub) = Ynab4CsvImportBase.SplitCategoryNamePath(namePath);
            if (Ynab4CsvImportBase.IsIncomeThisMonth(master, sub))
            {
                _thisMonthCategoryImportWrapper.TransAssignments.Add(transLike);
            }
            if (Ynab4CsvImportBase.IsIncomeNextMonth(master, sub))
            {
                _nextMonthCategoryImportWrapper.TransAssignments.Add(transLike);
            }
            else
            {
                CategoryImportWrapper masterCategoryWrapper =
                    _categoryImportWrappers.SingleOrDefault(ciw => ciw.Category.Name == master);
                if (masterCategoryWrapper is null)
                {
                    ICategorySql category = _categoryFactory();
                    category.Name = master;
                    masterCategoryWrapper = new CategoryImportWrapper { Parent = null, Category = category };
                    _categoryImportWrappers.Add(masterCategoryWrapper);
                }
                CategoryImportWrapper subCategoryWrapper =
                    masterCategoryWrapper.Categories.SingleOrDefault(c => c.Category.Name == sub);
                if (subCategoryWrapper is null)
                {
                    ICategorySql category = _categoryFactory();
                    category.Name = sub;
                    subCategoryWrapper = new CategoryImportWrapper { Parent = masterCategoryWrapper, Category = category };
                    masterCategoryWrapper.Categories.Add(subCategoryWrapper);
                }
                subCategoryWrapper.TransAssignments.Add(transLike);
            }
        }

        #endregion

        #region Payees

        private readonly IDictionary<string, IPayeeSql> _payeeCache = new Dictionary<string, IPayeeSql>();
        private readonly IDictionary<IPayeeSql, IList<IHavePayeeSql>> _payeeAssignment =
            new Dictionary<IPayeeSql, IList<IHavePayeeSql>>();

        private void AssignPayee(
            string name, IHavePayeeSql transBase)
        {
            _payeeAssignment.AddToKey(GetOrCreatePayee(name), transBase);
        }

        private IPayeeSql GetOrCreatePayee(string name)
        {
            if (name.IsNullOrEmpty()) throw new ArgumentException("Payee name cannot be empty!", nameof(name));

            if (_payeeCache.TryGetValue(name, out var payee)) return payee;

            payee = _payeeFactory();
            payee.Name = name;

            _payeeCache[name] = payee;

            return payee;
        }

        #endregion

        #region Flags

        private readonly IDictionary<string, IFlagSql> _flagCache = new Dictionary<string, IFlagSql>();
        private readonly IDictionary<IFlagSql, IList<IHaveFlagSql>> _flagAssignment =
            new Dictionary<IFlagSql, IList<IHaveFlagSql>>();

        private void AssignFlag(
            string name, IHaveFlagSql transBase)
        {
            _flagAssignment.AddToKey(GetOrCreateFlag(name), transBase);
        }

        private IFlagSql GetOrCreateFlag(string name)
        {
            if (name.IsNullOrEmpty()) throw new ArgumentException("Flag name cannot be empty!", nameof(name));

            if (_flagCache.TryGetValue(name, out var flag)) return flag;

            flag = _flagFactory();
            flag.Name = name;
            flag.Color = Ynab4CsvImportBase.FlagNameToColorNumber(name);

            _flagCache[name] = flag;

            return flag;
        }

        #endregion

        private class ImportAssignmentsInner : ISqliteYnab4CsvImportAssignments
        {
            public ImportAssignmentsInner(IReadOnlyDictionary<IAccountSql, IList<IHaveAccountSql>> accountToTransactionBase, IReadOnlyDictionary<IAccountSql, IList<ITransSql>> fromAccountToTransfer, IReadOnlyDictionary<IAccountSql, IList<ITransSql>> toAccountToTransfer, IReadOnlyDictionary<IPayeeSql, IList<IHavePayeeSql>> payeeToTransactionBase, IReadOnlyDictionary<ITransSql, IList<ISubTransactionSql>> parentTransactionToSubTransaction, IReadOnlyDictionary<IFlagSql, IList<IHaveFlagSql>> flagToTransBase)
            {
                AccountToTransactionBase = accountToTransactionBase;
                FromAccountToTransfer = fromAccountToTransfer;
                ToAccountToTransfer = toAccountToTransfer;
                PayeeToTransactionBase = payeeToTransactionBase;
                ParentTransactionToSubTransaction = parentTransactionToSubTransaction;
                FlagToTransBase = flagToTransBase;
            }

            public IReadOnlyDictionary<IAccountSql, IList<IHaveAccountSql>> AccountToTransactionBase { get; }
            public IReadOnlyDictionary<IAccountSql, IList<ITransSql>> FromAccountToTransfer { get; }
            public IReadOnlyDictionary<IAccountSql, IList<ITransSql>> ToAccountToTransfer { get; }

            public IReadOnlyDictionary<IPayeeSql, IList<IHavePayeeSql>> PayeeToTransactionBase { get; }

            public IReadOnlyDictionary<ITransSql, IList<ISubTransactionSql>> ParentTransactionToSubTransaction { get; }

            public IReadOnlyDictionary<IFlagSql, IList<IHaveFlagSql>> FlagToTransBase { get; }
        }
    }
}