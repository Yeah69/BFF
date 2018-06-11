using System;
using System.Collections.Generic;
using System.Linq;
using BFF.DB;

namespace BFF.Helper
{
    public interface INotifyBudgetOverviewRelevantChange
    {
        void TransChangedDate(DateTime date);
    }
    public interface IBudgetOverviewCachingOperations
    {
        bool TryGetValue((int Year, int Half) halfYearBlock, out (IDictionary<long, long> BalancePerCategoryId, long NotBudgetedOrOverbudgeted) budgetOverviewValues);

        void Add((int Year, int Half) halfYearBlock, (IDictionary<long, long> BalancePerCategoryId, long NotBudgetedOrOverbudgeted) budgetOverviewValues);
    }

    public interface IBudgetOverviewCache : INotifyBudgetOverviewRelevantChange, IBudgetOverviewCachingOperations
    {
    }

    public class BudgetOverviewCache : IBudgetOverviewCache, IOncePerBackend
    {
        readonly IDictionary<(int Year, int Half), (IDictionary<long, long> BalancePerCategoryId, long NotBudgetedOrOverbudgeted)> _cachedBudgetOverviews = new Dictionary<(int Year, int Half), (IDictionary<long, long> BalancePerCategoryId, long NotBudgetedOrOverbudgeted)>();

        public void TransChangedDate(DateTime date)
        {
            var toRemoves = _cachedBudgetOverviews
                .Keys
                .Where(t => t.Year == date.Year && (date.Month <= 6 || t.Half == 2)
                             || t.Year > date.Year)
                .ToList();
            foreach (var toRemove in toRemoves)
            {
                _cachedBudgetOverviews.Remove(toRemove);
            }
        }

        public bool TryGetValue((int Year, int Half) halfYearBlock, out (IDictionary<long, long> BalancePerCategoryId, long NotBudgetedOrOverbudgeted) budgetOverviewValues)
        {
            return _cachedBudgetOverviews.TryGetValue(halfYearBlock, out budgetOverviewValues);
        }

        public void Add((int Year, int Half) halfYearBlock, (IDictionary<long, long> BalancePerCategoryId, long NotBudgetedOrOverbudgeted) budgetOverviewValues)
        {
            _cachedBudgetOverviews.Add(halfYearBlock, budgetOverviewValues);
        }
    }
}