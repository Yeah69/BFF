﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Sql.Models.Domain;
using BFF.Persistence.Sql.ORM.Interfaces;
using BFF.Persistence.Sql.Repositories.ModelRepositories;
using MrMeeseeks.Extensions;

namespace BFF.Persistence.Sql.Repositories
{
    internal class SqliteBudgetMonthRepository : IBudgetMonthRepository
    {
        private readonly Lazy<ISqliteBudgetEntryRepository> _budgetEntryRepository;
        private readonly Lazy<ICategoryRepository> _categoryRepository;
        private readonly Lazy<IIncomeCategoryRepository> _incomeCategoryRepository;
        private readonly Lazy<IAccountRepository> _accountRepository;
        private readonly Lazy<ISqliteTransRepository> _transRepository;
        private readonly Lazy<IBudgetOrm> _budgetOrm;

        public SqliteBudgetMonthRepository(
            Lazy<ISqliteBudgetEntryRepository> budgetEntryRepository,
            Lazy<ICategoryRepository> categoryRepository,
            Lazy<IIncomeCategoryRepository> incomeCategoryRepository,
            Lazy<IAccountRepository> accountRepository,
            Lazy<ISqliteTransRepository> transRepository,
            Lazy<IBudgetOrm> budgetOrm)
        {
            _budgetEntryRepository = budgetEntryRepository;
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

            foreach (var monthWithBudgetEntries in _.BudgetEntriesPerMonth.OrderBy(kvp => kvp.Key))
            {
                var newBudgetMonth =
                    new Models.Domain.BudgetMonth(
                        _transRepository.Value,
                        _incomeCategoryRepository.Value,
                        month: monthWithBudgetEntries.Key,
                        budgetEntries: 
                            await monthWithBudgetEntries
                                .Value
                                .Select(async g => await _budgetEntryRepository.Value.Convert(g.Entry, g.Outflow, g.Balance).ConfigureAwait(false))
                                .ToAwaitableEnumerable()
                                .ConfigureAwait(false),
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
