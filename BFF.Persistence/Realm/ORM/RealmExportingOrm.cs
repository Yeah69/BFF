using System;
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
        private readonly IBudgetOrm _budgetOrm;

        public RealmExportingOrm(
            IRealmOperations realmOperations,
            IBudgetOrm budgetOrm)
        {
            _realmOperations = realmOperations;
            _budgetOrm = budgetOrm;
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
                            groupedBudgets => _budgetOrm.GenerateBudgetCacheEntriesFor(
                                groupedBudgets.Key,
                                groupedBudgets,
                                Enumerable.Empty<(DateTimeOffset, long)>(),
                                0,
                                0,
                                0),
                            groupedOutflows => _budgetOrm.GenerateBudgetCacheEntriesFor(
                                groupedOutflows.Key,
                                Enumerable.Empty<(DateTimeOffset, long)>(),
                                groupedOutflows,
                                0,
                                0,
                                0),
                            (groupedBudgets, groupedOutflows) => _budgetOrm.GenerateBudgetCacheEntriesFor(
                                groupedBudgets.Key,
                                groupedBudgets,
                                groupedOutflows,
                                0,
                                0,
                                0))
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
                                if (t.Category is null || t.Category.IncomeMonthOffset == 0)
                                    return (Month: new DateTimeOffset(t.Date.Year, t.Date.Month, 1, 0, 0, 0, TimeSpan.Zero),
                                        t.Sum);
                                var offsetMonth = t.Date.OffsetMonthBy(t.Category.IncomeMonthOffset);
                                return (Month: new DateTimeOffset(offsetMonth.Year, offsetMonth.Month, 1, 0, 0, 0, TimeSpan.Zero),
                                        t.Sum);
                            }))
                        // Unassigned sub-transactions and income sub-transactions
                        .Concat(exportContainer.SubTransactions
                            .Where(st => st.Category is null || st.Category.IsIncomeRelevant)
                            .Select(st =>
                            {
                                if (st.Category is null || st.Category.IncomeMonthOffset == 0)
                                    return (Month: new DateTimeOffset(st.Parent.Date.Year, st.Parent.Date.Month, 1, 0, 0, 0, TimeSpan.Zero),
                                        st.Sum);
                                var offsetMonth = st.Parent.Date.OffsetMonthBy(st.Category.IncomeMonthOffset);
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
                });
            }
        }
    }
}
