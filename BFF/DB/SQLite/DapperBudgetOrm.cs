using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using BFF.DB.PersistenceModels;
using BFF.Helper;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native.Structure;
using Dapper;
using BudgetEntry = BFF.DB.PersistenceModels.BudgetEntry;
using SubTransaction = BFF.DB.PersistenceModels.SubTransaction;

namespace BFF.DB.SQLite
{
    class DapperBudgetOrm : IBudgetOrm
    {
        private static readonly string NotBudgetedOrOverbudgetedQuery =
            $@"SELECT Total(Sum) as Sum FROM
    (
        SELECT Total({nameof(Account.StartingBalance)}) AS Sum FROM {nameof(Account)}s
        WHERE strftime('%Y', {nameof(Account.StartingDate)}) < @Year OR strftime('%Y', {nameof(Account.StartingDate)}) == @Year AND strftime('%m', {nameof(Account.StartingDate)}) <= @Month
        UNION ALL
        SELECT Total({nameof(Trans.Sum)}) AS Sum FROM {nameof(Trans)}s
        WHERE {nameof(Trans.Type)} = '{nameof(TransType.Transfer)}' AND {nameof(Trans.CategoryId)} IS NOT NULL AND strftime('%Y', {nameof(Trans.Date)}) < @Year OR strftime('%Y', {nameof(Trans.Date)}) == @Year AND strftime('%m', {nameof(Trans.Date)}) <= @Month
        UNION ALL
        SELECT - Total({nameof(Trans.Sum)}) AS Sum FROM {nameof(Trans)}s
        WHERE {nameof(Trans.Type)} = '{nameof(TransType.Transfer)}' AND {nameof(Trans.PayeeId)} IS NOT NULL AND strftime('%Y', {nameof(Trans.Date)}) < @Year OR strftime('%Y', {nameof(Trans.Date)}) == @Year AND strftime('%m', {nameof(Trans.Date)}) <= @Month
        UNION ALL
        SELECT Total({nameof(Trans.Sum)})
        FROM {nameof(Trans)}s
        INNER JOIN {nameof(Category)}s ON {nameof(Trans.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(Trans.CategoryId)} == {nameof(Category)}s.{nameof(Category.Id)} AND {nameof(Category)}s.{nameof(Category.IsIncomeRelevant)} == 0
        WHERE strftime('%Y', {nameof(Trans.Date)}) < @Year
            OR strftime('%m', {nameof(Trans.Date)}) <= @Month
            AND strftime('%Y', {nameof(Trans.Date)}) = @Year
        UNION ALL
        SELECT Total({nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)})
        FROM {nameof(SubTransaction)}s
        INNER JOIN {nameof(Category)}s ON {nameof(SubTransaction)}s.{nameof(SubTransaction.CategoryId)} == {nameof(Category)}s.{nameof(Category.Id)} AND {nameof(Category)}s.{nameof(Category.IsIncomeRelevant)} == 0
        INNER JOIN {nameof(Trans)}s ON {nameof(SubTransaction)}s.{nameof(SubTransaction.ParentId)} = {nameof(Trans)}s.{nameof(Trans.Id)}
        WHERE strftime('%Y', {nameof(Trans.Date)}) < @Year
            OR strftime('%m', {nameof(Trans.Date)}) <= @Month
            AND strftime('%Y', {nameof(Trans.Date)}) = @Year
        UNION ALL
        SELECT Total({nameof(Trans.Sum)}) AS Sum FROM {nameof(Trans)}s 
        WHERE {nameof(Trans.Type)} = '{nameof(TransType.Transaction)}' AND {nameof(Trans.CategoryId)} IS NULL AND (strftime('%Y', {nameof(Trans.Date)}) < @Year
            OR strftime('%m', {nameof(Trans.Date)}) <= @Month
            AND strftime('%Y', {nameof(Trans.Date)}) = @Year)
        UNION ALL
        SELECT Total({nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)}) AS Sum
        FROM {nameof(SubTransaction)}s
        INNER JOIN {nameof(Trans)}s ON {nameof(SubTransaction)}s.{nameof(SubTransaction.ParentId)} = {nameof(Trans)}s.{nameof(Trans.Id)}
        WHERE {nameof(SubTransaction)}s.{nameof(SubTransaction.CategoryId)} IS NULL AND (strftime('%Y', {nameof(Trans.Date)}) < @Year
            OR strftime('%m', {nameof(Trans.Date)}) <= @Month
            AND strftime('%Y', {nameof(Trans.Date)}) = @Year)
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
    WHERE {nameof(Trans.CategoryId)} == @CategoryId AND (strftime('%Y', {nameof(Trans.Date)}{(offset == 0 ? "" : $", 'start of month', '+15 days', '{offset:+0;-0;0} months'")}) < @Year OR strftime('%Y', {nameof(Trans.Date)}{(offset == 0 ? "" : $", 'start of month', '+15 days', '{offset:+0;-0;0} months'")}) == @Year AND strftime('%m', {nameof(Trans.Date)}{(offset == 0 ? "" : $", 'start of month', '+15 days', '{offset:+0;-0;0} months'")}) <= @Month);";

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
        WHERE {nameof(Trans.Type)} = '{TransType.Transfer}' AND {nameof(Trans.PayeeId)} IS NULL AND (strftime('%Y', {nameof(Trans.Date)}) < @Year OR strftime('%Y', {nameof(Trans.Date)}) == @Year AND strftime('%m', {nameof(Trans.Date)}) <= @Month))
    - (SELECT Total({nameof(Trans.Sum)}) 
        FROM {nameof(Trans)}s 
        WHERE {nameof(Trans.Type)} = '{TransType.Transfer}' AND {nameof(Trans.CategoryId)} IS NULL AND (strftime('%Y', {nameof(Trans.Date)}) < @Year OR strftime('%Y', {nameof(Trans.Date)}) == @Year AND strftime('%m', {nameof(Trans.Date)}) <= @Month))";

        private class OutflowResponse
        {
            public DateTime Month { get; set; }
            public long Sum { get; set; }
        }

        private static readonly string BudgetQuery =
            $@"SELECT {nameof(BudgetEntry.Id)}, {nameof(BudgetEntry.CategoryId)}, {nameof(BudgetEntry.Month)}, {nameof(BudgetEntry.Budget)}
  FROM {nameof(BudgetEntry)}s
  WHERE {nameof(BudgetEntry.CategoryId)} = @CategoryId AND (strftime('%Y', {nameof(BudgetEntry.Month)}) < @Year OR strftime('%Y', {nameof(BudgetEntry.Month)}) = @Year AND strftime('%m', {nameof(BudgetEntry.Month)}) <= @Month)
  ORDER BY {nameof(BudgetEntry.Month)};";

        private static readonly string BudgetFirstHalfQuery =
            $@"SELECT {nameof(BudgetEntry.Id)}, {nameof(BudgetEntry.CategoryId)}, {nameof(BudgetEntry.Month)}, {nameof(BudgetEntry.Budget)}
  FROM {nameof(BudgetEntry)}s
  WHERE {nameof(BudgetEntry.CategoryId)} = @CategoryId AND (strftime('%Y', {nameof(BudgetEntry.Month)}) = @Year AND strftime('%m', {nameof(BudgetEntry.Month)}) <= '06')
  ORDER BY {nameof(BudgetEntry.Month)};";

        private static readonly string BudgetSecondHalfQuery =
            $@"SELECT {nameof(BudgetEntry.Id)}, {nameof(BudgetEntry.CategoryId)}, {nameof(BudgetEntry.Month)}, {nameof(BudgetEntry.Budget)}
  FROM {nameof(BudgetEntry)}s
  WHERE {nameof(BudgetEntry.CategoryId)} = @CategoryId AND (strftime('%Y', {nameof(BudgetEntry.Month)}) = @Year AND strftime('%m', {nameof(BudgetEntry.Month)}) >= '07')
  ORDER BY {nameof(BudgetEntry.Month)};";

        private static readonly string OutflowQuery =
            $@"SELECT Date as Month, Total(Sum) as Sum FROM
(
  SELECT strftime('%Y', {nameof(Trans.Date)}) || '-' || strftime('%m', {nameof(Trans.Date)}) || '-' || '01' AS Date, {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(Trans.CategoryId)} = @CategoryId AND (strftime('%Y', {nameof(Trans.Date)}) < @Year OR strftime('%Y', {nameof(Trans.Date)}) = @Year AND strftime('%m', {nameof(Trans.Date)}) <= @Month)
  UNION ALL
  SELECT strftime('%Y', {nameof(Trans.Date)}) || '-' || strftime('%m', {nameof(Trans.Date)}) || '-'  || '01' AS Date, subs.{nameof(SubTransaction.Sum)} FROM
    (SELECT {nameof(SubTransaction.ParentId)}, {nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)} FROM {nameof(SubTransaction)}s WHERE {nameof(SubTransaction)}s.{nameof(SubTransaction.CategoryId)} = @CategoryId) subs
      INNER JOIN {nameof(Trans)}s ON subs.{nameof(SubTransaction.ParentId)} = {nameof(Trans)}s.{nameof(Trans.Id)}
  WHERE strftime('%Y', Date) < @Year OR strftime('%Y', Date) = @Year AND strftime('%m', Date) <= @Month
)
  GROUP BY Date
  ORDER BY Date;";

        private static readonly string OutflowFirstHalfQuery =
            $@"SELECT Date as Month, Total(Sum) as Sum FROM
(
  SELECT strftime('%Y', {nameof(Trans.Date)}) || '-' || strftime('%m', {nameof(Trans.Date)}) || '-' || '01' AS Date, {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(Trans.CategoryId)} = @CategoryId AND (strftime('%Y', Date) = @Year AND strftime('%m', Date) <= '06')
  UNION ALL
  SELECT strftime('%Y', {nameof(Trans.Date)}) || '-' || strftime('%m', {nameof(Trans.Date)}) || '-'  || '01' AS Date, subs.{nameof(SubTransaction.Sum)} FROM
    (SELECT {nameof(SubTransaction.ParentId)}, {nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)} FROM {nameof(SubTransaction)}s WHERE {nameof(SubTransaction)}s.{nameof(SubTransaction.CategoryId)} = @CategoryId) subs
      INNER JOIN {nameof(Trans)}s ON subs.{nameof(SubTransaction.ParentId)} = {nameof(Trans)}s.{nameof(Trans.Id)}
  WHERE strftime('%Y', Date) = @Year AND strftime('%m', Date) <= '06'
)
  GROUP BY Date
  ORDER BY Date;";

        private static readonly string OutflowSecondHalfQuery =
            $@"SELECT Date as Month, Total(Sum) as Sum FROM
(
  SELECT strftime('%Y', {nameof(Trans.Date)}) || '-' || strftime('%m', {nameof(Trans.Date)}) || '-' || '01' AS Date, {nameof(Trans.Sum)} FROM {nameof(Trans)}s WHERE {nameof(Trans.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(Trans.CategoryId)} = @CategoryId AND (strftime('%Y', Date) = @Year AND strftime('%m', Date) >= '07')
  UNION ALL
  SELECT strftime('%Y', {nameof(Trans.Date)}) || '-' || strftime('%m', {nameof(Trans.Date)}) || '-'  || '01' AS Date, subs.{nameof(SubTransaction.Sum)} FROM
    (SELECT {nameof(SubTransaction.ParentId)}, {nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)} FROM {nameof(SubTransaction)}s WHERE {nameof(SubTransaction)}s.{nameof(SubTransaction.CategoryId)} = @CategoryId) subs
      INNER JOIN {nameof(Trans)}s ON subs.{nameof(SubTransaction.ParentId)} = {nameof(Trans)}s.{nameof(Trans.Id)}
  WHERE strftime('%Y', Date) = @Year AND strftime('%m', Date) >= '07'
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

        private readonly IProvideConnection _provideConnection;
        private readonly IBudgetOverviewCachingOperations _budgetOverviewCachingOperations;

        public DapperBudgetOrm(
            IProvideConnection provideConnection,
            IBudgetOverviewCachingOperations budgetOverviewCachingOperations)
        {
            _provideConnection = provideConnection;
            _budgetOverviewCachingOperations = budgetOverviewCachingOperations;
        }

        public async Task<BudgetBlock> FindAsync(DateTime fromMonth, DateTime toMonth, long[] categoryIds, (long Id, int MonthOffset)[] incomeCategories)
        {
            IDictionary<DateTime, IList<(BudgetEntry Entry, long Outflow, long Balance)>> budgetEntriesPerMonth;
            long initialNotBudgetedOrOverbudgeted;
            long initialOverspentInPreviousMonth;
            IDictionary<DateTime, long> incomesPerMonth;
            IDictionary<DateTime, long> danglingTransfersPerMonth;
            IDictionary<DateTime, long> unassignedTransactionsPerMonth;
            DateTime previousMonth = fromMonth.PreviousMonth();

            IList<DateTime> monthRange = new List<DateTime>();

            var currentMonth = new DateTime(fromMonth.Year, fromMonth.Month, 1);

            do
            {
                monthRange.Add(currentMonth);
                currentMonth = currentMonth.NextMonth();

            } while (currentMonth.Year < toMonth.Year || currentMonth.Year == toMonth.Year && currentMonth.Month <= toMonth.Month);

            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();

                var budgetEntries = new List<(BudgetEntry Entry, long Outflow, long Balance)>();

                IDictionary<long, long> entryBudgetValuePerCategoryId = new Dictionary<long, long>();

                if (_budgetOverviewCachingOperations.TryGetValue((fromMonth.Year, fromMonth.Month == 1 ? 1 : 2), out var tuple))
                {
                    (entryBudgetValuePerCategoryId, initialNotBudgetedOrOverbudgeted) = tuple;
                }
                else
                {
                    foreach (var categoryId in categoryIds)
                    {
                        var budgetEntriesTask = connection
                            .QueryAsync<BudgetEntry>(
                                BudgetQuery,
                                new
                                {
                                    CategoryId = categoryId,
                                    Year = $"{toMonth.Year:0000}",
                                    Month = $"{toMonth.Month:00}"
                                });

                        var outflowTask = connection
                            .QueryAsync<OutflowResponse>(
                                OutflowQuery,
                                new
                                {
                                    CategoryId = categoryId,
                                    Year = $"{toMonth.Year:0000}",
                                    Month = $"{toMonth.Month:00}"
                                });

                        long entryBudgetValue = 0L;

                        var budgetList = (await budgetEntriesTask.ConfigureAwait(false)).ToDictionary(be => new DateTime(be.Month.Year, be.Month.Month, 1), be => be);

                        var outflowList = (await outflowTask.ConfigureAwait(false)).ToDictionary(or => new DateTime(or.Month.Year, or.Month.Month, 1), or => or.Sum);

                        var previousMonths = budgetList
                            .Keys
                            .Where(dt => dt < fromMonth)
                            .Concat(outflowList.Keys.Where(dt => dt < fromMonth))
                            .Distinct()
                            .OrderBy(dt => dt).ToList();

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

                        entryBudgetValuePerCategoryId[categoryId] = 
                            lastBalanceValue < 0 && previousMonths.Last() != previousMonth 
                                ? 0 
                                : lastBalanceValue;
                    }

                    initialOverspentInPreviousMonth = entryBudgetValuePerCategoryId.Values.Where(s => s < 0).Sum();

                    var firstBalance = entryBudgetValuePerCategoryId.Values.Where(s => s > 0).Sum();

                    initialNotBudgetedOrOverbudgeted =
                        await connection.QueryFirstAsync<long?>(NotBudgetedOrOverbudgetedQuery,
                            new
                            {
                                Year = $"{previousMonth.Year:0000}",
                                Month = $"{previousMonth.Month:00}"
                            }).ConfigureAwait(false) ?? 0L;

                    foreach (var incomeCategory in incomeCategories)
                    {
                        initialNotBudgetedOrOverbudgeted += await connection.QueryFirstAsync<long?>(IncomeForCategoryUntilMonthQuery(incomeCategory.MonthOffset), new
                        {
                            CategoryId = incomeCategory.Id,
                            Year = $"{previousMonth.Year:0000}",
                            Month = $"{previousMonth.Month:00}"
                        }).ConfigureAwait(false) ?? 0L;
                    }

                    initialNotBudgetedOrOverbudgeted -= firstBalance;

                    initialNotBudgetedOrOverbudgeted -= initialOverspentInPreviousMonth;

                    initialNotBudgetedOrOverbudgeted += await connection.QueryFirstAsync<long?>(DanglingTransferUntilMonthQuery, new
                    {
                        Year = $"{previousMonth.Year:0000}",
                        Month = $"{previousMonth.Month:00}"
                    }).ConfigureAwait(false) ?? 0L;

                    _budgetOverviewCachingOperations.Add((fromMonth.Year, fromMonth.Month == 1 ? 1 : 2),
                        (entryBudgetValuePerCategoryId, initialNotBudgetedOrOverbudgeted));
                }

                foreach (var categoryId in categoryIds)
                {
                    var budgetEntriesTask = connection
                        .QueryAsync<BudgetEntry>(
                            toMonth.Month <= 6 ? BudgetFirstHalfQuery : BudgetSecondHalfQuery,
                            new
                            {
                                CategoryId = categoryId,
                                Year = $"{toMonth.Year:0000}"
                            });

                    var outflowTask = connection
                        .QueryAsync<OutflowResponse>(
                            toMonth.Month <= 6 ? OutflowFirstHalfQuery : OutflowSecondHalfQuery,
                            new
                            {
                                CategoryId = categoryId,
                                Year = $"{toMonth.Year:0000}"
                            });

                    long entryBudgetValue = Math.Max(0L, entryBudgetValuePerCategoryId[categoryId]);

                    var budgetList = (await budgetEntriesTask.ConfigureAwait(false)).ToDictionary(be => new DateTime(be.Month.Year, be.Month.Month, 1), be => be);

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
                        budgetEntries.Add((new BudgetEntry
                        {
                            Id = id,
                            CategoryId = categoryId,
                            Budget = budgeted,
                            Month = month
                        }, outflow, currentValue));

                        entryBudgetValue = Math.Max(0L, currentValue);
                    }

                }

                initialOverspentInPreviousMonth = entryBudgetValuePerCategoryId.Values.Where(s => s < 0).Sum();

                var groupedBudgetEntriesPerMonth = budgetEntries
                    .GroupBy(be => be.Entry.Month).ToDictionary(g => g.Key);

                budgetEntriesPerMonth = monthRange
                    .Select(m =>
                        (m, groupedBudgetEntriesPerMonth.ContainsKey(m)
                            ? (IList<(BudgetEntry Entry, long Outflow, long Balance)>)groupedBudgetEntriesPerMonth[m].ToList()
                            : new List<(BudgetEntry Entry, long Outflow, long Balance)>()))
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

                        return (m, incomeSum);
                    })
                    .ToDictionary(kvp => kvp.m, kvp => kvp.incomeSum);

                danglingTransfersPerMonth = monthRange
                    .Select(m =>
                        (m, DanglingTransferSum: connection.QueryFirstOrDefault<long?>(DanglingTransferQuery, new
                        {
                            Year = $"{m.Year:0000}",
                            Month = $"{m.Month:00}"
                        }) ?? 0L))
                    .ToDictionary(kvp => kvp.m, kvp => kvp.DanglingTransferSum);

                unassignedTransactionsPerMonth = monthRange
                    .Select(m =>
                        (m, UnassignedTransactionsSum: connection.QueryFirstOrDefault<long?>(UnassignedTransactionsQuery, new
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
        public IDictionary<DateTime, IList<(BudgetEntry Entry, long Outflow, long Balance)>> BudgetEntriesPerMonth
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
