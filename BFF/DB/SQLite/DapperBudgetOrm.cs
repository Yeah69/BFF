using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using BFF.DB.PersistenceModels;
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
  WHERE {nameof(BudgetEntry.CategoryId)} = @CategoryId AND (strftime('%Y', {nameof(BudgetEntry.Month)}) < @Year OR strftime('%Y', {nameof(BudgetEntry.Month)} = @Year AND strftime('%m', {nameof(BudgetEntry.Month)} <= @Month)))
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

        public DapperBudgetOrm(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public async Task<BudgetBlock> FindAsync(DateTime fromMonth, DateTime toMonth, long[] categoryIds, (long Id, int MonthOffset)[] incomeCategories)
        {
            IDictionary<DateTime, IList<(BudgetEntry Entry, long Outflow, long Balance)>> budgetEntriesPerMonth;
            long initialNotBudgetedOrOverbudgeted;
            IDictionary<DateTime, long> incomesPerMonth;
            IDictionary<DateTime, long> danglingTransfersPerMonth;
            IDictionary<DateTime, long> unassignedTransactionsPerMonth;

            IList<DateTime> monthRange = new List<DateTime>();

            var currentMonth = new DateTime(fromMonth.Year, fromMonth.Month, 01);

            do
            {
                monthRange.Add(currentMonth);
                currentMonth = new DateTime(
                    currentMonth.Month == 12 ? currentMonth.Year + 1 : currentMonth.Year,
                    currentMonth.Month == 12 ? 1 : currentMonth.Month + 1,
                    1);

            } while (currentMonth.Year < toMonth.Year || currentMonth.Year == toMonth.Year && currentMonth.Month <= toMonth.Month);

            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();

                var budgetEntries = new List<(BudgetEntry Entry, long Outflow, long Balance)>();

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

                    var previous = budgetList
                        .Keys
                        .Where(dt => dt < fromMonth)
                        .Concat(outflowList.Keys.Where(dt => dt < fromMonth))
                        .Distinct()
                        .OrderBy(dt => dt)
                        .Select(dt =>
                        {
                            (long Budget, long Outflow) ret = (0L, 0L);
                            if (budgetList.ContainsKey(dt))
                                ret.Budget = budgetList[dt].Budget;
                            if (outflowList.ContainsKey(dt))
                                ret.Outflow = outflowList[dt];
                            return ret;
                        });

                    foreach (var result in previous)
                    {
                        entryBudgetValue = Math.Max(0L, entryBudgetValue + result.Budget + result.Outflow);
                    }

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

                var groupedBudgetEntriesPerMonth = budgetEntries
                    .GroupBy(be => be.Entry.Month).ToDictionary(g => g.Key);

                budgetEntriesPerMonth = monthRange
                    .Select(m =>
                        (m, groupedBudgetEntriesPerMonth.ContainsKey(m)
                            ? (IList<(BudgetEntry Entry, long Outflow, long Balance)>)groupedBudgetEntriesPerMonth[m].ToList()
                            : new List<(BudgetEntry Entry, long Outflow, long Balance)>()))
                    .ToDictionary(vt => vt.m, vt => vt.Item2);

                long firstBalance = 0;
                if (budgetEntriesPerMonth.Any())
                    firstBalance = budgetEntriesPerMonth[monthRange[0]].Where(be => be.Balance > 0).Sum(be => be.Balance);

                initialNotBudgetedOrOverbudgeted =
                    await connection.QueryFirstAsync<long?>(NotBudgetedOrOverbudgetedQuery,
                        new
                        {
                            Year = $"{fromMonth.Year:0000}",
                            Month = $"{fromMonth.Month:00}"
                        }).ConfigureAwait(false) ?? 0L;

                foreach (var incomeCategory in incomeCategories)
                {
                    initialNotBudgetedOrOverbudgeted += await connection.QueryFirstAsync<long?>(IncomeForCategoryUntilMonthQuery(incomeCategory.MonthOffset), new
                    {
                        CategoryId = incomeCategory.Id,
                        Year = $"{fromMonth.Year:0000}",
                        Month = $"{fromMonth.Month:00}"
                    }).ConfigureAwait(false) ?? 0L;
                }

                initialNotBudgetedOrOverbudgeted -= firstBalance;

                if (budgetEntriesPerMonth.Any())
                    initialNotBudgetedOrOverbudgeted -= budgetEntriesPerMonth[monthRange[0]].Where(be => be.Balance < 0).Sum(be => be.Balance);

                initialNotBudgetedOrOverbudgeted += await connection.QueryFirstAsync<long?>(DanglingTransferUntilMonthQuery, new
                {
                    Year = $"{fromMonth.Year:0000}",
                    Month = $"{fromMonth.Month:00}"
                }).ConfigureAwait(false) ?? 0L;

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

        public long InitialNotBudgetedOrOverbudgeted {
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
