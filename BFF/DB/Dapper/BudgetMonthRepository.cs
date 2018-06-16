using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.DB.Dapper.ModelRepositories;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native;

namespace BFF.DB.Dapper
{
    public interface IBudgetMonthRepository : IDisposable
    {
        Task<IList<IBudgetMonth>> FindAsync(int year);
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

        public async Task<IList<IBudgetMonth>> FindAsync(int year)
        {
            var _ = await _budgetOrm.FindAsync(
                year,
                _categoryRepository.All.Select(c => c.Id).ToArray(),
                _incomeCategoryRepository.All.Select(ic => (ic.Id, ic.MonthOffset)).ToArray()).ConfigureAwait(false);

            long currentNotBudgetedOrOverbudgeted = _.InitialNotBudgetedOrOverbudgeted;
            long currentOverspentInPreviousMonth = _.InitialOverspentInPreviousMonth;

            var budgetMonths = new List<IBudgetMonth>();

            foreach (var monthWithBudgetEntries in _.BudgetEntriesPerMonth.OrderBy(kvp => kvp.Key))
            {
                var newBudgetMonth =
                    new BudgetMonth(
                        month: monthWithBudgetEntries.Key,
                        budgetEntries: 
                            await monthWithBudgetEntries
                                .Value
                                .Select(async g => await _budgetEntryRepository.Convert(g.Entry, g.Outflow, g.Balance).ConfigureAwait(false))
                                .ToAwaitableEnumerable()
                                .ConfigureAwait(false),
                        overspentInPreviousMonth: currentOverspentInPreviousMonth,
                        notBudgetedInPreviousMonth: currentNotBudgetedOrOverbudgeted,
                        incomeForThisMonth: _.IncomesPerMonth[monthWithBudgetEntries.Key]
                                            + _accountRepository
                                                .All
                                                .Where(a => a.StartingDate.Year == monthWithBudgetEntries.Key.Year && a.StartingDate.Month == monthWithBudgetEntries.Key.Month)
                                                .Select(a => a.StartingBalance).Sum(),
                        danglingTransferForThisMonth: _.DanglingTransfersPerMonth[monthWithBudgetEntries.Key],
                        unassignedTransactionSumForThisMonth: _.UnassignedTransactionsPerMonth[monthWithBudgetEntries.Key]);
                budgetMonths.Add(newBudgetMonth);
                currentNotBudgetedOrOverbudgeted = newBudgetMonth.AvailableToBudget;
                currentOverspentInPreviousMonth = monthWithBudgetEntries
                    .Value
                    .Where(be => be.Balance < 0)
                    .Sum(be => be.Balance);
            }
            return budgetMonths;
        }

        public void Dispose()
        {
        }
    }
}
