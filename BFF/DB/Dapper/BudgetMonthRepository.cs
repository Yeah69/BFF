using System;
using System.Collections.Generic;
using System.Linq;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper
{
    public interface IBudgetMonthRepository : IDisposable
    {
        IList<IBudgetMonth> Find(DateTime fromMonth, DateTime toMonth);
    }

    public class BudgetMonthRepository : IBudgetMonthRepository
    {
        private readonly IBudgetEntryRepository _budgetEntryRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IIncomeCategoryRepository _incomeCategoryRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IBudgetOrm _budgetOrm;

        public BudgetMonthRepository(
            IBudgetEntryRepository budgetEntryRepository, 
            ICategoryRepository categoryRepository,
            IIncomeCategoryRepository incomeCategoryRepository,
            IAccountRepository accountRepository,
            IBudgetOrm budgetOrm)
        {
            _budgetEntryRepository = budgetEntryRepository;
            _categoryRepository = categoryRepository;
            _incomeCategoryRepository = incomeCategoryRepository;
            _accountRepository = accountRepository;
            _budgetOrm = budgetOrm;
        }

        public IList<IBudgetMonth> Find(DateTime fromMonth, DateTime toMonth)
        {
            DateTime actualFromMonth = new DateTime(
                fromMonth.Month == 1 ? fromMonth.Year - 1 : fromMonth.Year,
                fromMonth.Month == 1 ? 12 : fromMonth.Month - 1,
                1);

            var _ = _budgetOrm.Find(
                actualFromMonth,
                toMonth,
                _categoryRepository.All.Select(c => c.Id).ToArray(),
                _incomeCategoryRepository.All.Select(ic => (ic.Id, ic.MonthOffset)).ToArray());

            long currentNotBudgetedOrOverbudgeted = _.InitialNotBudgetedOrOverbudgeted;
            long currentOverspentInPreviousMonth = _.BudgetEntriesPerMonth
                .OrderBy(kvp => kvp.Key)
                .First()
                .Value
                .Where(be => be.Balance < 0).Sum(be => be.Balance);

            var budgetMonths = new List<IBudgetMonth>();

            foreach (var monthWithBudgetEntries in _.BudgetEntriesPerMonth.Skip(1).OrderBy(kvp => kvp.Key))
            {
                var newBudgetMonth =
                    new BudgetMonth(
                        month: monthWithBudgetEntries.Key,
                        budgetEntries: monthWithBudgetEntries.Value.Select(g => _budgetEntryRepository.Convert(g.Entry, g.Outflow, g.Balance)),
                        overspentInPreviousMonth: currentOverspentInPreviousMonth,
                        notBudgetedInPreviousMonth: currentNotBudgetedOrOverbudgeted,
                        incomeForThisMonth: _.IncomesPerMonth[monthWithBudgetEntries.Key]
                                            + _accountRepository
                                                .All
                                                .Where(a => a.StartingDate.Year == monthWithBudgetEntries.Key.Year && a.StartingDate.Month == monthWithBudgetEntries.Key.Month)
                                                .Select(a => a.StartingBalance).Sum(),
                        danglingTransferForThisMonth: _.DanglingTransfersPerMonth[monthWithBudgetEntries.Key]);
                budgetMonths.Add(newBudgetMonth);
                currentNotBudgetedOrOverbudgeted = newBudgetMonth.AvailableToBudget;
                currentOverspentInPreviousMonth = monthWithBudgetEntries
                    .Value
                    .Where(be => be.Balance < 0).Sum(be => be.Balance);
            }
            return budgetMonths;
        }

        public void Dispose()
        {
        }
    }
}
