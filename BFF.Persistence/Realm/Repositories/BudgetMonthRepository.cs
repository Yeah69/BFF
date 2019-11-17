using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;
using MrMeeseeks.Extensions;

namespace BFF.Persistence.Realm.Repositories
{
    internal class RealmBudgetMonthRepository : IBudgetMonthRepository
    {
        private readonly Lazy<IRealmBudgetEntryRepository> _budgetEntryRepository;
        private readonly Lazy<IIncomeCategoryRepository> _incomeCategoryRepository;
        private readonly Lazy<IRealmTransRepository> _transRepository;
        private readonly Lazy<IBudgetOrm> _budgetOrm;

        public RealmBudgetMonthRepository(
            Lazy<IRealmBudgetEntryRepository> budgetEntryRepository,
            Lazy<IIncomeCategoryRepository> incomeCategoryRepository,
            Lazy<IRealmTransRepository> transRepository,
            Lazy<IBudgetOrm> budgetOrm)
        {
            _budgetEntryRepository = budgetEntryRepository;
            _incomeCategoryRepository = incomeCategoryRepository;
            _transRepository = transRepository;
            _budgetOrm = budgetOrm;
        }

        public async Task<IList<IBudgetMonth>> FindAsync(int year)
        {
            var budgetBlock = await _budgetOrm
                .Value
                .FindAsync(year)
                .ConfigureAwait(false);

            long currentNotBudgetedOrOverbudgeted = budgetBlock.InitialNotBudgetedOrOverbudgeted;
            long currentOverspentInPreviousMonth = budgetBlock.InitialOverspentInPreviousMonth;

            var budgetMonths = new List<IBudgetMonth>();

            foreach (var monthWithBudgetEntries in budgetBlock.BudgetEntriesPerMonth.OrderBy(kvp => kvp.Key))
            {
                var newBudgetMonth =
                    new Models.Domain.BudgetMonth(
                        _transRepository.Value,
                        _incomeCategoryRepository.Value,
                        month: monthWithBudgetEntries.Key,
                        budgetEntries: 
                            await monthWithBudgetEntries
                                .Value
                                .Select(async g => await _budgetEntryRepository.Value.Convert(
                                    g.Entry,
                                    g.Category, 
                                    monthWithBudgetEntries.Key, 
                                    g.Budget, 
                                    g.Outflow, 
                                    g.Balance).ConfigureAwait(false))
                                .ToAwaitableEnumerable()
                                .ConfigureAwait(false),
                        overspentInPreviousMonth: currentOverspentInPreviousMonth,
                        notBudgetedInPreviousMonth: currentNotBudgetedOrOverbudgeted,
                        incomeForThisMonth: budgetBlock.IncomesPerMonth.TryGetValue(monthWithBudgetEntries.Key, out var income) ? income : 0L,
                        danglingTransferForThisMonth: budgetBlock.DanglingTransfersPerMonth.TryGetValue(monthWithBudgetEntries.Key, out var dangling) ? dangling : 0L,
                        unassignedTransactionSumForThisMonth: budgetBlock.UnassignedTransactionsPerMonth.TryGetValue(monthWithBudgetEntries.Key, out var unassigned) ? unassigned : 0L);
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
