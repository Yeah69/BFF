using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using BFF.Model;
using BFF.Persistence.Common;
using BFF.Persistence.Helper;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using MoreLinq.Extensions;
using MrMeeseeks.Extensions;
using MrMeeseeks.Reactive.Extensions;
using Account = BFF.Persistence.Realm.Models.Persistence.Account;
using BudgetEntry = BFF.Persistence.Realm.Models.Persistence.BudgetEntry;
using Category = BFF.Persistence.Realm.Models.Persistence.Category;

namespace BFF.Persistence.Realm.ORM
{
    internal class RealmBudgetOrm : IBudgetOrm, IDisposable
    {
        private readonly IRealmOperations _realmOperations;
        private readonly IClearBudgetCache _clearBudgetCache;

        private readonly BudgetDataCache _cache = new BudgetDataCache();
        
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public RealmBudgetOrm(
            IRealmOperations realmOperations,
            IObserveClearBudgetCache observeBudgetCache,
            IClearBudgetCache clearBudgetCache)
        {
            _realmOperations = realmOperations;
            _clearBudgetCache = clearBudgetCache;
            observeBudgetCache.Observe.Subscribe(_ => _cache.Clear()).CompositeDisposalWith(_compositeDisposable);
            Disposable.Create(() => _cache.Clear()).CompositeDisposalWith(_compositeDisposable);
        }

        public Task<BudgetBlock> FindAsync(int year)
        {
            var currentYearMonthIndex = new DateTimeOffset(year, 1, 1, 0, 0, 0, TimeSpan.Zero).ToMonthIndex();

            var nextYearMonthIndex = new DateTimeOffset(year + 1, 1, 1, 0, 0, 0, TimeSpan.Zero).ToMonthIndex();

            var monthIndicesOfYear = Enumerable
                .Range(0, 11)
                .Scan(
                    currentYearMonthIndex, 
                    (prev, _) => prev + 1)
                .ToArray();
            return _realmOperations.RunFuncAsync(realm =>
            {
                var categories = realm.All<Category>()
                    .Where(c => !c.IsIncomeRelevant)
                    .ToArray(); 
                var initialCacheEntriesForCategories = GetInitialCategoryValues(realm, currentYearMonthIndex)
                    .ToArray();

                var categoryToInitialBalance = initialCacheEntriesForCategories.ToDictionary(t => t.Category, t => Math.Max(0L, t.Balance));

                var categoryToBudgetEntryLookup = realm.All<BudgetEntry>()
                    .Where(be => be.MonthIndex >= currentYearMonthIndex && be.MonthIndex < nextYearMonthIndex)
                    .ToArray()
                    .ToLookup(be => be.Category);

                var categoryToTransactionLookup = realm.All<Trans>()
                    .Where(t => t.TypeIndex == (int) TransType.Transaction 
                                && t.MonthIndex >= currentYearMonthIndex
                                && t.MonthIndex < nextYearMonthIndex
                                && t.Category != null)
                    .ToArray()
                    .ToLookup(t => t.Category);

                var categoryToSubTransactionLookup = realm.All<Trans>()
                    .Where(t => t.TypeIndex == (int)TransType.ParentTransaction
                                && t.MonthIndex >= currentYearMonthIndex
                                && t.MonthIndex < nextYearMonthIndex)
                    .ToArray()
                    .SelectMany(pt => pt.SubTransactions.ToArray().Select(st => (pt, st)))
                    .ToLookup(t => t.st.Category);

                var budgetEntriesPerMonth = categories
                    .SelectMany(c =>
                    {
                        var initialBalance = categoryToInitialBalance.TryGetValue(c, out var ib) ? ib : 0L;
                        var budgetEntries = categoryToBudgetEntryLookup.Contains(c) 
                            ? categoryToBudgetEntryLookup[c]
                                .ToDictionary(be => be.MonthIndex)
                            : new Dictionary<int, BudgetEntry>();
                        var transactionOutflows = categoryToTransactionLookup.Contains(c)
                            ? categoryToTransactionLookup[c]
                                .GroupBy(t => t.MonthIndex, t => t.Sum)
                                .ToDictionary(g => g.Key, g => g.Sum())
                            : new Dictionary<int, long>();
                        var subTransactionOutflows = categoryToSubTransactionLookup.Contains(c)
                            ? categoryToSubTransactionLookup[c]
                                .GroupBy(t => t.pt.MonthIndex, t => t.st.Sum)
                                .ToDictionary(g => g.Key, g => g.Sum())
                            : new Dictionary<int, long>();

                        var currentBalance = initialBalance;
                        var list = new List<(int MonthIndex, (long Budget, long Outflow, long Balance) Tuple)>();
                        foreach (var monthIndex in monthIndicesOfYear)
                        {
                            var budgetEntry = budgetEntries.TryGetValue(monthIndex, out var be) ? be : null;
                            var transactionOutflow = transactionOutflows.TryGetValue(monthIndex, out var tOut) ? tOut : 0L;
                            var subTransactionOutflow = subTransactionOutflows.TryGetValue(monthIndex, out var stOut) ? stOut : 0L;

                            var budget = budgetEntry?.Budget ?? 0L;
                            var outflow = transactionOutflow + subTransactionOutflow;
                            var balance = currentBalance + budget + outflow;

                            list.Add((monthIndex, (budget, outflow, balance)));
                            currentBalance = Math.Max(0L, balance);
                        }

                        return list.ToReadOnlyList();
                    })
                    .GroupBy(t => t.MonthIndex, t => t.Tuple)
                    .ToDictionary(g => g.Key, g =>
                    {
                        return (
                            g.Sum(be => be.Budget),
                            g.Sum(be => be.Outflow),
                            g.Sum(be => be.Budget), 
                            g.Sum(be => Math.Min(0, be.Balance)));
                    });


                var danglingTransfersPerMonth = realm.All<Trans>()
                    .Where(t => t.TypeIndex == (int)TransType.Transfer
                                && (t.FromAccount == null && t.ToAccount != null || t.ToAccount == null && t.FromAccount != null)
                                && t.MonthIndex >= currentYearMonthIndex && t.MonthIndex < nextYearMonthIndex)
                    .ToArray()
                    .Select(t => (t.MonthIndex, Sum: t.FromAccount is null ? -1L * t.Sum : t.Sum))
                    .GroupBy(t => t.MonthIndex, t => t.Sum)
                    .ToDictionary(g => g.Key, g => g.Sum());

                var unassignedTransactionsPerMonth = realm.All<Trans>()
                    .Where(t => t.TypeIndex == (int)TransType.Transaction
                                && t.Category == null
                                && t.MonthIndex >= currentYearMonthIndex && t.MonthIndex < nextYearMonthIndex)
                    .ToArray()
                    .Select(t => (t.MonthIndex, t.Sum))
                    .Concat(realm.All<Trans>()
                        .Where(t => t.TypeIndex == (int)TransType.ParentTransaction
                                    && t.MonthIndex >= currentYearMonthIndex && t.MonthIndex < nextYearMonthIndex)
                        .ToArray()
                        .SelectMany(t => t.SubTransactions
                            .Where(st => st.Category == null), (t, st) => (t.MonthIndex, st.Sum)))
                    .GroupBy(t => t.MonthIndex, t => t.Sum)
                    .ToDictionary(g => g.Key, g => g.Sum());

                var incomesPerMonth = realm.All<Category>()
                    .Where(c => c.IsIncomeRelevant)
                    .ToArray()
                    .SelectMany(c =>
                    {
                        var offsetCurrentYear = currentYearMonthIndex - c.IncomeMonthOffset;
                        var offsetNextYear = nextYearMonthIndex - c.IncomeMonthOffset;
                        var transactions = realm.All<Trans>()
                            .Where(t => t.TypeIndex == (int) TransType.Transaction 
                                        && t.Category == c 
                                        && t.MonthIndex >= offsetCurrentYear 
                                        && t.MonthIndex < offsetNextYear)
                            .ToArray()
                            .Select(t => (offsetMonthIndex: t.MonthIndex + c.IncomeMonthOffset, t.Sum));
                        var subTransactions = realm.All<Trans>()
                            .Where(t => t.TypeIndex == (int)TransType.ParentTransaction
                                        && t.MonthIndex >= offsetCurrentYear
                                        && t.MonthIndex < offsetNextYear)
                            .ToArray()
                            .SelectMany(t =>
                            {
                                var offsetDate = t.MonthIndex + c.IncomeMonthOffset;
                                return t.SubTransactions
                                    .Where(st => st.Category == c)
                                    .ToArray()
                                    .Select(st => (offsetMonthIndex: offsetDate, st.Sum));
                            });
                        return transactions.Concat(subTransactions);
                    })
                    .Concat(realm.All<Account>()
                        .Where(a => a.StartingMonthIndex >= currentYearMonthIndex && a.StartingMonthIndex < nextYearMonthIndex)
                        .ToArray()
                        .Select(a => (offsetMonthIndex: a.StartingMonthIndex, Sum: a.StartingBalance)))
                    .GroupBy(t => t.offsetMonthIndex, t => t.Sum)
                    .ToDictionary(g => g.Key, g => g.Sum());

                var initialOverspentInPreviousMonth = initialCacheEntriesForCategories
                    .Where(t => t.LastMonthIndex == currentYearMonthIndex - 1 && t.Balance < 0)
                    .Sum(t => t.Balance);

                var totalGlobalPotMoney = CalculateGlobalPot(realm, currentYearMonthIndex);

                var expensesFromCategoryPots = initialCacheEntriesForCategories.Aggregate(0L,
                    (previous, current) => previous - current.TotalBudget + current.TotalNegativeBalance);

                var initialNotBudgetedOrOverBudgeted =
                    totalGlobalPotMoney
                    + expensesFromCategoryPots
                    // Subtract "initialOverspentInPreviousMonth" because it will be used in the budget month and is included in "expensesFromCategoryPots"
                    // hence, it would be accounted two times otherwise
                    - initialOverspentInPreviousMonth;

                return new BudgetBlock
                {
                    BudgetDataPerMonth = budgetEntriesPerMonth,
                    IncomesPerMonth = incomesPerMonth,
                    DanglingTransfersPerMonth = danglingTransfersPerMonth,
                    UnassignedTransactionsPerMonth = unassignedTransactionsPerMonth,
                    InitialNotBudgetedOrOverbudgeted = initialNotBudgetedOrOverBudgeted,
                    InitialOverspentInPreviousMonth = initialOverspentInPreviousMonth
                };
            });

            IEnumerable<(Category? Category, int LastMonthIndex, long Balance, long TotalBudget, long TotalNegativeBalance)> GetInitialCategoryValues(
            Realms.Realm realm,
            int untilMonth)
            {
                var budgetEntries = realm
                        .All<BudgetEntry>()
                        .Where(be => be.MonthIndex < untilMonth)
                        .ToList()
                        .Select(be => (be.Category, be.MonthIndex, be.Budget))
                        .ToList();
                
                List<(Category? Category, int MonthIndex, long Sum)> transactions = realm
                    .All<Trans>()
                    .Where(t => t.TypeIndex == (int)TransType.Transaction && t.MonthIndex < untilMonth)
                    .ToList()
                    .Select(t => (t.Category, t.MonthIndex, t.Sum))
                    .Concat(realm
                        .All<SubTransaction>()
                        .ToList()
                        .Where(st => st.Parent?.MonthIndex < untilMonth)
                        .Select(st => (st.Category,
                            st.Parent?.MonthIndex ?? throw new NullReferenceException(),
                            st.Sum)))
                    .ToList();

                return budgetEntries.FullGroupJoin(
                    transactions,
                    be => be.Category,
                    t => t.Category,
                    (category, budgets, outflows) =>
                    {
                        (int m, long balance, long totalBudget, long totalNegativeBalance) = budgets
                            .FullGroupJoin(
                                outflows,
                                b => b.MonthIndex,
                                o => o.MonthIndex,
                                (month, bs, os) => (Month: month, Budget: bs.Select(b => b.Budget).FirstOrDefault(), Outflow: (os.Any() ? os.Select(o => o.Sum).Sum() : 0L)))
                            .OrderBy(t => t.Month)
                            .Aggregate((Month: 0, Balance: 0L, TotalBudget: 0L, TotalNegativeBalance: 0L), (previous, current) =>
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

            long CalculateGlobalPot(Realms.Realm realm, int monthIndex)
            {
                return realm.All<Account>()
                    .Where(a => a.StartingMonthIndex < monthIndex)
                    .ToList()
                    // Account starting balances
                    .Select(a => a.StartingBalance)
                    // Unassigned Transactions
                    .Concat(realm.All<Trans>()
                        .Where(t => t.TypeIndex == (int)TransType.Transaction
                                    && t.MonthIndex < monthIndex
                                    && t.Category == null)
                        .ToList()
                        .Select(t => t.Sum))
                    // Unassigned sub-transactions
                    .Concat(realm.All<SubTransaction>()
                        .Where(st => st.Category == null)
                        .ToList()
                        .Where(st => st.Parent?.MonthIndex < monthIndex)
                        .Select(st => st.Sum))
                    // Income Transactions and SubTransactions
                    .Concat(realm.All<Category>()
                        .Where(c => c.IsIncomeRelevant)
                        .ToList()
                        .GroupBy(c => c.IncomeMonthOffset)
                        .SelectMany(g =>
                        {
                            var offsetMontOffset = monthIndex - g.Key;
                            return g.SelectMany(c => c
                                    .Transactions
                                    .Where(t => t.MonthIndex < offsetMontOffset)
                                    .ToList()
                                    .Select(t => t.Sum))
                                .Concat(g.SelectMany(c => c
                                    .SubTransactions
                                    .ToList()
                                    .Where(st => st.Parent?.MonthIndex < offsetMontOffset)
                                    .Select(st => st.Sum)));
                        }))
                    // Dangling transfers
                    .Concat(realm
                        .All<Trans>()
                        .Where(t => t.TypeIndex == (int)TransType.Transfer
                                    && t.MonthIndex < monthIndex
                                    && (t.ToAccount == null && t.FromAccount != null ||
                                     t.FromAccount == null && t.ToAccount != null))
                        .ToList()
                        .Select(t => t.ToAccount is null ? -1L * t.Sum : t.Sum))
                    .Sum();
            }
        }

        public Task<IReadOnlyList<(BudgetEntry? Entry, BudgetEntryData Data)>> FindAsync(int year, Category category) => 
            _realmOperations.RunFuncAsync(realm => FindAsyncInner(realm, year, category));

        private IReadOnlyList<(BudgetEntry? Entry, BudgetEntryData Data)> FindAsyncInner(Realms.Realm realm, int year, Category category)
        {
            var monthsOfYear = Enumerable
                .Range(0, 11)
                .Scan(
                    new DateTimeOffset(year, 1, 1, 0, 0, 0, TimeSpan.Zero), 
                    (dt, _) => dt.NextMonth())
                .ToArray();
            
            var list = new List<(BudgetEntry? Entry, BudgetEntryData Data)>();
            foreach (var month in monthsOfYear)
            {
                (BudgetEntry? budgetEntry, long budget, long outflow, long balance) = _cache.GetFor(category, month.Year, month.Month, realm);
                    
                (long aggregatedBudget, long aggregatedOutflow, long aggregatedBalance) = category
                    .IterateTreeBreadthFirst(c => c.Categories ?? Enumerable.Empty<Category>())
                    .Select(c => _cache.GetFor(c, month.Year, month.Month, realm))
                    .Aggregate((0L, 0L, 0L), (prev, cur) => (prev.Item1 + cur.Budget, prev.Item2 + cur.Outflow, prev.Item3 + cur.Balance));
                    
                list.Add(
                    (budgetEntry, 
                        new BudgetEntryData
                        {
                            Month = new DateTime(month.Year, month.Month, month.Day),
                            Budget = budget,
                            Outflow = outflow,
                            Balance = balance,
                            AggregatedBudget = aggregatedBudget,
                            AggregatedOutflow = aggregatedOutflow,
                            AggregatedBalance = aggregatedBalance
                        }));
            }
                
            return list.ToReadOnlyList();
        }

        public Task<long> GetAverageBudgetOfLastMonths(int currentMonthIndex, Category category, int monthCount) => 
            _realmOperations.RunFuncAsync(realm => 
                GetAverageBudgetOfLastMonthsInner(realm, currentMonthIndex, category, monthCount));

        private static long GetAverageBudgetOfLastMonthsInner(
            Realms.Realm realm,
            int currentMonthIndex, 
            Category category,
            int monthCount)
        {
            var firstMonthIndex = currentMonthIndex - monthCount;

            return realm.All<BudgetEntry>()
                .Where(be =>
                    be.Category == category && be.MonthIndex >= firstMonthIndex && be.MonthIndex < currentMonthIndex)
                .ToList()
                .Select(be => be.Budget)
                .Sum() / monthCount;
        }

        public Task<long> GetAverageOutflowOfLastMonths(int currentMonthIndex, Category category, int monthCount) => 
            _realmOperations.RunFuncAsync(realm => 
                GetAverageOutflowOfLastMonthsInner(realm, currentMonthIndex, category, monthCount));

        private static long GetAverageOutflowOfLastMonthsInner(
            Realms.Realm realm,
            int currentMonthIndex,
            Category category, 
            int monthCount)
        {
            var firstMonthIndex = currentMonthIndex - monthCount;

            var trans = realm.All<Trans>()
                .Where(t =>
                    t.Category == category && t.MonthIndex >= firstMonthIndex && t.MonthIndex < currentMonthIndex)
                .ToList();

            var transSum = trans
                .Where(t => t.TypeIndex == (int) TransType.Transaction)
                .Select(t => t.Sum)
                .Sum();

            var subTransSum = trans
                .Where(t => t.TypeIndex == (int)TransType.ParentTransaction)
                .SelectMany(t => t.SubTransactions)
                .ToList()
                .Select(st => st.Sum)
                .Sum();

            return (transSum + subTransSum) / monthCount;
        }

        public Task SetEmptyBudgetEntriesToAvgBudget(int monthIndex, int monthCount)
        {
            return SetEmptyBudgetEntriesToTemplate(monthIndex, Func);

            IEnumerable<(Category, long)> Func(IEnumerable<Category> categories, Realms.Realm realm) => categories
                .Select(c => (Category: c, Budget: GetAverageBudgetOfLastMonthsInner(realm, monthIndex, c, monthCount)))
                .Where(t => t.Budget > 0);
        }

        public Task SetEmptyBudgetEntriesToAvgOutflow(int monthIndex, int monthCount)
        {
            return SetEmptyBudgetEntriesToTemplate(monthIndex, Func);

            IEnumerable<(Category, long)> Func(IEnumerable<Category> categories, Realms.Realm realm) => categories
                .Select(c => (Category: c, Budget: -1 * GetAverageOutflowOfLastMonthsInner(realm, monthIndex, c, monthCount)))
                .Where(t => t.Budget != 0);
        }

        public Task SetEmptyBudgetEntriesToBalanceZero(int monthIndex)
        {
            DateTime dateTime = DateTimeExtensions.FromMonthIndex(monthIndex);
            return SetEmptyBudgetEntriesToTemplate(monthIndex, Func);

            IEnumerable<(Category, long)> Func(IEnumerable<Category> categories, Realms.Realm realm) => categories
                .Select(c =>
                {
                    var relevantData = FindAsyncInner(realm, dateTime.Year, c)[dateTime.Month - 1].Data;
                    return (Category: c, Budget: relevantData.Budget - relevantData.Balance);
                });
        }

        private Task SetEmptyBudgetEntriesToTemplate(int monthIndex, Func<IEnumerable<Category>, Realms.Realm, IEnumerable<(Category, long)>> func)
        {
            return _realmOperations.RunActionAsync(realm =>
            {
                var dbSetting = realm.All<DbSetting>().First();
                var nextId = dbSetting.NextBudgetEntryId;

                foreach ((Category category, long budget) in func(CategoriesWithEmptyBudgetEntry(realm, monthIndex), realm))
                {
                    var ro = new BudgetEntry{ Id = nextId++, Budget = budget, Category = category, MonthIndex = monthIndex};
                    realm.Write(() => realm.Add(ro));
                }

                realm.Write(() =>
                {
                    dbSetting.NextBudgetEntryId = nextId;
                    realm.Add(dbSetting, true);
                });
                _clearBudgetCache.Clear();
            });
            
            static IReadOnlyList<Category> CategoriesWithEmptyBudgetEntry(Realms.Realm realm, int monthIndex)
            {
                var allCategories = realm.All<Category>().ToList();

                var categoriesToExclude = realm
                    .All<BudgetEntry>()
                    .Where(be => be.MonthIndex == monthIndex)
                    .ToList()
                    .Select(be => be.Category)
                    .WhereNotNullRef()
                    .ToList();

                return allCategories.Except(categoriesToExclude).ToList();
            }
        }

        public Task SetAllBudgetEntriesToZero(int monthIndex)
        {
            return _realmOperations.RunActionAsync(realm =>
                realm.Write(() =>
                {
                    realm.RemoveRange(
                        realm
                            .All<BudgetEntry>()
                            .Where(be => be.MonthIndex == monthIndex));
                    _clearBudgetCache.Clear();
                }));
        }

        private class BudgetDataCache
        {
            private readonly Dictionary<(Category Category, int Year, int Month), (BudgetEntry? Entry, long Budget, long Outflow, long Balance)> _cache =
                new Dictionary<(Category Category, int Year, int Month), (BudgetEntry? Entry, long Budget, long Outflow, long Balance)>();

            public (BudgetEntry? Entry, long Budget, long Outflow, long Balance) GetFor(Category category, int yearNumber, int monthNumber, Realms.Realm realm)
            {
                if (_cache.TryGetValue((category, yearNumber, monthNumber), out var data))
                {
                    return data;
                }
                
                var currentYearMonthIndex = new DateTimeOffset(yearNumber, 1, 1, 0, 0, 0, TimeSpan.Zero).ToMonthIndex();

                var nextYearMonthIndex = new DateTimeOffset(yearNumber + 1, 1, 1, 0, 0, 0, TimeSpan.Zero).ToMonthIndex();

                var monthIndicesOfYear = Enumerable
                    .Range(0, 11)
                    .Scan(currentYearMonthIndex, (index, _) => index + 1)
                    .ToArray();
                
                var initialCacheEntries = GetInitialValues(realm, category, currentYearMonthIndex);

                var initialBalance = Math.Max(0L, initialCacheEntries.Balance);

                var budgetEntries = realm.All<BudgetEntry>()
                    .Where(be => be.MonthIndex >= currentYearMonthIndex 
                                 && be.MonthIndex < nextYearMonthIndex
                                 && be.Category == category)
                    .ToDictionary(be => be.MonthIndex, be => be);

                var transactions = realm.All<Trans>()
                    .Where(t => t.TypeIndex == (int) TransType.Transaction 
                                && t.MonthIndex >= currentYearMonthIndex
                                && t.MonthIndex < nextYearMonthIndex
                                && t.Category == category)
                    .ToArray();

                var subTransactions = realm.All<Trans>()
                    .Where(t => t.TypeIndex == (int)TransType.ParentTransaction
                                && t.MonthIndex >= currentYearMonthIndex
                                && t.MonthIndex < nextYearMonthIndex)
                    .ToArray()
                    .SelectMany(pt => 
                        pt.SubTransactions
                            .Where(st => st.Category == category)
                            .ToArray()
                            .Select(st => (pt, st)))
                    .ToArray();

                var transactionOutflows = transactions
                    .GroupBy(t => t.MonthIndex, t => t.Sum)
                    .ToDictionary(g => g.Key, g => g.Sum());
                var subTransactionOutflows = subTransactions
                    .GroupBy(t => t.pt.MonthIndex, t => t.st.Sum)
                    .ToDictionary(g => g.Key, g => g.Sum());

                var currentBalance = initialBalance;
                foreach (var monthIndex in monthIndicesOfYear)
                {
                    var budgetEntry = budgetEntries.TryGetValue(monthIndex, out var be) ? be : null;
                    var transactionOutflow = transactionOutflows.TryGetValue(monthIndex, out var tOut) ? tOut : 0L;
                    var subTransactionOutflow = subTransactionOutflows.TryGetValue(monthIndex, out var stOut) ? stOut : 0L;

                    var budget = budgetEntry?.Budget ?? 0L;
                    var outflow = transactionOutflow + subTransactionOutflow;
                    var balance = currentBalance + budget + outflow;

                    var month = DateTimeOffsetExtensions.FromMonthIndex(monthIndex);

                    _cache[(category, month.Year, month.Month)] = (budgetEntry, budget, outflow, balance);
                    currentBalance = Math.Max(0L, balance);
                }
                
                return _cache[(category, yearNumber, monthNumber)];
            }

            public void Clear()
            {
                _cache.Clear();
            }
            
            private static (int LastMonthIndex, long Balance, long TotalBudget, long TotalNegativeBalance) GetInitialValues(
            Realms.Realm realm,
            Category category,
            int untilMonthIndex)
            {
                var budgetEntries = realm
                        .All<BudgetEntry>()
                        .Where(be => be.MonthIndex < untilMonthIndex
                            && be.Category == category)
                        .ToList()
                        .Select(be => (be.MonthIndex, be.Budget))
                        .ToList();
                
                List<(int MonthIndex, long Sum)> transactions = realm
                    .All<Trans>()
                    .Where(t => t.TypeIndex == (int)TransType.Transaction 
                                && t.MonthIndex < untilMonthIndex
                                && t.Category == category)
                    .ToList()
                    .Select(t => (t.MonthIndex, t.Sum))
                    .Concat(realm
                        .All<SubTransaction>()
                        .Where(st => st.Category == category)
                        .ToList()
                        .Where(st => st.Parent?.MonthIndex < untilMonthIndex)
                        .Select(st => (st.Parent?.MonthIndex ?? throw new NullReferenceException(), st.Sum)))
                    .ToList();
                
                (int m, long balance, long totalBudget, long totalNegativeBalance) = budgetEntries
                    .FullGroupJoin(
                        transactions,
                        b => b.MonthIndex,
                        o => o.MonthIndex,
                        (monthIndex, bs, os) => 
                            (MonthIndex: monthIndex, Budget: bs.Select(b => b.Budget).FirstOrDefault(), Outflow: os.Any() ? os.Select(o => o.Sum).Sum() : 0L))
                    .OrderBy(t => t.MonthIndex)
                    .Aggregate((DateTimeOffset.MinValue.ToMonthIndex(), Balance: 0L, TotalBudget: 0L, TotalNegativeBalance: 0L), (previous, current) =>
                    {
                        var currentBalance = Math.Max(0L, previous.Balance) + current.Budget + current.Outflow;
                        return (
                            current.MonthIndex,
                            Balance: currentBalance,
                            TotalBudget: previous.TotalBudget + current.Budget,
                            TotalNegativeBalance:
                            previous.TotalNegativeBalance +
                            Math.Min(0L, currentBalance));
                    });
                return (m, balance, totalBudget, totalNegativeBalance);
            }
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }

    public struct BudgetBlock
    {
        /// <summary>
        /// Key is month index.
        /// </summary>
        internal IDictionary<int, (long Budget, long Outflow, long Balance, long Overspend)> BudgetDataPerMonth
        {
            get;
            set;
        }

        internal long InitialNotBudgetedOrOverbudgeted { get; set; }

        internal long InitialOverspentInPreviousMonth { get; set; }

        /// <summary>
        /// Key is month index.
        /// </summary>
        internal IDictionary<int, long> IncomesPerMonth { get; set; }

        /// <summary>
        /// Key is month index.
        /// </summary>
        internal IDictionary<int, long> DanglingTransfersPerMonth { get; set; }

        /// <summary>
        /// Key is month index.
        /// </summary>
        internal IDictionary<int, long> UnassignedTransactionsPerMonth { get; set; }
    }
}
