using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;
using MoreLinq;

namespace BFF.DB.Dapper
{
    public interface IBudgetMonthRepository : IDisposable
    {
        IList<IBudgetMonth> Find(DateTime fromMonth, DateTime toMonth, DbConnection connection);
    }

    public class BudgetMonthRepository : IBudgetMonthRepository
    {
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
            return ConnectionHelper.QueryOnExistingOrNewConnection<IBudgetMonth>(c =>
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

                for (int i = 1; i < groupings.Length; i++)
                {
                    budgetMonths.Add(
                        new BudgetMonth(
                            groupings[i].Key,
                            groupings[i],
                            groupings[i - 1].Where(be => be.Balance < 0).Sum(be => be.Balance),
                            0L,
                            0L));
                }

                return budgetMonths;
            }, _provideConnection, connection).ToList();
        }

        public void Dispose()
        {
        }
    }
}
