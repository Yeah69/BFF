using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;

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
            return ConnectionHelper.QueryOnExistingOrNewConnection<IBudgetMonth>(c =>
            {
                return _categoryRepository.All.Select(category =>
                    _budgetEntryRepository.GetBudgetEntries(fromMonth, toMonth, category, c))
                    .SelectMany(l => l)
                    .GroupBy(be => be.Month)
                    .OrderBy(grouping => grouping.Key)
                    .Select(grouping => new BudgetMonth(grouping))
                    .ToList();
            }, _provideConnection, connection).ToList();
        }

        public void Dispose()
        {
        }
    }
}
