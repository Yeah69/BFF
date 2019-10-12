using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Persistence.Import;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using MoreLinq;
using MrMeeseeks.Extensions;
using MrMeeseeks.Utility;
using Realms;

namespace BFF.Persistence.Realm.ORM
{
    internal class RealmExportingOrm : IExportingOrm
    {
        private readonly IRealmOperations _realmOperations;

        public RealmExportingOrm(
            IRealmOperations realmOperations)
        {
            _realmOperations = realmOperations;
        }

        public Task PopulateDatabaseAsync(IRealmExportContainerData exportContainer)
        {
            return _realmOperations.RunActionAsync(Inner);

            void Inner(Realms.Realm realm)
            {

                realm.Write(() =>
                {
                    exportContainer.Accounts.ForEach(a => realm.Add(a as RealmObject));
                    exportContainer.Payees.ForEach(p => realm.Add(p as RealmObject));
                    exportContainer.Categories.ForEach(mc => realm.Add(mc as RealmObject));
                    exportContainer.IncomeCategories.ForEach(ic => realm.Add(ic as RealmObject));
                    exportContainer.Flags.ForEach(f => realm.Add(f as RealmObject));
                    exportContainer.Trans.ForEach(t => realm.Add(t as RealmObject));
                    exportContainer.SubTransactions.ForEach(st => realm.Add(st as RealmObject));
                    exportContainer.BudgetEntries.ForEach(be => realm.Add(be as RealmObject));
                    var dbSetting = new DbSetting
                    {
                        NextSubTransactionId = exportContainer.SubTransactions.Count,
                        NextTransId = exportContainer.Trans.Count,
                        NextCategoryId = exportContainer.IncomeCategories.Count + exportContainer.Categories.Count,
                        NextBudgetEntryId = exportContainer.BudgetEntries.Count
                    };
                    realm.Add(dbSetting);

                    // Create budget cache

                    var categoryGroupedBudgetEntries = exportContainer
                        .BudgetEntries 
                        .GroupBy(
                            be => be.Category, 
                            be => (Month: new DateTimeOffset(be.Month.Year, be.Month.Month, 1, 0, 0, 0, TimeSpan.Zero), be.Budget));
                    var categoryGroupedTransactions = exportContainer
                        .Trans
                        .Where(t => t.TypeIndex == (int) TransType.Transaction && t.Category != null && !t.Category.IsIncomeRelevant)
                        .Select(t => (CategoryRef: t.Category, Month: new DateTimeOffset(t.Date.Year, t.Date.Month, 1, 0, 0, 0, TimeSpan.Zero), t.Sum))
                        .Concat(
                            exportContainer
                                .SubTransactions
                                .Where(st => !st.Category.IsIncomeRelevant)
                                .Select(st => (CategoryRef: st.Category, Month: new DateTimeOffset(st.Parent.Date.Year, st.Parent.Date.Month, 1, 0, 0, 0, TimeSpan.Zero), st.Sum)))
                        .GroupBy(
                            t => t.CategoryRef,
                            t => (t.Month, t.Sum));

                    categoryGroupedBudgetEntries
                        .FullJoin(
                            categoryGroupedTransactions,
                            g => g.Key,
                            groupedBudgets => GenerateBudgetCacheEntriesFor(
                                groupedBudgets.Key,
                                groupedBudgets,
                                Enumerable.Empty<(DateTimeOffset, long)>()),
                            groupedOutflows => GenerateBudgetCacheEntriesFor(
                                groupedOutflows.Key,
                                Enumerable.Empty<(DateTimeOffset, long)>(),
                                groupedOutflows),
                            (groupedBudgets, groupedOutflows) => GenerateBudgetCacheEntriesFor(
                                groupedBudgets.Key,
                                groupedBudgets,
                                groupedOutflows))
                        .SelectMany(Basic.Identity)
                        .ForEach(bce => realm.Add(bce));

                    exportContainer.Accounts
                        // Account starting balances
                        .Select(a => (
                            Month: new DateTimeOffset(a.StartingDate.Year, a.StartingDate.Month, 1, 0, 0, 0,
                                TimeSpan.Zero), a.StartingBalance))
                        // Unassigned Transactions and income transactions
                        .Concat(exportContainer.Trans
                            .Where(t => t.TypeIndex == (int) TransType.Transaction && (t.Category is null || t.Category.IsIncomeRelevant))
                            .Select(t =>
                            {
                                if (t.Category is null || t.Category.Month == 0)
                                    return (Month: new DateTimeOffset(t.Date.Year, t.Date.Month, 1, 0, 0, 0, TimeSpan.Zero),
                                        t.Sum);
                                var offsetMonth = t.Date.OffsetMonthBy(t.Category.Month);
                                return (Month: new DateTimeOffset(offsetMonth.Year, offsetMonth.Month, 1, 0, 0, 0, TimeSpan.Zero),
                                        t.Sum);
                            }))
                        // Unassigned sub-transactions and income sub-transactions
                        .Concat(exportContainer.SubTransactions
                            .Where(st => st.Category is null || st.Category.IsIncomeRelevant)
                            .Select(st =>
                            {
                                if (st.Category is null || st.Category.Month == 0)
                                    return (Month: new DateTimeOffset(st.Parent.Date.Year, st.Parent.Date.Month, 1, 0, 0, 0, TimeSpan.Zero),
                                        st.Sum);
                                var offsetMonth = st.Parent.Date.OffsetMonthBy(st.Category.Month);
                                return (Month: new DateTimeOffset(offsetMonth.Year, offsetMonth.Month, 1, 0, 0, 0, TimeSpan.Zero),
                                    st.Sum);
                            }))
                        // Dangling transfers
                        .Concat(exportContainer.Trans
                            .Where(t => t.TypeIndex == (int) TransType.Transfer && (t.ToAccount is null && t.FromAccount != null || t.FromAccount is null && t.ToAccount != null))
                            .Select(t => (Month: new DateTimeOffset(t.Date.Year, t.Date.Month, 1, 0, 0, 0, TimeSpan.Zero), t.ToAccount is null ? -1L * t.Sum : t.Sum)))
                        .GroupBy(t => t.Month, t => t.Item2)
                        .OrderBy(g => g.Key)
                        .Scan(
                            new BudgetCacheEntry
                            {
                                Category = null,
                                Month = DateTimeOffset.MinValue,
                                Balance = 0,
                                TotalBudget = 0,
                                TotalNegativeBalance = 0
                            },
                            (previous, current) =>
                                new BudgetCacheEntry
                                {
                                    Category = null,
                                    Month = current.Key,
                                    Balance = 0,
                                    TotalBudget = previous.TotalBudget + current.Sum(),
                                    TotalNegativeBalance = 0
                                })
                        .Skip(1)
                        .ForEach(bce => realm.Add(bce));

                    IEnumerable<BudgetCacheEntry> GenerateBudgetCacheEntriesFor(
                        Category category,
                        IEnumerable<(DateTimeOffset Month, long Budget)> budgets,
                        IEnumerable<(DateTimeOffset Month, long Sum)> outflows)
                    {
                        return budgets.GroupBy(t => t.Month, t => t.Budget)
                            .FullJoin(
                                outflows.GroupBy(t => t.Month, t => t.Sum),
                                g => g.Key,
                                g => (g.Key, g.First(), 0L),
                                g => (g.Key, 0L, g.Sum()),
                                (b, o) => (b.Key, b.First(), o.Sum()))
                            .OrderBy(t => t.Key)
                            .Scan(
                                new BudgetCacheEntry
                                {
                                    Category = category,
                                    Month = DateTimeOffset.MinValue,
                                    Balance = 0,
                                    TotalBudget = 0,
                                    TotalNegativeBalance = 0
                                },
                                (previous, current) =>
                                {
                                    var currentBalance = Math.Max(0L, previous.Balance) + current.Item2 + current.Item3;
                                    return new BudgetCacheEntry
                                    {
                                        Category = category,
                                        Month = current.Key,
                                        Balance = currentBalance,
                                        TotalBudget = previous.TotalBudget + current.Item2,
                                        TotalNegativeBalance =
                                            previous.TotalNegativeBalance +
                                            Math.Min(0L, currentBalance)
                                    };
                                })
                            .Skip(1);
                    }
                });
            }
        }
    }
}
