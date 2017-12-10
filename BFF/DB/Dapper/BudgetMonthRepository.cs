using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;
using Dapper;
using Persistence = BFF.DB.PersistenceModels;

namespace BFF.DB.Dapper
{
    public interface IBudgetMonthRepository : IDisposable
    {
        IList<IBudgetMonth> Find(DateTime fromMonth, DateTime toMonth, DbConnection connection);
    }

    public class BudgetMonthRepository : IBudgetMonthRepository
    {
        private static readonly string NotBudgetedOrOverbudgetedQuery =
$@"SELECT Sum(Sum) as Sum FROM
    (
        SELECT {nameof(Persistence.Income.Sum)}
        FROM {nameof(Persistence.Income)}s
        WHERE {nameof(Persistence.Income.CategoryId)} == @NextCategoryId AND (strftime('%Y', {nameof(Persistence.Income.Date)}, '+1 month') < @Year OR strftime('%Y', {nameof(Persistence.Income.Date)}, '+1 month') == @Year AND strftime('%m', {nameof(Persistence.Income.Date)}, '+1 month') <= @Month)
        UNION ALL
        SELECT {nameof(Persistence.Income.Sum)}
        FROM {nameof(Persistence.Income)}s
        WHERE {nameof(Persistence.Income.CategoryId)} == @ThisCategoryId AND (strftime('%Y', {nameof(Persistence.Income.Date)}) < @Year OR strftime('%Y', {nameof(Persistence.Income.Date)}) == @Year AND strftime('%m', {nameof(Persistence.Income.Date)}) <= @Month)
        UNION ALL
        SELECT Sum({nameof(Persistence.Account.StartingBalance)}) AS Sum FROM {nameof(Persistence.Account)}s
        UNION ALL
        SELECT {nameof(Persistence.Transaction.Sum)}
        FROM {nameof(Persistence.Transaction)}s
        WHERE strftime('%Y', {nameof(Persistence.Transaction.Date)}) < @Year
            OR strftime('%m', {nameof(Persistence.Transaction.Date)}) <= @Month
            AND strftime('%Y', {nameof(Persistence.Transaction.Date)}) = @Year
        UNION ALL
        SELECT {nameof(Persistence.SubTransaction)}s.{nameof(Persistence.SubTransaction.Sum)}
        FROM {nameof(Persistence.SubTransaction)}s
        INNER JOIN {nameof(Persistence.ParentTransaction)}s ON {nameof(Persistence.SubTransaction)}s.{nameof(Persistence.SubTransaction.ParentId)} = {nameof(Persistence.ParentTransaction)}s.{nameof(Persistence.ParentTransaction.Id)}
        WHERE strftime('%Y', {nameof(Persistence.ParentTransaction.Date)}) < @Year
            OR strftime('%m', {nameof(Persistence.ParentTransaction.Date)}) <= @Month
            AND strftime('%Y', {nameof(Persistence.ParentTransaction.Date)}) = @Year
    );"; // TODO this is proprietary to YNAB. Adjust ASAP (Income-Categories required)! // TODO Accounts need a StartMonth to locate the StartingBalance precisely

        private static readonly string IncomeForGivenMonthQuery =
            $@"SELECT Sum({nameof(Persistence.Income.Sum)}) as Sum FROM
  (
    SELECT {nameof(Persistence.Income.Sum)}
    FROM {nameof(Persistence.Income)}s
    WHERE {nameof(Persistence.Income.CategoryId)} == @NextCategoryId AND strftime('%Y', {nameof(Persistence.Income.Date)}, '+1 month') == @Year AND strftime('%m', {nameof(Persistence.Income.Date)}, '+1 month') == @Month
    UNION ALL
    SELECT {nameof(Persistence.Income.Sum)}
    FROM {nameof(Persistence.Income)}s
    WHERE {nameof(Persistence.Income.CategoryId)} == @ThisCategoryId AND strftime('%Y', {nameof(Persistence.Income.Date)}) == @Year AND strftime('%m', {nameof(Persistence.Income.Date)}) == @Month
  );"; // TODO this is proprietary to YNAB. Adjust ASAP (Income-Categories required)! // TODO Accounts need a StartMonth to locate the StartingBalance precisely

        private readonly IBudgetEntryRepository _budgetEntryRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProvideConnection _provideConnection;

        public BudgetMonthRepository(
            IBudgetEntryRepository budgetEntryRepository, 
            ICategoryRepository categoryRepository,
            IProvideConnection provideConnection)
        {
            _budgetEntryRepository = budgetEntryRepository;
            _categoryRepository = categoryRepository;
            _provideConnection = provideConnection;
        }

        public IList<IBudgetMonth> Find(DateTime fromMonth, DateTime toMonth, DbConnection connection)
        {
            DateTime actualFromMonth = new DateTime(
                fromMonth.Month == 1 ? fromMonth.Year - 1 : fromMonth.Year,
                fromMonth.Month == 1 ? 12 : fromMonth.Month - 1,
                1);
            return ConnectionHelper.QueryOnExistingOrNewConnection(c =>
            {
                var groupings = _categoryRepository
                    .All
                    .Where(category =>
                        category.Name != "Available this month" &&
                        category.Name != "Available next month") // TODO this is proprietary to YNAB. Adjust ASAP (Income-Categories required)! 
                    .Select(category => _budgetEntryRepository.GetBudgetEntries(actualFromMonth, toMonth, category, c))
                    .SelectMany(l => l)
                    .GroupBy(be => be.Month)
                    .OrderBy(grouping => grouping.Key).ToArray();

                var budgetMonths = new List<IBudgetMonth>();

                long thisCategoryId = _categoryRepository.All.Single(cat => cat.Name == "Available this month").Id; // TODO this is proprietary to YNAB. Adjust ASAP (Income-Categories required)! 
                long nextCategoryId = _categoryRepository.All.Single(cat => cat.Name == "Available next month").Id; // TODO this is proprietary to YNAB. Adjust ASAP (Income-Categories required)! 

                long firstBalance = groupings[0].Where(be => be.Balance > 0).Sum(be => be.Balance);
                long currentNotBudgetedOrOverbudgeted =
                    c.QuerySingleOrDefault<long?>(
                        NotBudgetedOrOverbudgetedQuery,
                        new
                        {
                            Year = $"{actualFromMonth.Year:0000}",
                            Month = $"{actualFromMonth.Month:00}",
                            ThisCategoryId = thisCategoryId,
                            NextCategoryId = nextCategoryId
                        }) ?? 0L;

                currentNotBudgetedOrOverbudgeted -= firstBalance;

                for (int i = 1; i < groupings.Length; i++)
                {
                    var newBudgetMonth =
                        new BudgetMonth(
                            groupings[i].Key,
                            groupings[i],
                            groupings[i - 1].Where(be => be.Balance < 0).Sum(be => be.Balance),
                            currentNotBudgetedOrOverbudgeted,
                            c.QuerySingleOrDefault<long?>(
                                IncomeForGivenMonthQuery,
                                new
                                {
                                    Year = $"{groupings[i].Key.Year:0000}",
                                    Month = $"{groupings[i].Key.Month:00}",
                                    ThisCategoryId = thisCategoryId,
                                    NextCategoryId = nextCategoryId
                                }
                            ) ?? 0L);
                    budgetMonths.Add(newBudgetMonth);
                    currentNotBudgetedOrOverbudgeted = newBudgetMonth.AvailableToBudget;
                }

                return budgetMonths;
            }, _provideConnection, connection).ToList();
        }

        public void Dispose()
        {
        }
    }
}
