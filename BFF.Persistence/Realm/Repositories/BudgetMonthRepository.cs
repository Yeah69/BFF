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
        private readonly Lazy<IIncomeCategoryRepository> _incomeCategoryRepository;
        private readonly Lazy<IRealmTransRepository> _transRepository;
        private readonly Lazy<IBudgetOrm> _budgetOrm;

        public RealmBudgetMonthRepository(
            Lazy<IIncomeCategoryRepository> incomeCategoryRepository,
            Lazy<IRealmTransRepository> transRepository,
            Lazy<IBudgetOrm> budgetOrm)
        {
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

            foreach (var monthBudgetData in budgetBlock.BudgetDataPerMonth.OrderBy(kvp => kvp.Key))
            {
                var newBudgetMonth =
                    new Models.Domain.BudgetMonth(
                        month: DateTimeExtensions.FromMonthIndex(monthBudgetData.Key),
                        budgetData: (monthBudgetData.Value.Budget, monthBudgetData.Value.Outflow, monthBudgetData.Value.Balance),
                        overspentInPreviousMonth: currentOverspentInPreviousMonth,
                        notBudgetedInPreviousMonth: currentNotBudgetedOrOverbudgeted,
                        incomeForThisMonth: budgetBlock.IncomesPerMonth.TryGetValue(monthBudgetData.Key, out var income) ? income : 0L,
                        danglingTransferForThisMonth: budgetBlock.DanglingTransfersPerMonth.TryGetValue(monthBudgetData.Key, out var dangling) ? dangling : 0L,
                        unassignedTransactionSumForThisMonth: budgetBlock.UnassignedTransactionsPerMonth.TryGetValue(monthBudgetData.Key, out var unassigned) ? unassigned : 0L,
                        _transRepository.Value,
                        _incomeCategoryRepository.Value,
                        _budgetOrm.Value);
                budgetMonths.Add(newBudgetMonth);
                currentNotBudgetedOrOverbudgeted = newBudgetMonth.AvailableToBudget;
                currentOverspentInPreviousMonth = monthBudgetData.Value.Overspend;
            }
            return budgetMonths;
        }

        public async Task<long> GetAvailableToBudgetOfCurrentMonth()
        {
            var now = DateTime.Now;
            return (await this.FindAsync(now.Year).ConfigureAwait(false))[now.Month - 1].AvailableToBudget;
        }

        public void Dispose()
        {
        }
    }
}
