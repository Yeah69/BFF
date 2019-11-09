using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using MoreLinq.Extensions;
using MrMeeseeks.Extensions;
using Account = BFF.Persistence.Realm.Models.Persistence.Account;
using BudgetEntry = BFF.Persistence.Realm.Models.Persistence.BudgetEntry;
using Category = BFF.Persistence.Realm.Models.Persistence.Category;

namespace BFF.Persistence.Realm.ORM
{
    internal class RealmBudgetOrm : IBudgetOrm
    {
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
                var categories = realm.All<Category>()
                    .Where(c => !c.IsIncomeRelevant)
                    .ToArray();
                var initialCacheEntriesForCategories = categories
                    .Select(c =>
                    {
                        var budgetCacheEntry = c.BudgetCacheEntries
                            .Where(bce => bce.Month < currentYear)
                            .OrderBy(bce => bce.Month)
                            .LastOrDefault();
                        return budgetCacheEntry is null
                            ? (Category: c,
                                Month: DateTimeOffset.MinValue, 
                                Balance: 0L, 
                                TotalBudget: 0L, 
                                TotalNegativeBalance: 0L)
                            : (Category: c,
                                budgetCacheEntry.Month,
                                budgetCacheEntry.Balance,
                                budgetCacheEntry.TotalBudget,
                                budgetCacheEntry.TotalNegativeBalance);
                    })
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
                    .Where(t => t.Month.Year == currentYear.Year - 1 && t.Month.Month == 12 && t.Balance < 0)
                    .Sum(t => t.Balance);

                var totalGlobalPotMoney = realm.All<BudgetCacheEntry>()
                    .Where(bce => bce.Category == null && bce.Month < currentYear)
                    .OrderBy(bce => bce.Month)
                    .LastOrDefault()?.TotalBudget ?? 0L;

                var expensesFromCategoryPots = initialCacheEntriesForCategories.Aggregate(0L,
                    (previous, current) => previous - current.TotalBudget + current.TotalNegativeBalance);

                var initialNotBudgetedOrOverbudgeted =
                    totalGlobalPotMoney
                    + expensesFromCategoryPots
                    // Subtract "initialOverspentInPreviousMonth" because it will be used in the budget month and is included in "expensesFromCategoryPots"
                    // hence, it would be accounted two times otherwise
                    - initialOverspentInPreviousMonth;


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
        }

        public Task UpdateCategorySpecificCacheAsync(Category category, DateTimeOffset month)
        {
            return _realmOperations.RunActionAsync(realm =>
            {
                // Delete all cache entries from given month onwards
                realm.RemoveRange(
                    realm
                        .All<BudgetCacheEntry>()
                        .Where(bce => bce.Month >= month && bce.Category == category));

                // Update cache
                var budgetEntries = realm
                        .All<BudgetEntry>()
                        .Where(be => be.Category == category && be.Month >= month)
                        .ToList()
                        .Select(be => (Month: new DateTimeOffset(be.Month.Year, be.Month.Month, 1, 0, 0, 0, TimeSpan.Zero), be.Budget))
                        .ToList();

                var transactions = realm
                    .All<Trans>()
                    .Where(t => t.TypeIndex == (int)TransType.Transaction && t.Category == category && t.Date >= month)
                    .ToList()
                    .Select(t => (Month: new DateTimeOffset(t.Date.Year, t.Date.Month, 1, 0, 0, 0, TimeSpan.Zero), t.Sum))
                    .Concat(
                        realm
                            .All<SubTransaction>()
                            .Where(st => st.Category == category)
                            .ToList()
                            .Where(st => st.Parent.Date >= month)
                            .Select(st => (Month: new DateTimeOffset(st.Parent.Date.Year, st.Parent.Date.Month, 1, 0, 0, 0, TimeSpan.Zero), st.Sum)))
                    .ToList();

                var lastBudgetCacheEntry = realm
                    .All<BudgetCacheEntry>()
                    .Where(bce => bce.Category == category && bce.Month < month)
                    .OrderByDescending(bce => bce.Month)
                    .FirstOrDefault();

                GenerateBudgetCacheEntriesFor(
                    category,
                    budgetEntries,
                    transactions,
                    lastBudgetCacheEntry?.Balance ?? 0L,
                    lastBudgetCacheEntry?.TotalBudget ?? 0L,
                    lastBudgetCacheEntry?.TotalNegativeBalance ?? 0L)
                    .ForEach(bce => realm.Add(bce));
            });
        }

        public Task DeleteCategorySpecificCacheAsync(Category category)
        {
            return _realmOperations.RunActionAsync(
                realm => realm.RemoveRange(
                    realm
                        .All<BudgetCacheEntry>()
                        .Where(bce => bce.Category == category)));
        }

        public Task UpdateGlobalPotCacheAsync(DateTimeOffset month)
        {

            return _realmOperations.RunActionAsync(realm =>
            {
                // Delete all cache entries from given month onwards
                realm.RemoveRange(
                    realm
                        .All<BudgetCacheEntry>()
                        .Where(bce => bce.Month >= month));

                var initialTotalBudget = realm
                                             .All<BudgetCacheEntry>()
                                             .Where(bce => bce.Category == null && bce.Month < month)
                                             .OrderByDescending(bce => bce.Month)
                                             .FirstOrDefault()?.TotalBudget ?? 0L;

                // Update cache
                var globalBudgets = realm.All<Account>()
                    .Where(a => a.StartingDate >= month)
                    .ToList()
                    // Account starting balances
                    .Select(a => (
                        Month: new DateTimeOffset(a.StartingDate.Year, a.StartingDate.Month, 1, 0, 0, 0,
                            TimeSpan.Zero), a.StartingBalance))
                    // Unassigned Transactions
                    .Concat(realm.All<Trans>()
                        .Where(t => t.TypeIndex == (int) TransType.Transaction
                                    && t.Date >= month
                                    && t.Category == null)
                        .ToList()
                        .Select(t => (Month: new DateTimeOffset(t.Date.Year, t.Date.Month, 1, 0, 0, 0, TimeSpan.Zero),
                            t.Sum)))
                    // Unassigned sub-transactions
                    .Concat(realm.All<SubTransaction>()
                        .Where(st => st.Category == null)
                        .ToList()
                        .Where(st => st.Parent.Date >= month)
                        .Select(st => (
                            Month: new DateTimeOffset(st.Parent.Date.Year, st.Parent.Date.Month, 1, 0, 0, 0,
                                TimeSpan.Zero),
                            st.Sum)))
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
                                    .Where(t => t.Date >= offsetMontOffset)
                                    .ToList()
                                    .Select(t => (
                                        Month: new DateTimeOffset(t.Date.Year, t.Date.Month, 1, 0, 0, 0, TimeSpan.Zero)
                                            .OffsetMonthBy(g.Key),
                                        t.Sum)))
                                .Concat(g.SelectMany(c => c
                                    .SubTransactions
                                    .ToList()
                                    .Where(st => st.Parent.Date >= offsetMontOffset)
                                    .Select(st => (
                                        Month: new DateTimeOffset(st.Parent.Date.Year, st.Parent.Date.Month, 1, 0, 0, 0,
                                            TimeSpan.Zero),
                                        st.Sum))));
                        }))
                    // Dangling transfers
                    .Concat(realm
                        .All<Trans>()
                        .Where(t => t.TypeIndex == (int) TransType.Transfer &&
                                    (t.ToAccount == null && t.FromAccount != null ||
                                     t.FromAccount == null && t.ToAccount != null))
                        .ToList()
                        .Select(t => (Month: new DateTimeOffset(t.Date.Year, t.Date.Month, 1, 0, 0, 0, TimeSpan.Zero),
                            t.ToAccount is null ? -1L * t.Sum : t.Sum)))
                    .GroupBy(t => t.Month, t => t.Item2)
                    .OrderBy(g => g.Key)
                    .Select(g => (Month: g.Key, Sum: g.Sum()))
                    .ToList();



                if (globalBudgets.None()) return;

                var first = globalBudgets.First();
                var firstBudgetCacheEntry = CreateBudgetCacheEntry(
                    initialTotalBudget,
                    first);

                globalBudgets
                    .Skip(1)
                    .Scan(
                        firstBudgetCacheEntry,
                        (previous, current) => CreateBudgetCacheEntry(
                            previous.TotalBudget,
                            current))
                    .ForEach(bce => realm.Add(bce));

                BudgetCacheEntry CreateBudgetCacheEntry(
                    long previousTotalBudget,
                    (DateTimeOffset Month, long Sum) currentSum)
                {
                    return new BudgetCacheEntry
                    {
                        Category = null,
                        Month = currentSum.Month,
                        Balance = 0L,
                        TotalBudget = previousTotalBudget + currentSum.Sum,
                        TotalNegativeBalance = 0L
                    };
                }
            });
        }

        public IEnumerable<BudgetCacheEntry> GenerateBudgetCacheEntriesFor(
            Category category, 
            IEnumerable<(DateTimeOffset Month, long Budget)> budgets, 
            IEnumerable<(DateTimeOffset Month, long Sum)> outflows,
            long initialBalance,
            long initialTotalBudget,
            long initialTotalNegativeBalance)
        {
            var joinOfBudgetsAndOutflows = budgets.GroupBy(t => t.Month, t => t.Budget)
                .FullJoin(
                    outflows.GroupBy(t => t.Month, t => t.Sum),
                    g => g.Key,
                    g => (g.Key, g.First(), 0L),
                    g => (g.Key, 0L, g.Sum()),
                    (b, o) => (b.Key, b.First(), o.Sum()))
                .ToReadOnlyList();

            if (joinOfBudgetsAndOutflows.None()) return Enumerable.Empty<BudgetCacheEntry>();

            var first = joinOfBudgetsAndOutflows.First();
            var firstBudgetCacheEntry = CreateBudgetCacheEntry (
                    initialBalance, 
                    initialTotalBudget, 
                    initialTotalNegativeBalance,
                    first);

            return joinOfBudgetsAndOutflows
                .Skip(1)
                .Scan(
                    firstBudgetCacheEntry,
                    (previous, current) => CreateBudgetCacheEntry(
                        previous.Balance, 
                        previous.TotalBudget,
                        previous.TotalNegativeBalance,
                        current));

            BudgetCacheEntry CreateBudgetCacheEntry(
                long previousBalance,
                long previousTotalBudget,
                long previousTotalNegativeBalance,
                (DateTimeOffset Month, long Budget, long Outflow) currentBudgetAndOutflow)
            {
                var (currentMonth, currentBudget, currentOutflow) = currentBudgetAndOutflow;
                var currentBalance = Math.Max(0L, previousBalance) + currentBudget + currentOutflow;
                return new BudgetCacheEntry
                {
                    Category = category,
                    Month = currentMonth,
                    Balance = currentBalance,
                    TotalBudget = previousTotalBudget + currentBudget,
                    TotalNegativeBalance =
                        previousTotalNegativeBalance +
                        Math.Min(0L, currentBalance)
                };
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
