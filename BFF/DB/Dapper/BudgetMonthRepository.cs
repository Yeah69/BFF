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

            var budgetMonths = new List<IBudgetMonth>();

            for (int i = 1; i < _.BudgetEntriesPerMonth.Length; i++)
            {
                var newBudgetMonth =
                    new BudgetMonth(
                        month: _.BudgetEntriesPerMonth[i].Key,
                        budgetEntries: _.BudgetEntriesPerMonth[i].Select(g => _budgetEntryRepository.Convert(g.Entry, g.Outflow, g.Balance)),
                        overspentInPreviousMonth: _.BudgetEntriesPerMonth[i - 1].Where(be => be.Balance < 0).Sum(be => be.Balance),
                        notBudgetedInPreviousMonth: currentNotBudgetedOrOverbudgeted,
                        incomeForThisMonth: _.IncomesPerMonth[_.BudgetEntriesPerMonth[i].Key]
                                            + _accountRepository
                                                .All
                                                .Where(a => a.StartingDate.Year == _.BudgetEntriesPerMonth[i].Key.Year && a.StartingDate.Month == _.BudgetEntriesPerMonth[i].Key.Month)
                                                .Select(a => a.StartingBalance).Sum(),
                        danglingTransferForThisMonth: _.DanglingTransfersPerMonth[_.BudgetEntriesPerMonth[i].Key]);
                budgetMonths.Add(newBudgetMonth);
                currentNotBudgetedOrOverbudgeted = newBudgetMonth.AvailableToBudget;
            }
            return budgetMonths;
        }

        public void Dispose()
        {
        }
    }
}
