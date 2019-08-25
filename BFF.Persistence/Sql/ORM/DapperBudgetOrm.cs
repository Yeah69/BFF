using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;
using Dapper;
using MrMeeseeks.Extensions;
using MrMeeseeks.Utility;

namespace BFF.Persistence.Sql.ORM
{
    internal class DapperBudgetOrm : IBudgetOrm
    {
        private static readonly string NotBudgetedOrOverbudgetedQuery =
            $@"SELECT Total(Sum) as Sum FROM
    (
        SELECT Total({nameof(Account.StartingBalance)}) AS Sum FROM {nameof(Account)}s
        WHERE strftime('%Y', {nameof(Account.StartingDate)}) < @Year
        UNION ALL
        SELECT Total({nameof(Trans.Sum)}) AS Sum FROM {nameof(Trans)}s
        WHERE {nameof(Trans.Type)} = '{nameof(TransType.Transfer)}' AND {nameof(Trans.CategoryId)} IS NOT NULL AND strftime('%Y', {nameof(Trans.Date)}) < @Year
        UNION ALL
        SELECT - Total({nameof(Trans.Sum)}) AS Sum FROM {nameof(Trans)}s
        WHERE {nameof(Trans.Type)} = '{nameof(TransType.Transfer)}' AND {nameof(Trans.PayeeId)} IS NOT NULL AND strftime('%Y', {nameof(Trans.Date)}) < @Year
        UNION ALL
        SELECT Total({nameof(Trans.Sum)})
        FROM {nameof(Trans)}s
        INNER JOIN {nameof(Category)}s ON {nameof(Trans.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(Trans.CategoryId)} == {nameof(Category)}s.{nameof(Category.Id)} AND {nameof(Category)}s.{nameof(Category.IsIncomeRelevant)} == 0
        WHERE strftime('%Y', {nameof(Trans.Date)}) < @Year
        UNION ALL
        SELECT Total({nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)})
        FROM {nameof(SubTransaction)}s
        INNER JOIN {nameof(Category)}s ON {nameof(SubTransaction)}s.{nameof(SubTransaction.CategoryId)} == {nameof(Category)}s.{nameof(Category.Id)} AND {nameof(Category)}s.{nameof(Category.IsIncomeRelevant)} == 0
        INNER JOIN {nameof(Trans)}s ON {nameof(SubTransaction)}s.{nameof(SubTransaction.ParentId)} = {nameof(Trans)}s.{nameof(Trans.Id)}
        WHERE strftime('%Y', {nameof(Trans.Date)}) < @Year
        UNION ALL
        SELECT Total({nameof(Trans.Sum)}) AS Sum FROM {nameof(Trans)}s 
        WHERE {nameof(Trans.Type)} = '{nameof(TransType.Transaction)}' AND {nameof(Trans.CategoryId)} IS NULL AND strftime('%Y', {nameof(Trans.Date)}) < @Year
        UNION ALL
        SELECT Total({nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)}) AS Sum
        FROM {nameof(SubTransaction)}s
        INNER JOIN {nameof(Trans)}s ON {nameof(SubTransaction)}s.{nameof(SubTransaction.ParentId)} = {nameof(Trans)}s.{nameof(Trans.Id)}
        WHERE {nameof(SubTransaction)}s.{nameof(SubTransaction.CategoryId)} IS NULL AND strftime('%Y', {nameof(Trans.Date)}) < @Year
    );";

        private static string IncomeForCategoryQuery(int offset) =>
            $@"
    SELECT Total({nameof(Trans.Sum)})
    FROM {nameof(Trans)}s
    WHERE {nameof(Trans.CategoryId)} == @CategoryId AND strftime('%Y', {nameof(Trans.Date)}{(offset == 0 ? "" : $", 'start of month', '+15 days', '{offset:+0;-0;0} months'")}) == @Year AND strftime('%m', {nameof(Trans.Date)}{(offset == 0 ? "" : $", 'start of month', '+15 days', '{offset:+0;-0;0} months'")}) == @Month;";

        private static string IncomeForCategoryUntilMonthQuery(int offset) =>
            $@"
    SELECT Total({nameof(Trans.Sum)})
    FROM {nameof(Trans)}s
    WHERE {nameof(Trans.CategoryId)} == @CategoryId AND strftime('%Y', {nameof(Trans.Date)}{(offset == 0 ? "" : $", 'start of month', '+15 days', '{offset:+0;-0;0} months'")}) < @Year;";

        private static string DanglingTransferQuery =>
            $@"
    SELECT (SELECT Total({nameof(Trans.Sum)}) 
        FROM {nameof(Trans)}s 
        WHERE {nameof(Trans.Type)} = '{TransType.Transfer}' AND {nameof(Trans.PayeeId)} IS NULL AND strftime('%Y', {nameof(Trans.Date)}) == @Year AND strftime('%m', {nameof(Trans.Date)}) == @Month)
    - (SELECT Total({nameof(Trans.Sum)})
        FROM {nameof(Trans)}s 
        WHERE {nameof(Trans.Type)} = '{TransType.Transfer}' AND {nameof(Trans.CategoryId)} IS NULL AND strftime('%Y', {nameof(Trans.Date)}) == @Year AND strftime('%m', {nameof(Trans.Date)}) == @Month)";

        private static string DanglingTransferUntilMonthQuery =>
            $@"
    SELECT (SELECT Total({nameof(Trans.Sum)}) 
        FROM {nameof(Trans)}s 
        WHERE {nameof(Trans.Type)} = '{TransType.Transfer}' AND {nameof(Trans.PayeeId)} IS NULL AND strftime('%Y', {nameof(Trans.Date)}) < @Year)
    - (SELECT Total({nameof(Trans.Sum)}) 
        FROM {nameof(Trans)}s 
        WHERE {nameof(Trans.Type)} = '{TransType.Transfer}' AND {nameof(Trans.CategoryId)} IS NULL AND strftime('%Y', {nameof(Trans.Date)}) < @Year)";

        private class OutflowResponse
        {
            public DateTime Month { get; set; }
            public long Sum { get; set; }
        }

        private static readonly string BudgetQuery =
            $@"SELECT {nameof(BudgetEntry.Id)}, {nameof(BudgetEntry.CategoryId)}, {nameof(BudgetEntry.Month)}, {nameof(BudgetEntry.Budget)}
  FROM {nameof(BudgetEntry)}s
  WHERE {nameof(BudgetEntry.CategoryId)} = @CategoryId AND strftime('%Y', {nameof(BudgetEntry.Month)}) <= @Year
  ORDER BY {nameof(BudgetEntry.Month)};";

        private static readonly string BudgetThisYearQuery =
            $@"SELECT {nameof(BudgetEntry.Id)}, {nameof(BudgetEntry.CategoryId)}, {nameof(BudgetEntry.Month)}, {nameof(BudgetEntry.Budget)}
  FROM {nameof(BudgetEntry)}s
  WHERE {nameof(BudgetEntry.CategoryId)} = @CategoryId AND strftime('%Y', {nameof(BudgetEntry.Month)}) = @Year
  ORDER BY {nameof(BudgetEntry.Month)};";

        private static readonly string OutflowQuery =
            $@"SELECT Date as Month, Total(Sum) as Sum FROM
(
  SELECT strftime('%Y', {nameof(Trans.Date)}) || '-' || strftime('%m', {nameof(Trans.Date)}) || '-' || '01' AS Date, {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(Trans.CategoryId)} = @CategoryId AND strftime('%Y', Date) <= @Year
  UNION ALL
  SELECT strftime('%Y', {nameof(Trans.Date)}) || '-' || strftime('%m', {nameof(Trans.Date)}) || '-'  || '01' AS Date, subs.{nameof(SubTransaction.Sum)} FROM
    (SELECT {nameof(SubTransaction.ParentId)}, {nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)} FROM {nameof(SubTransaction)}s WHERE {nameof(SubTransaction)}s.{nameof(SubTransaction.CategoryId)} = @CategoryId) subs
      INNER JOIN {nameof(Trans)}s ON subs.{nameof(SubTransaction.ParentId)} = {nameof(Trans)}s.{nameof(Trans.Id)}
  WHERE strftime('%Y', Date) <= @Year
)
  GROUP BY Date
  ORDER BY Date;";

        private static readonly string OutflowThisYearQuery =
            $@"SELECT Date as Month, Total(Sum) as Sum FROM
(
  SELECT strftime('%Y', {nameof(Trans.Date)}) || '-' || strftime('%m', {nameof(Trans.Date)}) || '-' || '01' AS Date, {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(Trans.CategoryId)} = @CategoryId AND strftime('%Y', Date) = @Year
  UNION ALL
  SELECT strftime('%Y', {nameof(Trans.Date)}) || '-' || strftime('%m', {nameof(Trans.Date)}) || '-'  || '01' AS Date, subs.{nameof(SubTransaction.Sum)} FROM
    (SELECT {nameof(SubTransaction.ParentId)}, {nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)} FROM {nameof(SubTransaction)}s WHERE {nameof(SubTransaction)}s.{nameof(SubTransaction.CategoryId)} = @CategoryId) subs
      INNER JOIN {nameof(Trans)}s ON subs.{nameof(SubTransaction.ParentId)} = {nameof(Trans)}s.{nameof(Trans.Id)}
  WHERE strftime('%Y', Date) = @Year
)
  GROUP BY Date
  ORDER BY Date;";

        private static readonly string UnassignedTransactionsQuery = $@"
SELECT Total(Sum) FROM
(
    SELECT {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} = '{nameof(TransType.Transaction)}' AND {nameof(Trans.CategoryId)} IS NULL AND strftime('%Y', {nameof(Trans.Date)}) = @Year AND strftime('%m', {nameof(Trans.Date)}) = @Month
    UNION ALL
    SELECT {nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)}
    FROM {nameof(SubTransaction)}s
    INNER JOIN {nameof(Trans)}s ON {nameof(SubTransaction)}s.{nameof(SubTransaction.ParentId)} = {nameof(Trans)}s.{nameof(Trans.Id)}
    WHERE {nameof(SubTransaction)}s.{nameof(SubTransaction.CategoryId)} IS NULL AND strftime('%Y', {nameof(Trans.Date)}) = @Year AND strftime('%m', {nameof(Trans.Date)}) = @Month
)";

        private readonly IProvideSqliteConnection _provideConnection;
        private readonly Func<BudgetEntry> _budgetEntryFactory;

        public DapperBudgetOrm(
            IProvideSqliteConnection provideConnection,
            Func<BudgetEntry> budgetEntryFactory)
        {
            _provideConnection = provideConnection;
            _budgetEntryFactory = budgetEntryFactory;
        }

        public async Task<BudgetBlock> FindAsync(int year, long[] categoryIds, (long Id, int MonthOffset)[] incomeCategories)
        {
            IDictionary<DateTime, IList<(IBudgetEntrySql Entry, long Outflow, long Balance)>> budgetEntriesPerMonth;
            long initialNotBudgetedOrOverbudgeted = 0;
            long initialOverspentInPreviousMonth;
            IDictionary<DateTime, long> incomesPerMonth;
            IDictionary<DateTime, long> danglingTransfersPerMonth;
            IDictionary<DateTime, long> unassignedTransactionsPerMonth;

            IList<DateTime> monthRange = new List<DateTime>();

            var currentMonth = new DateTime(year, 1, 1);

            do
            {
                monthRange.Add(currentMonth);
                currentMonth = currentMonth.NextMonth();

            } while (currentMonth.Year == year);

            using (TransactionScope transactionScope = new TransactionScope())
            using (IDbConnection connection = _provideConnection.Connection)
            {
                var budgetEntries = new List<(IBudgetEntrySql Entry, long Outflow, long Balance)>();

                IDictionary<long, long> entryBudgetValuePerCategoryId = new Dictionary<long, long>();
                
                foreach (var categoryId in categoryIds)
                {
                    var budgetEntriesTask = connection
                        .QueryAsync<BudgetEntry>(
                            BudgetQuery,
                            new
                            {
                                CategoryId = categoryId,
                                Year = $"{year:0000}"
                            });

                    var outflowTask = connection
                        .QueryAsync<OutflowResponse>(
                            OutflowQuery,
                            new
                            {
                                CategoryId = categoryId,
                                Year = $"{year:0000}"
                            });

                    long entryBudgetValue = 0L;

                    var budgetList = (await budgetEntriesTask.ConfigureAwait(false)).ToDictionary(be => new DateTime(be.Month.Year, be.Month.Month, 1), Basic.Identity);

                    var outflowList = (await outflowTask.ConfigureAwait(false)).ToDictionary(or => new DateTime(or.Month.Year, or.Month.Month, 1), or => or.Sum);

                    var previousMonths = budgetList
                        .Keys
                        .Where(dt => dt.Year < year)
                        .Concat(outflowList.Keys.Where(dt => dt.Year < year))
                        .Distinct()
                        .OrderBy(Basic.Identity).ToList();

                    var previous = previousMonths
                        .Select(dt =>
                        {
                            (long Budget, long Outflow) ret = (0L, 0L);
                            if (budgetList.ContainsKey(dt))
                                ret.Budget = budgetList[dt].Budget;
                            if (outflowList.ContainsKey(dt))
                                ret.Outflow = outflowList[dt];
                            return ret;
                        });

                    long lastBalanceValue = 0L;

                    foreach (var result in previous)
                    {
                        lastBalanceValue = entryBudgetValue + result.Budget + result.Outflow;
                        entryBudgetValue = Math.Max(0L, lastBalanceValue);
                    }
                        
                    DateTime? youngestPreviousMonth = previousMonths.Any() ? previousMonths.Last() : (DateTime?)null;

                    entryBudgetValuePerCategoryId[categoryId] = 
                        lastBalanceValue < 0 && (youngestPreviousMonth is null || !(youngestPreviousMonth.Value.Year == year - 1 && youngestPreviousMonth.Value.Month == 12)) 
                            ? 0 
                            : lastBalanceValue;

                    initialOverspentInPreviousMonth = entryBudgetValuePerCategoryId.Values.Where(s => s < 0).Sum();

                    var firstBalance = entryBudgetValuePerCategoryId.Values.Where(s => s > 0).Sum();

                    initialNotBudgetedOrOverbudgeted =
                        await connection.QueryFirstAsync<long?>(NotBudgetedOrOverbudgetedQuery,
                            new
                            {
                                Year = $"{year:0000}"
                            }).ConfigureAwait(false) ?? 0L;

                    foreach (var incomeCategory in incomeCategories)
                    {
                        initialNotBudgetedOrOverbudgeted += await connection.QueryFirstAsync<long?>(IncomeForCategoryUntilMonthQuery(incomeCategory.MonthOffset), new
                        {
                            CategoryId = incomeCategory.Id,
                            Year = $"{year:0000}"
                        }).ConfigureAwait(false) ?? 0L;
                    }

                    initialNotBudgetedOrOverbudgeted -= firstBalance;

                    initialNotBudgetedOrOverbudgeted -= initialOverspentInPreviousMonth;

                    initialNotBudgetedOrOverbudgeted += await connection.QueryFirstAsync<long?>(DanglingTransferUntilMonthQuery, new
                    {
                        Year = $"{year:0000}"
                    }).ConfigureAwait(false) ?? 0L;
                }

                foreach (var categoryId in categoryIds)
                {
                    var budgetEntriesTask = connection
                        .QueryAsync<BudgetEntry>(
                            BudgetThisYearQuery,
                            new
                            {
                                CategoryId = categoryId,
                                Year = $"{year:0000}"
                            });

                    var outflowTask = connection
                        .QueryAsync<OutflowResponse>(
                            OutflowThisYearQuery,
                            new
                            {
                                CategoryId = categoryId,
                                Year = $"{year:0000}"
                            });

                    long entryBudgetValue = Math.Max(0L, entryBudgetValuePerCategoryId[categoryId]);

                    var budgetList = (await budgetEntriesTask.ConfigureAwait(false)).ToDictionary(be => new DateTime(be.Month.Year, be.Month.Month, 1), Basic.Identity);

                    var outflowList = (await outflowTask.ConfigureAwait(false)).ToDictionary(or => new DateTime(or.Month.Year, or.Month.Month, 1), or => or.Sum);
                    foreach (var month in monthRange)
                    {
                        long id = -1;
                        long budgeted = 0L;
                        if (budgetList.ContainsKey(month))
                        {
                            var budgetEntry = budgetList[month];
                            budgeted = budgetEntry.Budget;
                            id = budgetEntry.Id;
                        }
                        long outflow = 0L;
                        if (outflowList.ContainsKey(month))
                            outflow = outflowList[month];
                        long currentValue = entryBudgetValue + budgeted + outflow;

                        var newBudgetEntry = _budgetEntryFactory();

                        newBudgetEntry.Id = id;
                        newBudgetEntry.CategoryId = categoryId;
                        newBudgetEntry.Budget = budgeted;
                        newBudgetEntry.Month = month;
                        budgetEntries.Add((newBudgetEntry, outflow, currentValue));

                        entryBudgetValue = Math.Max(0L, currentValue);
                    }

                }

                initialOverspentInPreviousMonth = entryBudgetValuePerCategoryId.Values.Where(s => s < 0).Sum();

                var groupedBudgetEntriesPerMonth = budgetEntries
                    .GroupBy(be => be.Entry.Month).ToDictionary(g => g.Key);

                budgetEntriesPerMonth = monthRange
                    .Select(m =>
                        (m: m, groupedBudgetEntriesPerMonth.ContainsKey(m)
                            ? (IList<(IBudgetEntrySql Entry, long Outflow, long Balance)>)groupedBudgetEntriesPerMonth[m].ToList()
                            : new List<(IBudgetEntrySql Entry, long Outflow, long Balance)>()))
                    .ToDictionary(vt => vt.m, vt => vt.Item2);

                incomesPerMonth = monthRange
                    .Select(m =>
                    {
                        long incomeSum = 0;
                        foreach (var incomeCategory in incomeCategories)
                        {
                            incomeSum += connection.QueryFirst<long?>(IncomeForCategoryQuery(incomeCategory.MonthOffset), new
                            {
                                CategoryId = incomeCategory.Id,
                                Year = $"{m.Year:0000}",
                                Month = $"{m.Month:00}"
                            }) ?? 0L;
                        }

                        return (m: m, incomeSum: incomeSum);
                    })
                    .ToDictionary(kvp => kvp.m, kvp => kvp.incomeSum);

                danglingTransfersPerMonth = monthRange
                    .Select(m =>
                        (m: m, DanglingTransferSum: connection.QueryFirstOrDefault<long?>(DanglingTransferQuery, new
                        {
                            Year = $"{m.Year:0000}",
                            Month = $"{m.Month:00}"
                        }) ?? 0L))
                    .ToDictionary(kvp => kvp.m, kvp => kvp.DanglingTransferSum);

                unassignedTransactionsPerMonth = monthRange
                    .Select(m =>
                        (m: m, UnassignedTransactionsSum: connection.QueryFirstOrDefault<long?>(UnassignedTransactionsQuery, new
                        {
                            Year = $"{m.Year:0000}",
                            Month = $"{m.Month:00}"
                        }) ?? 0L))
                    .ToDictionary(kvp => kvp.m, kvp => kvp.UnassignedTransactionsSum);

                transactionScope.Complete();
            }

            return new BudgetBlock
            {
                BudgetEntriesPerMonth = budgetEntriesPerMonth,
                InitialNotBudgetedOrOverbudgeted = initialNotBudgetedOrOverbudgeted,
                InitialOverspentInPreviousMonth = initialOverspentInPreviousMonth,
                IncomesPerMonth = incomesPerMonth,
                DanglingTransfersPerMonth = danglingTransfersPerMonth,
                UnassignedTransactionsPerMonth = unassignedTransactionsPerMonth
            };
        }
    }

    public class BudgetBlock
    {
        public IDictionary<DateTime, IList<(IBudgetEntrySql Entry, long Outflow, long Balance)>> BudgetEntriesPerMonth
        {
            get;
            set;
        }

        public long InitialNotBudgetedOrOverbudgeted
        {
            get;
            set;
        }

        public long InitialOverspentInPreviousMonth
        {
            get;
            set;
        }

        public IDictionary<DateTime, long> IncomesPerMonth {
            get;
            set;
        }

        public IDictionary<DateTime, long> DanglingTransfersPerMonth {
            get;
            set;
        }

        public IDictionary<DateTime, long> UnassignedTransactionsPerMonth {
            get;
            set;
        }
    }
}
