using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using MoreLinq.Extensions;
using MrMeeseeks.Extensions;
using NLog;
using Account = BFF.Persistence.Realm.Models.Persistence.Account;
using BudgetEntry = BFF.Persistence.Realm.Models.Persistence.BudgetEntry;
using Category = BFF.Persistence.Realm.Models.Persistence.Category;

namespace BFF.Persistence.Realm.ORM
{
    internal class RealmBudgetOrm : IBudgetOrm
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IRealmOperations _realmOperations;

        public RealmBudgetOrm(
            IRealmOperations realmOperations)
        {
            _realmOperations = realmOperations;
        }

        public Task<BudgetBlock> FindAsync(int year)
        {
            var currentYear = new DateTimeOffset(year, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var monthsOfYear = Enumerable
                .Range(0, 11)
                .Scan(
                    new DateTime(year, 1, 1), 
                    (dt, _) => dt.NextMonth())
                .ToArray();

            var nextYear = new DateTimeOffset(year + 1, 1, 1, 0, 0, 0, TimeSpan.Zero);
            return _realmOperations.RunFuncAsync(realm =>
            {
                var queryStart = DateTime.Now;
                var categories = realm.All<Category>()
                    .Where(c => !c.IsIncomeRelevant)
                    .ToArray(); 
                var initialCacheEntriesForCategories = GetInitialCategoryValues(realm, currentYear)
                    .ToArray();

                var categoryToInitialBalance = initialCacheEntriesForCategories.ToDictionary(t => t.Category, t => Math.Max(0L, t.Balance));

                var categoryToBudgetEntryLookup = realm.All<BudgetEntry>()
                    .Where(be => be.Month >= currentYear && be.Month < nextYear)
                    .ToArray()
                    .ToLookup(be => be.Category);

                var categoryToTransactionLookup = realm.All<Trans>()
                    .Where(t => t.TypeIndex == (int) TransType.Transaction 
                                && t.Date >= currentYear
                                && t.Date < nextYear
                                && t.Category != null)
                    .ToArray()
                    .ToLookup(t => t.Category);

                var categoryToSubTransactionLookup = realm.All<Trans>()
                    .Where(t => t.TypeIndex == (int)TransType.ParentTransaction
                                && t.Date >= currentYear
                                && t.Date < nextYear)
                    .ToArray()
                    .SelectMany(pt => pt.SubTransactions.ToArray().Select(st => (pt, st)))
                    .ToLookup(t => t.st.Category);

                var budgetEntriesPerMonth = categories
                    .SelectMany(c =>
                    {
                        var initialBalance = categoryToInitialBalance.TryGetValue(c, out var ib) ? ib : 0L;
                        var budgetEntries = categoryToBudgetEntryLookup.Contains(c) 
                            ? categoryToBudgetEntryLookup[c]
                                .ToDictionary(be => new DateTime(be.Month.Year, be.Month.Month, 1))
                            : new Dictionary<DateTime, BudgetEntry>();
                        var transactionOutflows = categoryToTransactionLookup.Contains(c)
                            ? categoryToTransactionLookup[c]
                                .GroupBy(t => new DateTime(t.Date.Year, t.Date.Month, 1), t => t.Sum)
                                .ToDictionary(g => g.Key, g => g.Sum())
                            : new Dictionary<DateTime, long>();
                        var subTransactionOutflows = categoryToSubTransactionLookup.Contains(c)
                            ? categoryToSubTransactionLookup[c]
                                .GroupBy(t => new DateTime(t.pt.Date.Year, t.pt.Date.Month, 1), t => t.st.Sum)
                                .ToDictionary(g => g.Key, g => g.Sum())
                            : new Dictionary<DateTime, long>();

                        var currentBalance = initialBalance;
                        var list = new List<(DateTime Month, (BudgetEntry Entry, Category Category, long Budget, long Outflow, long Balance) Tuple)>();
                        foreach (var month in monthsOfYear)
                        {
                            var budgetEntry = budgetEntries.TryGetValue(month, out var be) ? be : null;
                            var transactionOutflow = transactionOutflows.TryGetValue(month, out var tOut) ? tOut : 0L;
                            var subTransactionOutflow = subTransactionOutflows.TryGetValue(month, out var stOut) ? stOut : 0L;

                            var budget = budgetEntry?.Budget ?? 0L;
                            var outflow = transactionOutflow + subTransactionOutflow;
                            var balance = currentBalance + budget + outflow;

                            list.Add((month, (budgetEntry, c, budget, outflow, balance)));
                            currentBalance = Math.Max(0L, balance);
                        }

                        return list.ToReadOnlyList();
                    })
                    .GroupBy(t => t.Month, t => t.Tuple)
                    .ToDictionary(g => g.Key, g => g.ToReadOnlyList());


                var danglingTransfersPerMonth = realm.All<Trans>()
                    .Where(t => t.TypeIndex == (int)TransType.Transfer
                                && (t.FromAccount == null && t.ToAccount != null || t.ToAccount == null && t.FromAccount != null)
                                && t.Date >= currentYear && t.Date < nextYear)
                    .ToArray()
                    .Select(t => (Month: new DateTime(t.Date.Year, t.Date.Month, 1),
                        Sum: t.FromAccount is null ? -1L * t.Sum : t.Sum))
                    .GroupBy(t => t.Month, t => t.Sum)
                    .ToDictionary(g => g.Key, g => g.Sum());

                var unassignedTransactionsPerMonth = realm.All<Trans>()
                    .Where(t => t.TypeIndex == (int)TransType.Transaction
                                && t.Category == null
                                && t.Date >= currentYear && t.Date < nextYear)
                    .ToArray()
                    .Select(t => (Month: new DateTime(t.Date.Year, t.Date.Month, 1), t.Sum))
                    .Concat(realm.All<Trans>()
                        .Where(t => t.TypeIndex == (int)TransType.ParentTransaction
                                    && t.Date >= currentYear && t.Date < nextYear)
                        .ToArray()
                        .SelectMany(t => t.SubTransactions.Where(st => st.Category == null), (t, st) => (Month: new DateTime(t.Date.Year, t.Date.Month, 1), st.Sum)))
                    .GroupBy(t => t.Month, t => t.Sum)
                    .ToDictionary(g => g.Key, g => g.Sum());

                var incomesPerMonth = realm.All<Category>()
                    .Where(c => c.IsIncomeRelevant)
                    .ToArray()
                    .SelectMany(c =>
                    {
                        var offsetCurrentYear = new DateTimeOffset(new DateTime(currentYear.Year, currentYear.Month, 1).OffsetMonthBy(-1 * c.IncomeMonthOffset), TimeSpan.Zero);
                        var offsetNextYear = new DateTimeOffset(new DateTime(nextYear.Year, nextYear.Month, 1).OffsetMonthBy(-1 * c.IncomeMonthOffset), TimeSpan.Zero);
                        var transactions = realm.All<Trans>()
                            .Where(t => t.TypeIndex == (int) TransType.Transaction 
                                        && t.Category == c 
                                        && t.Date >= offsetCurrentYear 
                                        && t.Date < offsetNextYear)
                            .ToArray()
                            .Select(t =>
                            {
                                var offsetDate = new DateTime(t.Date.Year, t.Date.Month, 1).OffsetMonthBy(c.IncomeMonthOffset);
                                return (offsetDate, t.Sum);
                            });
                        var subTransactions = realm.All<Trans>()
                            .Where(t => t.TypeIndex == (int)TransType.ParentTransaction
                                        && t.Date >= offsetCurrentYear
                                        && t.Date < offsetNextYear)
                            .ToArray()
                            .SelectMany(t =>
                            {
                                var offsetDate = new DateTime(t.Date.Year, t.Date.Month, 1).OffsetMonthBy(c.IncomeMonthOffset);
                                return t.SubTransactions.Where(st => st.Category == c).ToArray().Select(st => (offsetDate, st.Sum));
                            });
                        return transactions.Concat(subTransactions);
                    })
                    .Concat(realm.All<Account>()
                        .Where(a => a.StartingDate >= currentYear && a.StartingDate < nextYear)
                        .ToArray()
                        .Select(a => (offsetDate: new DateTime(a.StartingDate.Year, a.StartingDate.Month, 1), Sum: a.StartingBalance)))
                    .GroupBy(t => t.offsetDate, t => t.Sum)
                    .ToDictionary(g => g.Key, g => g.Sum());

                var initialOverspentInPreviousMonth = initialCacheEntriesForCategories
                    .Where(t => t.LastMonth.Year == currentYear.Year - 1 && t.LastMonth.Month == 12 && t.Balance < 0)
                    .Sum(t => t.Balance);

                var totalGlobalPotMoney = CalculateGlobalPot(realm, currentYear);

                var expensesFromCategoryPots = initialCacheEntriesForCategories.Aggregate(0L,
                    (previous, current) => previous - current.TotalBudget + current.TotalNegativeBalance);

                var initialNotBudgetedOrOverbudgeted =
                    totalGlobalPotMoney
                    + expensesFromCategoryPots
                    // Subtract "initialOverspentInPreviousMonth" because it will be used in the budget month and is included in "expensesFromCategoryPots"
                    // hence, it would be accounted two times otherwise
                    - initialOverspentInPreviousMonth;

                var queryDuration = DateTime.Now - queryStart;
                Logger.Debug($"Budget Query Duration: {queryDuration.ToString()}");

                return new BudgetBlock
                {
                    BudgetEntriesPerMonth = budgetEntriesPerMonth,
                    IncomesPerMonth = incomesPerMonth,
                    DanglingTransfersPerMonth = danglingTransfersPerMonth,
                    UnassignedTransactionsPerMonth = unassignedTransactionsPerMonth,
                    InitialNotBudgetedOrOverbudgeted = initialNotBudgetedOrOverbudgeted,
                    InitialOverspentInPreviousMonth = initialOverspentInPreviousMonth
                };
            });

            IEnumerable<(Category Category, DateTimeOffset LastMonth, long Balance, long TotalBudget, long TotalNegativeBalance)> GetInitialCategoryValues(
            Realms.Realm realm,
            DateTimeOffset untilMonth)
            {
                var budgetEntries = realm
                        .All<BudgetEntry>()
                        .Where(be => be.Month < untilMonth)
                        .ToList()
                        .Select(be => (be.Category, Month: new DateTimeOffset(be.Month.Year, be.Month.Month, 1, 0, 0, 0, TimeSpan.Zero), be.Budget))
                        .ToList();
                
                var transactions = realm
                    .All<Trans>()
                    .Where(t => t.TypeIndex == (int)TransType.Transaction && t.Date < untilMonth)
                    .ToList()
                    .Select(t => (t.Category, Month: new DateTimeOffset(t.Date.Year, t.Date.Month, 1, 0, 0, 0, TimeSpan.Zero), t.Sum))
                    .Concat(realm
                        .All<SubTransaction>()
                        .ToList()
                        .Where(st => st.Parent.Date < untilMonth)
                        .Select(st => (st.Category,
                            Month: new DateTimeOffset(st.Parent.Date.Year, st.Parent.Date.Month, 1, 0, 0, 0, TimeSpan.Zero),
                            st.Sum)))
                    .ToList();

                return budgetEntries.FullGroupJoin(
                    transactions,
                    be => be.Category,
                    t => t.Category,
                    (category, budgets, outflows) =>
                    {
                        var (m, balance, totalBudget, totalNegativeBalance) = budgets
                            .FullGroupJoin(
                                outflows,
                                b => b.Month,
                                o => o.Month,
                                (month, bs, os) => (Month: month, Budget: bs.Select(b => b.Budget).FirstOrDefault(), Outflow: (os.Any() ? os.Select(o => o.Sum).Sum() : 0L)))
                            .OrderBy(t => t.Month)
                            .Aggregate((Month: DateTimeOffset.MinValue, Balance: 0L, TotalBudget: 0L, TotalNegativeBalance: 0L), (previous, current) =>
                            {
                                var currentBalance = Math.Max(0L, previous.Balance) + current.Budget + current.Outflow;
                                return (
                                    current.Month,
                                    Balance: currentBalance,
                                    TotalBudget: previous.TotalBudget + current.Budget,
                                    TotalNegativeBalance:
                                    previous.TotalNegativeBalance +
                                    Math.Min(0L, currentBalance));
                            });
                        return (category, m, balance, totalBudget, totalNegativeBalance);
                    });
            }

            long CalculateGlobalPot(Realms.Realm realm, DateTimeOffset month)
            {
                return realm.All<Account>()
                    .Where(a => a.StartingDate < month)
                    .ToList()
                    // Account starting balances
                    .Select(a => a.StartingBalance)
                    // Unassigned Transactions
                    .Concat(realm.All<Trans>()
                        .Where(t => t.TypeIndex == (int)TransType.Transaction
                                    && t.Date < month
                                    && t.Category == null)
                        .ToList()
                        .Select(t => t.Sum))
                    // Unassigned sub-transactions
                    .Concat(realm.All<SubTransaction>()
                        .Where(st => st.Category == null)
                        .ToList()
                        .Where(st => st.Parent.Date < month)
                        .Select(st => st.Sum))
                    // Income Transactions and SubTransactions
                    .Concat(realm.All<Category>()
                        .Where(c => c.IsIncomeRelevant)
                        .ToList()
                        .GroupBy(c => c.IncomeMonthOffset)
                        .SelectMany(g =>
                        {
                            var offsetMontOffset = month.OffsetMonthBy(-1 * g.Key);
                            return g.SelectMany(c => c
                                    .Transactions
                                    .Where(t => t.Date < offsetMontOffset)
                                    .ToList()
                                    .Select(t => t.Sum))
                                .Concat(g.SelectMany(c => c
                                    .SubTransactions
                                    .ToList()
                                    .Where(st => st.Parent.Date < offsetMontOffset)
                                    .Select(st => st.Sum)));
                        }))
                    // Dangling transfers
                    .Concat(realm
                        .All<Trans>()
                        .Where(t => t.TypeIndex == (int)TransType.Transfer
                                    && t.Date < month
                                    && (t.ToAccount == null && t.FromAccount != null ||
                                     t.FromAccount == null && t.ToAccount != null))
                        .ToList()
                        .Select(t => t.ToAccount is null ? -1L * t.Sum : t.Sum))
                    .Sum();
            }
        }

        public Task<IReadOnlyList<(BudgetEntry Entry, DateTime Month, long Budget, long Outflow, long Balance)>> FindAsync(int year, Category category)
        {
            var currentYear = new DateTimeOffset(year, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var monthsOfYear = Enumerable
                .Range(0, 11)
                .Scan(
                    new DateTimeOffset(year, 1, 1, 0, 0, 0, TimeSpan.Zero), 
                    (dt, _) => dt.NextMonth())
                .ToArray();

            var nextYear = new DateTimeOffset(year + 1, 1, 1, 0, 0, 0, TimeSpan.Zero);
            return _realmOperations.RunFuncAsync(realm =>
            {
                var queryStart = DateTime.Now;
                var initialCacheEntries = GetInitialValues(realm, currentYear);

                var initialBalance = Math.Max(0L, initialCacheEntries.Balance);

                var budgetEntries = realm.All<BudgetEntry>()
                    .Where(be => be.Month >= currentYear 
                                 && be.Month < nextYear
                                 && be.Category == category)
                    .ToDictionary(be => be.Month, be => be);

                var transactions = realm.All<Trans>()
                    .Where(t => t.TypeIndex == (int) TransType.Transaction 
                                && t.Date >= currentYear
                                && t.Date < nextYear
                                && t.Category == category)
                    .ToArray();

                var subTransactions = realm.All<Trans>()
                    .Where(t => t.TypeIndex == (int)TransType.ParentTransaction
                                && t.Date >= currentYear
                                && t.Date < nextYear)
                    .ToArray()
                    .SelectMany(pt => 
                        pt.SubTransactions
                            .Where(st => st.Category == category)
                            .ToArray()
                            .Select(st => (pt, st)))
                    .ToArray();

                var transactionOutflows = transactions
                    .GroupBy(t => new DateTimeOffset(t.Date.Year, t.Date.Month, 1, 0, 0, 0, TimeSpan.Zero), t => t.Sum)
                    .ToDictionary(g => g.Key, g => g.Sum());
                var subTransactionOutflows = subTransactions
                    .GroupBy(t => new DateTimeOffset(t.pt.Date.Year, t.pt.Date.Month, 1, 0, 0, 0, TimeSpan.Zero), t => t.st.Sum)
                    .ToDictionary(g => g.Key, g => g.Sum());

                var currentBalance = initialBalance;
                var list = new List<(BudgetEntry Entry, DateTime Month, long Budget, long Outflow, long Balance)>();
                foreach (var month in monthsOfYear)
                {
                    var budgetEntry = budgetEntries.TryGetValue(month, out var be) ? be : null;
                    var transactionOutflow = transactionOutflows.TryGetValue(month, out var tOut) ? tOut : 0L;
                    var subTransactionOutflow = subTransactionOutflows.TryGetValue(month, out var stOut) ? stOut : 0L;

                    var budget = budgetEntry?.Budget ?? 0L;
                    var outflow = transactionOutflow + subTransactionOutflow;
                    var balance = currentBalance + budget + outflow;

                    list.Add((budgetEntry, new DateTime(month.Year, month.Month, month.Day), budget, outflow, balance));
                    currentBalance = Math.Max(0L, balance);
                }

                var queryDuration = DateTime.Now - queryStart;
                Logger.Debug($"Budget Query Duration: {queryDuration.ToString()}");
                
                return list.ToReadOnlyList();
            });

            (DateTimeOffset LastMonth, long Balance, long TotalBudget, long TotalNegativeBalance) GetInitialValues(
            Realms.Realm realm,
            DateTimeOffset untilMonth)
            {
                var budgetEntries = realm
                        .All<BudgetEntry>()
                        .Where(be => be.Month < untilMonth
                            && be.Category == category)
                        .ToList()
                        .Select(be => (Month: new DateTimeOffset(be.Month.Year, be.Month.Month, 1, 0, 0, 0, TimeSpan.Zero), be.Budget))
                        .ToList();
                
                var transactions = realm
                    .All<Trans>()
                    .Where(t => t.TypeIndex == (int)TransType.Transaction 
                                && t.Date < untilMonth
                                && t.Category == category)
                    .ToList()
                    .Select(t => (Month: new DateTimeOffset(t.Date.Year, t.Date.Month, 1, 0, 0, 0, TimeSpan.Zero), t.Sum))
                    .Concat(realm
                        .All<SubTransaction>()
                        .Where(st => st.Category == category)
                        .ToList()
                        .Where(st => st.Parent.Date < untilMonth)
                        .Select(st => (
                            Month: new DateTimeOffset(st.Parent.Date.Year, st.Parent.Date.Month, 1, 0, 0, 0, TimeSpan.Zero),
                            st.Sum)))
                    .ToList();
                
                var (m, balance, totalBudget, totalNegativeBalance) = budgetEntries
                    .FullGroupJoin(
                        transactions,
                        b => b.Month,
                        o => o.Month,
                        (month, bs, os) => (Month: month, Budget: bs.Select(b => b.Budget).FirstOrDefault(), Outflow: os.Any() ? os.Select(o => o.Sum).Sum() : 0L))
                    .OrderBy(t => t.Month)
                    .Aggregate((Month: DateTimeOffset.MinValue, Balance: 0L, TotalBudget: 0L, TotalNegativeBalance: 0L), (previous, current) =>
                    {
                        var currentBalance = Math.Max(0L, previous.Balance) + current.Budget + current.Outflow;
                        return (
                            current.Month,
                            Balance: currentBalance,
                            TotalBudget: previous.TotalBudget + current.Budget,
                            TotalNegativeBalance:
                            previous.TotalNegativeBalance +
                            Math.Min(0L, currentBalance));
                    });
                return (m, balance, totalBudget, totalNegativeBalance);
            }
        }
    }

    public class BudgetBlock
    {
        internal IDictionary<DateTime, IReadOnlyList<(BudgetEntry Entry, Category Category, long Budget, long Outflow, long Balance)>> BudgetEntriesPerMonth
        {
            get;
            set;
        }

        public long InitialNotBudgetedOrOverbudgeted { get; set; }

        public long InitialOverspentInPreviousMonth { get; set; }

        public IDictionary<DateTime, long> IncomesPerMonth { get; set; }

        public IDictionary<DateTime, long> DanglingTransfersPerMonth { get; set; }

        public IDictionary<DateTime, long> UnassignedTransactionsPerMonth { get; set; }
    }
}
