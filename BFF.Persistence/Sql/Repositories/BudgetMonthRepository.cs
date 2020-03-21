using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Sql.Models.Domain;
using BFF.Persistence.Sql.ORM.Interfaces;
using BFF.Persistence.Sql.Repositories.ModelRepositories;

namespace BFF.Persistence.Sql.Repositories
{
    internal class SqliteBudgetMonthRepository : IBudgetMonthRepository
    {
        private readonly Lazy<ICategoryRepository> _categoryRepository;
        private readonly Lazy<IIncomeCategoryRepository> _incomeCategoryRepository;
        private readonly Lazy<IAccountRepository> _accountRepository;
        private readonly Lazy<ISqliteTransRepository> _transRepository;
        private readonly Lazy<IBudgetOrm> _budgetOrm;

        public SqliteBudgetMonthRepository(
            Lazy<ICategoryRepository> categoryRepository,
            Lazy<IIncomeCategoryRepository> incomeCategoryRepository,
            Lazy<IAccountRepository> accountRepository,
            Lazy<ISqliteTransRepository> transRepository,
            Lazy<IBudgetOrm> budgetOrm)
        {
            _categoryRepository = categoryRepository;
            _incomeCategoryRepository = incomeCategoryRepository;
            _accountRepository = accountRepository;
            _transRepository = transRepository;
            _budgetOrm = budgetOrm;
        }

        public async Task<IList<IBudgetMonth>> FindAsync(int year)
        {
            var _ = await _budgetOrm.Value.FindAsync(
                year,
                (await _categoryRepository.Value.AllAsync.ConfigureAwait(false)).OfType<ISqlModel>().Select(c => c.Id).ToArray(),
                (await _incomeCategoryRepository.Value.AllAsync.ConfigureAwait(false)).OfType<ISqlModel>().Select(ic => (ic.Id, (ic as IIncomeCategory).MonthOffset)).ToArray()).ConfigureAwait(false);

            long currentNotBudgetedOrOverbudgeted = _.InitialNotBudgetedOrOverbudgeted;
            long currentOverspentInPreviousMonth = _.InitialOverspentInPreviousMonth;

            var budgetMonths = new List<IBudgetMonth>();

            foreach (var monthWithBudgetEntries in _.BudgetDataPerMonth.OrderBy(kvp => kvp.Key))
            {
                var newBudgetMonth =
                    new Models.Domain.BudgetMonth(
                        _transRepository.Value,
                        _incomeCategoryRepository.Value,
                        month: monthWithBudgetEntries.Key,
                        budgetData:  (monthWithBudgetEntries.Value.Budget, monthWithBudgetEntries.Value.Outflow, monthWithBudgetEntries.Value.Balance),
                        overspentInPreviousMonth: currentOverspentInPreviousMonth,
                        notBudgetedInPreviousMonth: currentNotBudgetedOrOverbudgeted,
                        incomeForThisMonth: _.IncomesPerMonth[monthWithBudgetEntries.Key]
                                            + (await _accountRepository
                                                .Value
                                                .AllAsync
                                                .ConfigureAwait(false))
                                                .Where(a => a.StartingDate.Year == monthWithBudgetEntries.Key.Year && a.StartingDate.Month == monthWithBudgetEntries.Key.Month)
                                                .Select(a => a.StartingBalance).Sum(),
                        danglingTransferForThisMonth: _.DanglingTransfersPerMonth[monthWithBudgetEntries.Key],
                        unassignedTransactionSumForThisMonth: _.UnassignedTransactionsPerMonth[monthWithBudgetEntries.Key]);
                budgetMonths.Add(newBudgetMonth);
                currentNotBudgetedOrOverbudgeted = newBudgetMonth.AvailableToBudget;
                currentOverspentInPreviousMonth = monthWithBudgetEntries.Value.Overspend;
            }
            return budgetMonths;
        }

        public void Dispose()
        {
        }
    }
}
