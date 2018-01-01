﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using BFF.DB.Dapper.ModelRepositories;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
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
        SELECT Sum({nameof(Persistence.Account.StartingBalance)}) AS Sum FROM {nameof(Persistence.Account)}s
        WHERE strftime('%Y', {nameof(Persistence.Account.StartingDate)}) < @Year OR strftime('%Y', {nameof(Persistence.Account.StartingDate)}) == @Year AND strftime('%m', {nameof(Persistence.Account.StartingDate)}) <= @Month
        UNION ALL
        SELECT {nameof(Persistence.Trans.Sum)}
        FROM {nameof(Persistence.Trans)}s
        INNER JOIN {nameof(Persistence.Category)}s ON {nameof(Persistence.Trans.Type)} == '{nameof(TransType.Transaction)}' AND {nameof(Persistence.Trans.CategoryId)} == {nameof(Persistence.Category)}s.{nameof(Persistence.Category.Id)} AND {nameof(Persistence.Category)}s.{nameof(Persistence.Category.IsIncomeRelevant)} == 0
        WHERE strftime('%Y', {nameof(Persistence.Trans.Date)}) < @Year
            OR strftime('%m', {nameof(Persistence.Trans.Date)}) <= @Month
            AND strftime('%Y', {nameof(Persistence.Trans.Date)}) = @Year
        UNION ALL
        SELECT {nameof(Persistence.SubTransaction)}s.{nameof(Persistence.SubTransaction.Sum)}
        FROM {nameof(Persistence.SubTransaction)}s
        INNER JOIN {nameof(Persistence.Category)}s ON {nameof(Persistence.SubTransaction)}s.{nameof(Persistence.SubTransaction.CategoryId)} == {nameof(Persistence.Category)}s.{nameof(Persistence.Category.Id)} AND {nameof(Persistence.Category)}s.{nameof(Persistence.Category.IsIncomeRelevant)} == 0
        INNER JOIN {nameof(Persistence.Trans)}s ON {nameof(Persistence.SubTransaction)}s.{nameof(Persistence.SubTransaction.ParentId)} = {nameof(Persistence.Trans)}s.{nameof(Persistence.Trans.Id)}
        WHERE strftime('%Y', {nameof(Persistence.Trans.Date)}) < @Year
            OR strftime('%m', {nameof(Persistence.Trans.Date)}) <= @Month
            AND strftime('%Y', {nameof(Persistence.Trans.Date)}) = @Year
    );";

        private readonly IBudgetEntryRepository _budgetEntryRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IIncomeCategoryRepository _incomeCategoryRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IProvideConnection _provideConnection;

        public BudgetMonthRepository(
            IBudgetEntryRepository budgetEntryRepository, 
            ICategoryRepository categoryRepository,
            IIncomeCategoryRepository incomeCategoryRepository,
            IAccountRepository accountRepository,
            IProvideConnection provideConnection)
        {
            _budgetEntryRepository = budgetEntryRepository;
            _categoryRepository = categoryRepository;
            _incomeCategoryRepository = incomeCategoryRepository;
            _accountRepository = accountRepository;
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
                    .Select(category => _budgetEntryRepository.GetBudgetEntries(actualFromMonth, toMonth, category, c))
                    .SelectMany(l => l)
                    .GroupBy(be => be.Month)
                    .OrderBy(grouping => grouping.Key).ToArray();

                var budgetMonths = new List<IBudgetMonth>();

                long firstBalance = groupings[0].Where(be => be.Balance > 0).Sum(be => be.Balance);
                long currentNotBudgetedOrOverbudgeted =
                    c.QuerySingleOrDefault<long?>(
                        NotBudgetedOrOverbudgetedQuery,
                        new
                        {
                            Year = $"{actualFromMonth.Year:0000}",
                            Month = $"{actualFromMonth.Month:00}"
                        }) ?? 0L;
                currentNotBudgetedOrOverbudgeted += _incomeCategoryRepository.GetIncomeUntilMonth(actualFromMonth, c);

                currentNotBudgetedOrOverbudgeted -= firstBalance;

                currentNotBudgetedOrOverbudgeted -= groupings[0].Where(be => be.Balance < 0).Sum(be => be.Balance);

                for (int i = 1; i < groupings.Length; i++)
                {
                    var newBudgetMonth =
                        new BudgetMonth(
                            month: groupings[i].Key,
                            budgetEntries: groupings[i],
                            overspentInPreviousMonth: groupings[i - 1].Where(be => be.Balance < 0).Sum(be => be.Balance),
                            notBudgetedInPreviousMonth: currentNotBudgetedOrOverbudgeted,
                            incomeForThisMonth: _incomeCategoryRepository.GetMonthsIncome(groupings[i].Key, c) 
                                                + _accountRepository.All.Where(a => a.StartingDate.Year == groupings[i].Key.Year && a.StartingDate.Month == groupings[i].Key.Month).Select(a => a.StartingBalance).Sum());
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
