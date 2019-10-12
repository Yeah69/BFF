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
                        var offsetCurrentYear = new DateTimeOffset(new DateTime(currentYear.Year, currentYear.Month, 1).OffsetMonthBy(-1 * c.Month), TimeSpan.Zero);
                        var offsetNextYear = new DateTimeOffset(new DateTime(nextYear.Year, nextYear.Month, 1).OffsetMonthBy(-1 * c.Month), TimeSpan.Zero);
                        var transactions = realm.All<Trans>()
                            .Where(t => t.TypeIndex == (int) TransType.Transaction 
                                        && t.Category == c 
                                        && t.Date >= offsetCurrentYear 
                                        && t.Date < offsetNextYear)
                            .ToArray()
                            .Select(t =>
                            {
                                var offsetDate = new DateTime(t.Date.Year, t.Date.Month, 1).OffsetMonthBy(c.Month);
                                return (offsetDate, t.Sum);
                            });
                        var subTransactions = realm.All<Trans>()
                            .Where(t => t.TypeIndex == (int)TransType.ParentTransaction
                                        && t.Date >= offsetCurrentYear
                                        && t.Date < offsetNextYear)
                            .ToArray()
                            .SelectMany(t =>
                            {
                                var offsetDate = new DateTime(t.Date.Year, t.Date.Month, 1).OffsetMonthBy(c.Month);
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
