using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Realm.Models.Persistence;

namespace BFF.Persistence.Realm.ORM.Interfaces
{
    internal interface IBudgetOrm : IOncePerBackend
    {
        Task<BudgetBlock> FindAsync(int year);

        Task UpdateCategorySpecificCacheAsync(Category category, DateTimeOffset month);

        Task DeleteCategorySpecificCacheAsync(Category category);

        Task UpdateGlobalPotCacheAsync(DateTimeOffset month);

        IEnumerable<BudgetCacheEntry> GenerateBudgetCacheEntriesFor(
            Category category,
            IEnumerable<(DateTimeOffset Month, long Budget)> budgets,
            IEnumerable<(DateTimeOffset Month, long Sum)> outflows,
            long initialBalance,
            long initialTotalBudget,
            long initialTotalNegativeBalance);
    }
}