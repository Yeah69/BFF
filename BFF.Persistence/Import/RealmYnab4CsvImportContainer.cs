using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Persistence.Common;
using BFF.Persistence.Import.Models.YNAB;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using MoreLinq;

namespace BFF.Persistence.Import
{
    public interface IRealmYnab4CsvImportContainerData
    {
        IReadOnlyList<IAccountRealm> Accounts { get; }
        IReadOnlyList<IPayeeRealm> Payees { get; }
        IReadOnlyList<ICategoryRealm> MasterCategories { get; }
        IReadOnlyList<ICategoryRealm> SubCategories { get; }
        IReadOnlyList<ICategoryRealm> IncomeCategories { get; }
        IReadOnlyList<IFlagRealm> Flags { get; }
        IReadOnlyList<ITransRealm> Trans { get; }
        IReadOnlyList<ISubTransactionRealm> SubTransactions { get; }
        IReadOnlyList<IBudgetEntryRealm> BudgetEntries { get; }
    }

    internal class RealmYnab4CsvImportContainer : IYnab4CsvImportContainer, IRealmYnab4CsvImportContainerData
    {
        private readonly IImportingOrm _importingOrm;
        private readonly ICreateBackendOrm _createBackendOrm;
        private readonly Func<bool, Trans> _transFactory;
        private readonly Func<bool, BudgetEntry> _budgetEntryFactory;
        private readonly Func<bool, SubTransaction> _subTransactionFactory;
        private readonly Func<bool, Account> _accountFactory;
        private readonly Func<bool, Category> _categoryFactory;
        private readonly Func<bool, Payee> _payeeFactory;
        private readonly Func<bool, Flag> _flagFactory;
        private readonly IList<IAccountRealm> _accounts = new List<IAccountRealm>();
        private readonly IList<IPayeeRealm> _payees = new List<IPayeeRealm>();
        private readonly IList<ICategoryRealm> _masterCategories = new List<ICategoryRealm>();
        private readonly IList<ICategoryRealm> _subCategories = new List<ICategoryRealm>();
        private readonly IList<IFlagRealm> _flags = new List<IFlagRealm>();
        private readonly IList<ITransRealm> _trans = new List<ITransRealm>();
        private readonly IList<ISubTransactionRealm> _subTransactions = new List<ISubTransactionRealm>();
        private readonly IList<IBudgetEntryRealm> _budgetEntries = new List<IBudgetEntryRealm>();

        private readonly IDictionary<string, IAccountRealm> _accountDict = new Dictionary<string, IAccountRealm>();
        private readonly IDictionary<string, IPayeeRealm> _payeeDict = new Dictionary<string, IPayeeRealm>();
        private readonly IDictionary<string, IFlagRealm> _flagDict = new Dictionary<string, IFlagRealm>();

        public RealmYnab4CsvImportContainer(
            IImportingOrm importingOrm,
            ICreateBackendOrm createBackendOrm,
            Func<bool, Trans> transFactory,
            Func<bool, SubTransaction> subTransactionFactory,
            Func<bool, BudgetEntry> budgetEntryFactory,
            Func<bool, Account> accountFactory,
            Func<bool, Category> categoryFactory,
            Func<bool, Payee> payeeFactory,
            Func<bool, Flag> flagFactory)
        {
            _budgetEntryFactory = budgetEntryFactory;
            _importingOrm = importingOrm;
            _createBackendOrm = createBackendOrm;
            _transFactory = transFactory;
            _subTransactionFactory = subTransactionFactory;
            _accountFactory = accountFactory;
            _categoryFactory = categoryFactory;
            _payeeFactory = payeeFactory;
            _flagFactory = flagFactory;
            Accounts = _accounts.ToReadOnlyList();
            Payees = _payees.ToReadOnlyList();
            MasterCategories = _masterCategories.ToReadOnlyList();
            SubCategories = _subCategories.ToReadOnlyList();
            IncomeCategories = new List<ICategoryRealm>{ _thisMonthCategoryImportWrapper, _nextMonthCategoryImportWrapper };
            Flags = _flags.ToReadOnlyList();

            Trans = _trans.ToReadOnlyList();
            SubTransactions = _subTransactions.ToReadOnlyList();

            BudgetEntries = _budgetEntries.ToReadOnlyList();

            var thisMonthIncomeCategory = _categoryFactory(false);
            thisMonthIncomeCategory.Name = Ynab4CsvImportBase.IncomeCategoryThisMonthSubName;
            thisMonthIncomeCategory.IsIncomeRelevant = true;
            thisMonthIncomeCategory.MonthOffset = 0;
            thisMonthIncomeCategory.Parent = null;
            _thisMonthCategoryImportWrapper = thisMonthIncomeCategory;

            var nextMonthIncomeCategory = _categoryFactory(false);
            nextMonthIncomeCategory.Name = Ynab4CsvImportBase.IncomeCategoryNextMonthSubName;
            nextMonthIncomeCategory.IsIncomeRelevant = true;
            nextMonthIncomeCategory.MonthOffset = 1;
            nextMonthIncomeCategory.Parent = null;
            _nextMonthCategoryImportWrapper = nextMonthIncomeCategory;
        }

        public IReadOnlyList<IAccountRealm> Accounts { get; }
        public IReadOnlyList<IPayeeRealm> Payees { get; }
        public IReadOnlyList<ICategoryRealm> MasterCategories { get; }
        public IReadOnlyList<ICategoryRealm> SubCategories { get; }
        public IReadOnlyList<ICategoryRealm> IncomeCategories { get; }
        public IReadOnlyList<IFlagRealm> Flags { get; }

        public IReadOnlyList<ITransRealm> Trans { get; }

        public IReadOnlyList<ISubTransactionRealm> SubTransactions { get; }

        public IReadOnlyList<IBudgetEntryRealm> BudgetEntries { get; }

        public void SetAccountStartingBalance(string name, long balance)
        {
            var account = GetOrCreateAccount(name);
            account.StartingBalance = balance;
        }

        public void TrySetAccountStartingDate(string name, DateTime date)
        {
            var account = GetOrCreateAccount(name);
            account.StartingDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }

        public void AddAsTransfer(Ynab4Transaction ynab4Transaction)
        {
            long tempSum = ynab4Transaction.Inflow - ynab4Transaction.Outflow;

            ITransRealm transfer = _transFactory(false);
            transfer.CheckNumber = ynab4Transaction.CheckNumber;
            transfer.Date = DateTime.SpecifyKind(ynab4Transaction.Date, DateTimeKind.Utc);
            transfer.Memo = ynab4Transaction.Memo;
            transfer.Sum = Math.Abs(tempSum);
            transfer.Cleared = ynab4Transaction.Cleared;

            string fromAccount = ynab4Transaction.GetFromAccountName();
            string toAccount = ynab4Transaction.GetToAccountName();
            transfer.FromAccount = GetOrCreateAccount(fromAccount);
            transfer.ToAccount = GetOrCreateAccount(toAccount);
            TrySetAccountStartingDate(fromAccount, ynab4Transaction.Date);
            TrySetAccountStartingDate(toAccount, ynab4Transaction.Date);

            transfer.Flag = GetOrCreateFlag(ynab4Transaction.Flag);
            _trans.Add(transfer);
        }

        public void AddAsTransaction(Ynab4Transaction ynab4Transaction)
        {

            ITransRealm transaction = _transFactory(false);
            transaction.CheckNumber = ynab4Transaction.CheckNumber;
            transaction.Date = DateTime.SpecifyKind(ynab4Transaction.Date, DateTimeKind.Utc);
            transaction.Memo = ynab4Transaction.Memo;
            transaction.Sum = ynab4Transaction.Inflow - ynab4Transaction.Outflow;
            transaction.Cleared = ynab4Transaction.Cleared;

            transaction.Account = GetOrCreateAccount(ynab4Transaction.Account);
            TrySetAccountStartingDate(ynab4Transaction.Account, ynab4Transaction.Date);
            transaction.Payee = GetOrCreatePayee(ynab4Transaction.ParsePayeeFromPayee());
            transaction.Category = GetOrCreateCategory(ynab4Transaction.Category);
            transaction.Flag = GetOrCreateFlag(ynab4Transaction.Flag);
            _trans.Add(transaction);
        }

        public void AddParentAndSubTransactions(Ynab4Transaction parent, string parentMemo, IEnumerable<Ynab4Transaction> subTransactions)
        {
            var parentTransaction = AddAsParentTransaction(parent);
            subTransactions.ForEach(st => AddAsSubTransaction(st, parentTransaction));
        }

        private ITransRealm AddAsParentTransaction(Ynab4Transaction ynab4Transaction)
        {
            
            ITransRealm parentTransaction = _transFactory(false);
            parentTransaction.CheckNumber = ynab4Transaction.CheckNumber;
            parentTransaction.Date = DateTime.SpecifyKind(ynab4Transaction.Date, DateTimeKind.Utc);
            parentTransaction.Memo = ynab4Transaction.Memo;
            parentTransaction.Sum = ynab4Transaction.Inflow - ynab4Transaction.Outflow;
            parentTransaction.Cleared = ynab4Transaction.Cleared;

            parentTransaction.Account = GetOrCreateAccount(ynab4Transaction.Account);
            TrySetAccountStartingDate(ynab4Transaction.Account, ynab4Transaction.Date);
            parentTransaction.Payee = GetOrCreatePayee(ynab4Transaction.ParsePayeeFromPayee());
            parentTransaction.Flag = GetOrCreateFlag(ynab4Transaction.Flag);
            _trans.Add(parentTransaction);
            return parentTransaction;
        }

        private void AddAsSubTransaction(Ynab4Transaction ynab4Transaction, ITransRealm parent)
        {
            ISubTransactionRealm subTransaction = _subTransactionFactory(false);
            subTransaction.Parent = parent;
            subTransaction.Memo = ynab4Transaction.Memo;
            subTransaction.Sum = ynab4Transaction.Inflow - ynab4Transaction.Outflow;

            subTransaction.Category = GetOrCreateCategory(ynab4Transaction.Category);
            _subTransactions.Add(subTransaction);
        }

        public void AddBudgetEntry(Ynab4BudgetEntry budgetEntry)
        {
            if (budgetEntry.Budgeted != 0L)
            {
                var month = DateTime.ParseExact(budgetEntry.Month, "MMMM yyyy", CultureInfo.GetCultureInfo("de-DE")); // TODO make this customizable + exception handling
                IBudgetEntryRealm budgetEntryRealm = _budgetEntryFactory(false);
                budgetEntryRealm.Month = DateTime.SpecifyKind(month, DateTimeKind.Utc);
                budgetEntryRealm.Budget = budgetEntry.Budgeted;

                budgetEntryRealm.Category = GetOrCreateCategory(budgetEntry.Category);
                _budgetEntries.Add(budgetEntryRealm);
            }
        }

        public async Task SaveIntoDatabase()
        {
            await _createBackendOrm.CreateAsync().ConfigureAwait(false);
            await _importingOrm.PopulateDatabaseAsync(this).ConfigureAwait(false);
        }

        private IAccountRealm GetOrCreateAccount(string name)
        {
            if (!_accountDict.TryGetValue(name, out var account))
            {
                account = _accountFactory(false);
                account.Name = name;
                _accountDict[name] = account;
            }

            return account;
        }

        private IFlagRealm GetOrCreateFlag(string name)
        {
            if (!_flagDict.TryGetValue(name, out var flag))
            {
                flag = _flagFactory(false);
                flag.Name = name;
                flag.Color = Ynab4CsvImportBase.FlagNameToColorNumber(name);

                _flagDict[name] = flag;
            }

            return flag;
        }

        private IPayeeRealm GetOrCreatePayee(string name)
        {
            if (!_payeeDict.TryGetValue(name, out var payee))
            {
                payee = _payeeFactory(false);
                payee.Name = name;
                _payeeDict[name] = payee;
            }

            return payee;
        }

        private readonly ICategoryRealm _thisMonthCategoryImportWrapper;

        private readonly ICategoryRealm _nextMonthCategoryImportWrapper;

        private ICategoryRealm GetOrCreateCategory(string namePath)
        {
            (string master, string sub) = Ynab4CsvImportBase.SplitCategoryNamePath(namePath);
            if (Ynab4CsvImportBase.IsIncomeThisMonth(master, sub))
            {
                return _thisMonthCategoryImportWrapper;
            }
            if (Ynab4CsvImportBase.IsIncomeNextMonth(master, sub))
            {
                return _nextMonthCategoryImportWrapper;
            }

            ICategoryRealm masterCategory =
                _masterCategories.SingleOrDefault(c => c.Name == master);
            if (masterCategory is null)
            {
                masterCategory = _categoryFactory(false);
                masterCategory.Name = master;
                _masterCategories.Add(masterCategory);
            }
            ICategoryRealm subCategory =
                _subCategories.SingleOrDefault(c => c.Parent == masterCategory && c.Name == sub);
            if (subCategory is null)
            {
                subCategory = _categoryFactory(false);
                subCategory.Name = sub;
                _subCategories.Add(subCategory);
            }
            return subCategory;
        }
    }
}