using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
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
            $@"SELECT Sum(Sum) as Sum FROM
    (
        SELECT Sum({nameof(Account.StartingBalance)}) AS Sum FROM {nameof(Account)}s
        WHERE strftime('%Y', {nameof(Account.StartingDate)}) < @Year OR strftime('%Y', {nameof(Account.StartingDate)}) == @Year AND strftime('%m', {nameof(Account.StartingDate)}) <= @Month
        UNION ALL
        SELECT {nameof(Trans.Sum)}
        FROM {nameof(Trans)}s
        INNER JOIN {nameof(Category)}s ON {nameof(Trans.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(Trans.CategoryId)} == {nameof(Category)}s.{nameof(Category.Id)} AND {nameof(Category)}s.{nameof(Category.IsIncomeRelevant)} == 0
        WHERE strftime('%Y', {nameof(Trans.Date)}) < @Year
            OR strftime('%m', {nameof(Trans.Date)}) <= @Month
            AND strftime('%Y', {nameof(Trans.Date)}) = @Year
        UNION ALL
        SELECT {nameof(SubTransaction)}s.{nameof(SubTransaction.Sum)}
        FROM {nameof(SubTransaction)}s
        INNER JOIN {nameof(Category)}s ON {nameof(SubTransaction)}s.{nameof(SubTransaction.CategoryId)} == {nameof(Category)}s.{nameof(Category.Id)} AND {nameof(Category)}s.{nameof(Category.IsIncomeRelevant)} == 0
        INNER JOIN {nameof(Trans)}s ON {nameof(SubTransaction)}s.{nameof(SubTransaction.ParentId)} = {nameof(Trans)}s.{nameof(Trans.Id)}
        WHERE strftime('%Y', {nameof(Trans.Date)}) < @Year
            OR strftime('%m', {nameof(Trans.Date)}) <= @Month
            AND strftime('%Y', {nameof(Trans.Date)}) = @Year
    );";

        private static string IncomeForCategoryQuery(int offset) =>
            $@"
    SELECT Sum({nameof(Trans.Sum)})
    FROM {nameof(Trans)}s
    WHERE {nameof(Trans.CategoryId)} == @CategoryId AND strftime('%Y', {nameof(Trans.Date)}{(offset == 0 ? "" : $", 'start of month', '+15 days', '{offset:+0;-0;0} months'")}) == @Year AND strftime('%m', {nameof(Trans.Date)}{(offset == 0 ? "" : $", 'start of month', '+15 days', '{offset:+0;-0;0} months'")}) == @Month;";

        private static string IncomeForCategoryUntilMonthQuery(int offset) =>
            $@"
    SELECT Sum({nameof(Trans.Sum)})
    FROM {nameof(Trans)}s
    WHERE {nameof(Trans.CategoryId)} == @CategoryId AND (strftime('%Y', {nameof(Trans.Date)}{(offset == 0 ? "" : $", 'start of month', '+15 days', '{offset:+0;-0;0} months'")}) < @Year OR strftime('%Y', {nameof(Trans.Date)}{(offset == 0 ? "" : $", 'start of month', '+15 days', '{offset:+0;-0;0} months'")}) == @Year AND strftime('%m', {nameof(Trans.Date)}{(offset == 0 ? "" : $", 'start of month', '+15 days', '{offset:+0;-0;0} months'")}) <= @Month);";
        
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
            $@"SELECT Date as Month, Sum(Sum) as Sum FROM
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

        private readonly IProvideConnection _provideConnection;

        public DapperBudgetOrm(IProvideConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public (IGrouping<DateTime, (BudgetEntry Entry, long Outflow, long Balance)>[] BudgetEntriesPerMonth, long InitialNotBudgetedOrOverbudgeted, IDictionary<DateTime, long> IncomesPerMonth) Find(
            DateTime fromMonth, DateTime toMonth, long[] categoryIds, (long Id, int MonthOffset)[] incomeCategories)
        {
            IGrouping<DateTime, (BudgetEntry Entry, long Outflow, long Balance)>[] budgetEntriesPerMonth;
            long initialNotBudgetedOrOverbudgeted;
            IDictionary<DateTime, long> incomesPerMonth;

            using (TransactionScope transactionScope = new TransactionScope())
            using (DbConnection connection = _provideConnection.Connection)
            {
                connection.Open();

                var budgetEntries = new List<(BudgetEntry Entry, long Outflow, long Balance)>();

                foreach (var categoryId in categoryIds)
                {
                    var budgetList = connection
                        .Query<BudgetEntry>(
                        BudgetQuery,
                        new
                        {
                            CategoryId = categoryId,
                            Year = $"{toMonth.Year:0000}",
                            Month = $"{toMonth.Month:00}"
                        })
                        .ToDictionary(be => new DateTime(be.Month.Year, be.Month.Month, 1), be => be);

                    var outflowList = connection
                        .Query<OutflowResponse>(
                        OutflowQuery,
                        new
                        {
                            CategoryId = categoryId,
                            Year = $"{toMonth.Year:0000}",
                            Month = $"{toMonth.Month:00}"
                        })
                        .ToDictionary(or => new DateTime(or.Month.Year, or.Month.Month, 1), or => or.Sum);

                    long entryBudgetValue = 0L;

                    var previous = budgetList.Keys.Where(dt => dt < fromMonth).Concat(outflowList.Keys.Where(dt => dt < fromMonth)).Distinct().OrderBy(dt => dt).Select(dt =>
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

                    var currentDate = new DateTime(fromMonth.Year, fromMonth.Month, 01);
                    
                    do
                    {
                        long id = -1;
                        long budgeted = 0L;
                        if (budgetList.ContainsKey(currentDate))
                        {
                            var budgetEntry = budgetList[currentDate];
                            budgeted = budgetEntry.Budget;
                            id = budgetEntry.Id;
                        }
                        long outflow = 0L;
                        if (outflowList.ContainsKey(currentDate))
                            outflow = outflowList[currentDate];
                        long currentValue = entryBudgetValue + budgeted + outflow;
                        budgetEntries.Add((new BudgetEntry
                        {
                            Id = id,
                            CategoryId = categoryId,
                            Budget = budgeted,
                            Month = currentDate
                        }, outflow, currentValue));

                        entryBudgetValue = Math.Max(0L, currentValue);
                        currentDate = new DateTime(
                            currentDate.Month == 12 ? currentDate.Year + 1 : currentDate.Year,
                            currentDate.Month == 12 ? 1 : currentDate.Month + 1,
                            1);

                    } while (currentDate.Year < toMonth.Year || currentDate.Year == toMonth.Year && currentDate.Month <= toMonth.Month);
                    
                }

                

                budgetEntriesPerMonth = budgetEntries
                    .GroupBy(be => be.Entry.Month)
                    .OrderBy(grouping => grouping.Key).ToArray();

                long firstBalance = budgetEntriesPerMonth[0].Where(be => be.Balance > 0).Sum(be => be.Balance);

                initialNotBudgetedOrOverbudgeted =
                    connection.QueryFirst<long?>(NotBudgetedOrOverbudgetedQuery,
                        new
                        {
                            Year = $"{fromMonth.Year:0000}",
                            Month = $"{fromMonth.Month:00}"
                        }) ?? 0L;
                
                foreach (var incomeCategory in incomeCategories)
                {
                    initialNotBudgetedOrOverbudgeted += connection.QueryFirst<long?>(IncomeForCategoryUntilMonthQuery(incomeCategory.MonthOffset), new
                                     {
                                         CategoryId = incomeCategory.Id,
                                         Year = $"{fromMonth.Year:0000}",
                                         Month = $"{fromMonth.Month:00}"
                                     }) ?? 0L;
                }

                initialNotBudgetedOrOverbudgeted -= firstBalance;

                initialNotBudgetedOrOverbudgeted -= budgetEntriesPerMonth[0].Where(be => be.Balance < 0).Sum(be => be.Balance);

                incomesPerMonth = budgetEntriesPerMonth.Select(g => {
                    long incomeSum = 0;
                    foreach (var incomeCategory in incomeCategories)
                    {
                        incomeSum += connection.QueryFirst<long?>(IncomeForCategoryQuery(incomeCategory.MonthOffset), new
                        {
                            CategoryId = incomeCategory.Id,
                            Year = $"{g.Key.Year:0000}",
                            Month = $"{g.Key.Month:00}"
                        }) ?? 0L;
                    }

                    return (g.Key, incomeSum);
                }).ToDictionary(kvp => kvp.Key, kvp => kvp.incomeSum);

                transactionScope.Complete();
            }

            return (budgetEntriesPerMonth, initialNotBudgetedOrOverbudgeted, incomesPerMonth);
        }
    }
}
