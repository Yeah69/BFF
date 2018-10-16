using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using BFF.Core;
using BFF.Core.Extensions;
using BFF.Persistence.Models;
using BFF.Persistence.ORM.Interfaces;
using Dapper;

namespace BFF.Persistence.ORM.Sqlite
{
    internal class DapperBudgetOrm : IBudgetOrm
    {
        private static readonly string NotBudgetedOrOverbudgetedQuery =
            $@"SELECT Total(Sum) as Sum FROM
    (
        SELECT Total({nameof(AccountDto.StartingBalance)}) AS Sum FROM {nameof(AccountDto)}s
        WHERE strftime('%Y', {nameof(AccountDto.StartingDate)}) < @Year
        UNION ALL
        SELECT Total({nameof(TransDto.Sum)}) AS Sum FROM {nameof(TransDto)}s
        WHERE {nameof(TransDto.Type)} = '{nameof(TransType.Transfer)}' AND {nameof(TransDto.CategoryId)} IS NOT NULL AND strftime('%Y', {nameof(TransDto.Date)}) < @Year
        UNION ALL
        SELECT - Total({nameof(TransDto.Sum)}) AS Sum FROM {nameof(TransDto)}s
        WHERE {nameof(TransDto.Type)} = '{nameof(TransType.Transfer)}' AND {nameof(TransDto.PayeeId)} IS NOT NULL AND strftime('%Y', {nameof(TransDto.Date)}) < @Year
        UNION ALL
        SELECT Total({nameof(TransDto.Sum)})
        FROM {nameof(TransDto)}s
        INNER JOIN {nameof(CategoryDto)}s ON {nameof(TransDto.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(TransDto.CategoryId)} == {nameof(CategoryDto)}s.{nameof(CategoryDto.Id)} AND {nameof(CategoryDto)}s.{nameof(CategoryDto.IsIncomeRelevant)} == 0
        WHERE strftime('%Y', {nameof(TransDto.Date)}) < @Year
        UNION ALL
        SELECT Total({nameof(SubTransactionDto)}s.{nameof(SubTransactionDto.Sum)})
        FROM {nameof(SubTransactionDto)}s
        INNER JOIN {nameof(CategoryDto)}s ON {nameof(SubTransactionDto)}s.{nameof(SubTransactionDto.CategoryId)} == {nameof(CategoryDto)}s.{nameof(CategoryDto.Id)} AND {nameof(CategoryDto)}s.{nameof(CategoryDto.IsIncomeRelevant)} == 0
        INNER JOIN {nameof(TransDto)}s ON {nameof(SubTransactionDto)}s.{nameof(SubTransactionDto.ParentId)} = {nameof(TransDto)}s.{nameof(TransDto.Id)}
        WHERE strftime('%Y', {nameof(TransDto.Date)}) < @Year
        UNION ALL
        SELECT Total({nameof(TransDto.Sum)}) AS Sum FROM {nameof(TransDto)}s 
        WHERE {nameof(TransDto.Type)} = '{nameof(TransType.Transaction)}' AND {nameof(TransDto.CategoryId)} IS NULL AND strftime('%Y', {nameof(TransDto.Date)}) < @Year
        UNION ALL
        SELECT Total({nameof(SubTransactionDto)}s.{nameof(SubTransactionDto.Sum)}) AS Sum
        FROM {nameof(SubTransactionDto)}s
        INNER JOIN {nameof(TransDto)}s ON {nameof(SubTransactionDto)}s.{nameof(SubTransactionDto.ParentId)} = {nameof(TransDto)}s.{nameof(TransDto.Id)}
        WHERE {nameof(SubTransactionDto)}s.{nameof(SubTransactionDto.CategoryId)} IS NULL AND strftime('%Y', {nameof(TransDto.Date)}) < @Year
    );";

        private static string IncomeForCategoryQuery(int offset) =>
            $@"
    SELECT Total({nameof(TransDto.Sum)})
    FROM {nameof(TransDto)}s
    WHERE {nameof(TransDto.CategoryId)} == @CategoryId AND strftime('%Y', {nameof(TransDto.Date)}{(offset == 0 ? "" : $", 'start of month', '+15 days', '{offset:+0;-0;0} months'")}) == @Year AND strftime('%m', {nameof(TransDto.Date)}{(offset == 0 ? "" : $", 'start of month', '+15 days', '{offset:+0;-0;0} months'")}) == @Month;";

        private static string IncomeForCategoryUntilMonthQuery(int offset) =>
            $@"
    SELECT Total({nameof(TransDto.Sum)})
    FROM {nameof(TransDto)}s
    WHERE {nameof(TransDto.CategoryId)} == @CategoryId AND strftime('%Y', {nameof(TransDto.Date)}{(offset == 0 ? "" : $", 'start of month', '+15 days', '{offset:+0;-0;0} months'")}) < @Year;";

        private static string DanglingTransferQuery =>
            $@"
    SELECT (SELECT Total({nameof(TransDto.Sum)}) 
        FROM {nameof(TransDto)}s 
        WHERE {nameof(TransDto.Type)} = '{TransType.Transfer}' AND {nameof(TransDto.PayeeId)} IS NULL AND strftime('%Y', {nameof(TransDto.Date)}) == @Year AND strftime('%m', {nameof(TransDto.Date)}) == @Month)
    - (SELECT Total({nameof(TransDto.Sum)})
        FROM {nameof(TransDto)}s 
        WHERE {nameof(TransDto.Type)} = '{TransType.Transfer}' AND {nameof(TransDto.CategoryId)} IS NULL AND strftime('%Y', {nameof(TransDto.Date)}) == @Year AND strftime('%m', {nameof(TransDto.Date)}) == @Month)";

        private static string DanglingTransferUntilMonthQuery =>
            $@"
    SELECT (SELECT Total({nameof(TransDto.Sum)}) 
        FROM {nameof(TransDto)}s 
        WHERE {nameof(TransDto.Type)} = '{TransType.Transfer}' AND {nameof(TransDto.PayeeId)} IS NULL AND strftime('%Y', {nameof(TransDto.Date)}) < @Year)
    - (SELECT Total({nameof(TransDto.Sum)}) 
        FROM {nameof(TransDto)}s 
        WHERE {nameof(TransDto.Type)} = '{TransType.Transfer}' AND {nameof(TransDto.CategoryId)} IS NULL AND strftime('%Y', {nameof(TransDto.Date)}) < @Year)";

        private class OutflowResponse
        {
            public DateTime Month { get; set; }
            public long Sum { get; set; }
        }

        private static readonly string BudgetQuery =
            $@"SELECT {nameof(BudgetEntryDto.Id)}, {nameof(BudgetEntryDto.CategoryId)}, {nameof(BudgetEntryDto.Month)}, {nameof(BudgetEntryDto.Budget)}
  FROM {nameof(BudgetEntryDto)}s
  WHERE {nameof(BudgetEntryDto.CategoryId)} = @CategoryId AND strftime('%Y', {nameof(BudgetEntryDto.Month)}) <= @Year
  ORDER BY {nameof(BudgetEntryDto.Month)};";

        private static readonly string BudgetThisYearQuery =
            $@"SELECT {nameof(BudgetEntryDto.Id)}, {nameof(BudgetEntryDto.CategoryId)}, {nameof(BudgetEntryDto.Month)}, {nameof(BudgetEntryDto.Budget)}
  FROM {nameof(BudgetEntryDto)}s
  WHERE {nameof(BudgetEntryDto.CategoryId)} = @CategoryId AND strftime('%Y', {nameof(BudgetEntryDto.Month)}) = @Year
  ORDER BY {nameof(BudgetEntryDto.Month)};";

        private static readonly string OutflowQuery =
            $@"SELECT Date as Month, Total(Sum) as Sum FROM
(
  SELECT strftime('%Y', {nameof(TransDto.Date)}) || '-' || strftime('%m', {nameof(TransDto.Date)}) || '-' || '01' AS Date, {nameof(TransDto.Sum)} FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(TransDto.CategoryId)} = @CategoryId AND strftime('%Y', Date) <= @Year
  UNION ALL
  SELECT strftime('%Y', {nameof(TransDto.Date)}) || '-' || strftime('%m', {nameof(TransDto.Date)}) || '-'  || '01' AS Date, subs.{nameof(SubTransactionDto.Sum)} FROM
    (SELECT {nameof(SubTransactionDto.ParentId)}, {nameof(SubTransactionDto)}s.{nameof(SubTransactionDto.Sum)} FROM {nameof(SubTransactionDto)}s WHERE {nameof(SubTransactionDto)}s.{nameof(SubTransactionDto.CategoryId)} = @CategoryId) subs
      INNER JOIN {nameof(TransDto)}s ON subs.{nameof(SubTransactionDto.ParentId)} = {nameof(TransDto)}s.{nameof(TransDto.Id)}
  WHERE strftime('%Y', Date) <= @Year
)
  GROUP BY Date
  ORDER BY Date;";

        private static readonly string OutflowThisYearQuery =
            $@"SELECT Date as Month, Total(Sum) as Sum FROM
(
  SELECT strftime('%Y', {nameof(TransDto.Date)}) || '-' || strftime('%m', {nameof(TransDto.Date)}) || '-' || '01' AS Date, {nameof(TransDto.Sum)} FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(TransDto.CategoryId)} = @CategoryId AND strftime('%Y', Date) = @Year
  UNION ALL
  SELECT strftime('%Y', {nameof(TransDto.Date)}) || '-' || strftime('%m', {nameof(TransDto.Date)}) || '-'  || '01' AS Date, subs.{nameof(SubTransactionDto.Sum)} FROM
    (SELECT {nameof(SubTransactionDto.ParentId)}, {nameof(SubTransactionDto)}s.{nameof(SubTransactionDto.Sum)} FROM {nameof(SubTransactionDto)}s WHERE {nameof(SubTransactionDto)}s.{nameof(SubTransactionDto.CategoryId)} = @CategoryId) subs
      INNER JOIN {nameof(TransDto)}s ON subs.{nameof(SubTransactionDto.ParentId)} = {nameof(TransDto)}s.{nameof(TransDto.Id)}
  WHERE strftime('%Y', Date) = @Year
)
  GROUP BY Date
  ORDER BY Date;";

        private static readonly string UnassignedTransactionsQuery = $@"
SELECT Total(Sum) FROM
(
    SELECT {nameof(TransDto.Sum)} FROM {nameof(TransDto)}s WHERE {nameof(TransDto.Type)} = '{nameof(TransType.Transaction)}' AND {nameof(TransDto.CategoryId)} IS NULL AND strftime('%Y', {nameof(TransDto.Date)}) = @Year AND strftime('%m', {nameof(TransDto.Date)}) = @Month
    UNION ALL
    SELECT {nameof(SubTransactionDto)}s.{nameof(SubTransactionDto.Sum)}
    FROM {nameof(SubTransactionDto)}s
    INNER JOIN {nameof(TransDto)}s ON {nameof(SubTransactionDto)}s.{nameof(SubTransactionDto.ParentId)} = {nameof(TransDto)}s.{nameof(TransDto.Id)}
    WHERE {nameof(SubTransactionDto)}s.{nameof(SubTransactionDto.CategoryId)} IS NULL AND strftime('%Y', {nameof(TransDto.Date)}) = @Year AND strftime('%m', {nameof(TransDto.Date)}) = @Month
)";

        private readonly IProvideConnection _provideConnection;

        public DapperBudgetOrm(
            IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public async Task<BudgetBlock> FindAsync(int year, long[] categoryIds, (long Id, int MonthOffset)[] incomeCategories)
        {
            IDictionary<DateTime, IList<(BudgetEntryDto Entry, long Outflow, long Balance)>> budgetEntriesPerMonth;
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
                var budgetEntries = new List<(BudgetEntryDto Entry, long Outflow, long Balance)>();

                IDictionary<long, long> entryBudgetValuePerCategoryId = new Dictionary<long, long>();
                
                foreach (var categoryId in categoryIds)
                {
                    var budgetEntriesTask = connection
                        .QueryAsync<BudgetEntryDto>(
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

                    var budgetList = (await budgetEntriesTask.ConfigureAwait(false)).ToDictionary(be => new DateTime(be.Month.Year, be.Month.Month, 1), be => be);

                    var outflowList = (await outflowTask.ConfigureAwait(false)).ToDictionary(or => new DateTime(or.Month.Year, or.Month.Month, 1), or => or.Sum);

                    var previousMonths = budgetList
                        .Keys
                        .Where(dt => dt.Year < year)
                        .Concat(outflowList.Keys.Where(dt => dt.Year < year))
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
                        .QueryAsync<BudgetEntryDto>(
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
                        budgetEntries.Add((new BudgetEntryDto
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
                        (m: m, groupedBudgetEntriesPerMonth.ContainsKey(m)
                            ? (IList<(BudgetEntryDto Entry, long Outflow, long Balance)>)groupedBudgetEntriesPerMonth[m].ToList()
                            : new List<(BudgetEntryDto Entry, long Outflow, long Balance)>()))
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
        public IDictionary<DateTime, IList<(BudgetEntryDto Entry, long Outflow, long Balance)>> BudgetEntriesPerMonth
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
