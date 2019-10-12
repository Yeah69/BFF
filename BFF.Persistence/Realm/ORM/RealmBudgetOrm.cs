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
                        var budgetCacheEntry = c.BudgetCacheEntriesRef
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
                    .Where(be => be.MonthOffset >= currentYear && be.MonthOffset < nextYear)
                    .ToArray()
                    .ToLookup(be => be.CategoryRef);

                var categoryToTransactionLookup = realm.All<Trans>()
                    .Where(t => t.TypeIndex == (int) TransType.Transaction 
                                && t.DateOffset >= currentYear
                                && t.DateOffset < nextYear
                                && t.CategoryRef != null)
                    .ToArray()
                    .ToLookup(t => t.CategoryRef);

                var categoryToSubTransactionLookup = realm.All<Trans>()
                    .Where(t => t.TypeIndex == (int)TransType.ParentTransaction
                                && t.DateOffset >= currentYear
                                && t.DateOffset < nextYear)
                    .ToArray()
                    .SelectMany(pt => pt.SubTransactionsRef.ToArray().Select(st => (pt, st)))
                    .ToLookup(t => t.st.CategoryRef);

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
                                && (t.FromAccountRef == null && t.ToAccountRef != null || t.ToAccountRef == null && t.FromAccountRef != null)
                                && t.DateOffset >= currentYear && t.DateOffset < nextYear)
                    .ToArray()
                    .Select(t => (Month: new DateTime(t.Date.Year, t.Date.Month, 1),
                        Sum: t.FromAccountRef is null ? -1L * t.Sum : t.Sum))
                    .GroupBy(t => t.Month, t => t.Sum)
                    .ToDictionary(g => g.Key, g => g.Sum());

                var unassignedTransactionsPerMonth = realm.All<Trans>()
                    .Where(t => t.TypeIndex == (int)TransType.Transaction
                                && t.CategoryRef == null
                                && t.DateOffset >= currentYear && t.DateOffset < nextYear)
                    .ToArray()
                    .Select(t => (Month: new DateTime(t.Date.Year, t.Date.Month, 1), t.Sum))
                    .Concat(realm.All<Trans>()
                        .Where(t => t.TypeIndex == (int)TransType.ParentTransaction
                                    && t.DateOffset >= currentYear && t.DateOffset < nextYear)
                        .ToArray()
                        .SelectMany(t => t.SubTransactionsRef.Where(st => st.CategoryRef == null), (t, st) => (Month: new DateTime(t.Date.Year, t.Date.Month, 1), st.Sum)))
                    .GroupBy(t => t.Month, t => t.Sum)
                    .ToDictionary(g => g.Key, g => g.Sum());

                var incomesPerMonth = realm.All<Category>()
                    .Where(c => c.IsIncomeRelevant)
                    .ToArray()
                    .SelectMany(c =>
                    {
                        var offsetCurrentYear = new DateTimeOffset(new DateTime(currentYear.Year, currentYear.Month, 1).OffsetMonthBy(-1 * c.MonthOffset), TimeSpan.Zero);
                        var offsetNextYear = new DateTimeOffset(new DateTime(nextYear.Year, nextYear.Month, 1).OffsetMonthBy(-1 * c.MonthOffset), TimeSpan.Zero);
                        var transactions = realm.All<Trans>()
                            .Where(t => t.TypeIndex == (int) TransType.Transaction 
                                        && t.CategoryRef == c 
                                        && t.DateOffset >= offsetCurrentYear 
                                        && t.DateOffset < offsetNextYear)
                            .ToArray()
                            .Select(t =>
                            {
                                var offsetDate = new DateTime(t.DateOffset.Year, t.DateOffset.Month, 1).OffsetMonthBy(c.MonthOffset);
                                return (offsetDate, t.Sum);
                            });
                        var subTransactions = realm.All<Trans>()
                            .Where(t => t.TypeIndex == (int)TransType.ParentTransaction
                                        && t.DateOffset >= offsetCurrentYear
                                        && t.DateOffset < offsetNextYear)
                            .ToArray()
                            .SelectMany(t =>
                            {
                                var offsetDate = new DateTime(t.DateOffset.Year, t.DateOffset.Month, 1).OffsetMonthBy(c.MonthOffset);
                                return t.SubTransactionsRef.Where(st => st.CategoryRef == c).ToArray().Select(st => (offsetDate, st.Sum));
                            });
                        return transactions.Concat(subTransactions);
                    })
                    .Concat(realm.All<Account>()
                        .Where(a => a.StartingDateOffset >= currentYear && a.StartingDateOffset < nextYear)
                        .ToArray()
                        .Select(a => (offsetDate: new DateTime(a.StartingDateOffset.Year, a.StartingDateOffset.Month, 1), Sum: a.StartingBalance)))
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
                    //// Total previous income
                    //realm.All<Category>()
                    //.Where(c => c.IsIncomeRelevant)
                    //.ToArray()
                    //.Select(c =>
                    //{
                    //    var offsetCurrentYear = new DateTimeOffset(new DateTime(currentYear.Year, currentYear.Month, 1).OffsetMonthBy(-1 * c.MonthOffset), TimeSpan.Zero);
                    //    var transactionsSum = realm.All<Trans>()
                    //        .Where(t => t.TypeIndex == (int)TransType.Transaction
                    //                    && t.CategoryRef == c
                    //                    && t.DateOffset < offsetCurrentYear)
                    //        .ToArray()
                    //        .Sum(t =>t.Sum);
                    //    var subTransactionsSum = realm.All<Trans>()
                    //        .Where(t => t.TypeIndex == (int)TransType.ParentTransaction
                    //                    && t.DateOffset < offsetCurrentYear)
                    //        .ToArray()
                    //        .Sum(t => t.SubTransactionsRef.Where(st => st.CategoryRef == c).ToArray().Sum(st => st.Sum));
                    //    return transactionsSum + subTransactionsSum;
                    //})
                    //.Sum()
                    //// Total previous account starting-balances
                    //+ realm.All<Account>()
                    //    .Where(a => a.StartingDateOffset < currentYear)
                    //    .ToArray()
                    //    .Sum(a => a.StartingBalance)
                    //// Total previous unassigned transaction sums
                    //+ realm.All<Trans>()
                    //    .Where(t => t.TypeIndex == (int)TransType.Transaction
                    //                && t.CategoryRef == null
                    //                && t.DateOffset < currentYear)
                    //    .ToArray()
                    //    .Sum(t => t.Sum)
                    //// Total previous dangling transfer sums
                    //+ realm.All<Trans>()
                    //    .Where(t => t.TypeIndex == (int)TransType.Transfer
                    //                && (t.FromAccountRef == null && t.ToAccountRef != null || t.ToAccountRef == null && t.FromAccountRef != null)
                    //                && t.DateOffset < currentYear)
                    //    .ToArray()
                    //    .Sum(t => t.FromAccountRef is null ? -1L * t.Sum : t.Sum)
                    totalGlobalPotMoney
                    // Total previous budget and negative balances
                    + expensesFromCategoryPots
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
