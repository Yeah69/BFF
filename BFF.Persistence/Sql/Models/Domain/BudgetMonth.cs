using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BFF.Model.Models;
using BFF.Model.Models.Structure;
using BFF.Model.Repositories;
using BFF.Persistence.Sql.Repositories.ModelRepositories;
using MrMeeseeks.Extensions;
using MrMeeseeks.Utility;

namespace BFF.Persistence.Sql.Models.Domain
{
    internal class BudgetMonth : Model.Models.BudgetMonth
    {
        private readonly ISqliteTransRepository _transRepository;
        private readonly IIncomeCategoryRepository _incomeCategoryRepository;

        public BudgetMonth(
            ISqliteTransRepository transRepository,
            IIncomeCategoryRepository incomeCategoryRepository,
            DateTime month, 
            IEnumerable<IBudgetEntry> budgetEntries, 
            long overspentInPreviousMonth, 
            long notBudgetedInPreviousMonth, 
            long incomeForThisMonth,
            long danglingTransferForThisMonth,
            long unassignedTransactionSumForThisMonth) : base(month, budgetEntries, overspentInPreviousMonth, notBudgetedInPreviousMonth, incomeForThisMonth, danglingTransferForThisMonth, unassignedTransactionSumForThisMonth)
        {
            _transRepository = transRepository;
            _incomeCategoryRepository = incomeCategoryRepository;
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
    }
}
