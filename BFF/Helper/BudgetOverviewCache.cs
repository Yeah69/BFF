using System;
using System.Collections.Generic;
using System.Linq;
using BFF.DB;

namespace BFF.Helper
{
    public interface INotifyBudgetOverviewRelevantChange
    {
        void Notify(DateTime date);
    }
    public interface IBudgetOverviewCachingOperations
    {
        bool TryGetValue(int year, out (IDictionary<long, long> BalancePerCategoryId, long NotBudgetedOrOverbudgeted) budgetOverviewValues);

        void Add(int year, (IDictionary<long, long> BalancePerCategoryId, long NotBudgetedOrOverbudgeted) budgetOverviewValues);
    }

    public interface IBudgetOverviewCache : INotifyBudgetOverviewRelevantChange, IBudgetOverviewCachingOperations
    {
    }

    public class BudgetOverviewCache : IBudgetOverviewCache, IOncePerBackend
    {
        readonly IDictionary<long, (IDictionary<long, long> BalancePerCategoryId, long NotBudgetedOrOverbudgeted)> _cachedBudgetOverviews = 
            new Dictionary<long, (IDictionary<long, long> BalancePerCategoryId, long NotBudgetedOrOverbudgeted)>();

        public void Notify(DateTime date)
        {
            var toRemoves = _cachedBudgetOverviews
                .Keys
                .Where(y => y >= date.Year)
                .ToList();
            foreach (var toRemove in toRemoves)
            {
                _cachedBudgetOverviews.Remove(toRemove);
            }
        }

        public bool TryGetValue(int year, out (IDictionary<long, long> BalancePerCategoryId, long NotBudgetedOrOverbudgeted) budgetOverviewValues)
        {
            return _cachedBudgetOverviews.TryGetValue(year, out budgetOverviewValues);
        }

        public void Add(int year, (IDictionary<long, long> BalancePerCategoryId, long NotBudgetedOrOverbudgeted) budgetOverviewValues)
        {
            _cachedBudgetOverviews.Add(year, budgetOverviewValues);
        }
    }
}