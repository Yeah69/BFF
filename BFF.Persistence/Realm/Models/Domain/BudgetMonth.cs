using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Model.Models.Structure;
using BFF.Model.Repositories;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;
using MrMeeseeks.Extensions;
using MrMeeseeks.Utility;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class BudgetMonth : Model.Models.BudgetMonth
    {
        private readonly IRealmTransRepository _transRepository;
        private readonly IIncomeCategoryRepository _incomeCategoryRepository;
        private readonly IBudgetOrm _budgetOrm;

        public BudgetMonth(
            // parameter
            DateTime month, 
            (long Budget, long Outflow, long Balance) budgetData, 
            long overspentInPreviousMonth, 
            long notBudgetedInPreviousMonth, 
            long incomeForThisMonth,
            long danglingTransferForThisMonth,
            long unassignedTransactionSumForThisMonth,
            
            // dependencies
            IRealmTransRepository transRepository,
            IIncomeCategoryRepository incomeCategoryRepository,
            IBudgetOrm budgetOrm) 
            : base(
                month, 
                budgetData, 
                overspentInPreviousMonth, 
                notBudgetedInPreviousMonth, 
                incomeForThisMonth, 
                danglingTransferForThisMonth, 
                unassignedTransactionSumForThisMonth)
        {
            _transRepository = transRepository;
            _incomeCategoryRepository = incomeCategoryRepository;
            _budgetOrm = budgetOrm;
        }

        public override Task<IEnumerable<ITransBase>> GetAssociatedTransAsync()
        {
            return _transRepository.GetFromMonthAsync(Month);
        }

        public override async Task<IEnumerable<ITransBase>> GetAssociatedTransForIncomeCategoriesAsync()
        {
            var tasks = _incomeCategoryRepository
                .All
                .GroupBy(ic => ic.MonthOffset)
                .Select(async g =>
                    await _transRepository.GetFromMonthAndCategoriesAsync(
                        Month.OffsetMonthBy(g.Key * -1),
                        g).ConfigureAwait(false));
            return (await Task.WhenAll(tasks).ConfigureAwait(false))
                .SelectMany(Basic.Identity)
                .OrderBy(tb => tb.Date)
                .ToList();
        }

        public override Task EmptyBudgetEntriesToAvgBudget(int monthCount) => 
            _budgetOrm.SetEmptyBudgetEntriesToAvgBudget(this.Month.ToMonthIndex(), monthCount);

        public override Task EmptyBudgetEntriesToAvgOutflow(int monthCount) => 
            _budgetOrm.SetEmptyBudgetEntriesToAvgOutflow(this.Month.ToMonthIndex(), monthCount);

        public override Task EmptyBudgetEntriesToBalanceZero() => 
            _budgetOrm.SetEmptyBudgetEntriesToBalanceZero(this.Month.ToMonthIndex());
        public override Task AllBudgetEntriesToZero() => 
            _budgetOrm.SetAllBudgetEntriesToZero(this.Month.ToMonthIndex());
    }
}
