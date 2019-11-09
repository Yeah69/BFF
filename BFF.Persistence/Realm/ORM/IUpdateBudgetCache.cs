using System;
using System.Threading.Tasks;
using BFF.Persistence.Realm.ORM.Interfaces;
using MrMeeseeks.Extensions;

namespace BFF.Persistence.Realm.ORM
{
    internal interface IUpdateBudgetCache
    {
        Task OnBudgetEntryChange(
            Models.Persistence.Category category,
            DateTimeOffset date);

        Task OnTransactionInsertOrDeleteAsync(
            Models.Persistence.Category category,
            DateTimeOffset date);
        Task OnTransactionUpdateAsync(
            Models.Persistence.Category beforeCategory,
            DateTimeOffset beforeDate,
            long beforeSum,
            Models.Persistence.Category afterCategory,
            DateTimeOffset afterDate,
            long afterSum);

        Task OnTransferInsertOrDeleteAsync(
            Models.Persistence.Account fromAccount,
            Models.Persistence.Account toAccount,
            DateTimeOffset date);
        Task OnTransferUpdateAsync(
            Models.Persistence.Account beforeFromAccount,
            Models.Persistence.Account beforeToAccount,
            DateTimeOffset beforeDate,
            long beforeSum,
            Models.Persistence.Account afterFromAccount,
            Models.Persistence.Account afterToAccount,
            DateTimeOffset afterDate,
            long afterSum);

        Task OnCategoryDeletion(
            Models.Persistence.Category category);

        Task OnCategoryMergeForTarget(
            Models.Persistence.Category category);

        Task OnIncomeCategoryChange(
            Models.Persistence.Category category,
            int beforeIncomeMonthOffset,
            int afterIncomeMonthOffset);

        Task OnAccountChange(
            DateTimeOffset beforeStartingDate,
            long beforeStartingBalance,
            DateTimeOffset afterStartingDate,
            long afterStartingBalance);

        Task OnAccountInsertOrDelete(
            DateTimeOffset startingDate);
    }

    internal class UpdateBudgetCache : IUpdateBudgetCache
    {
        private readonly IBudgetOrm _budgetOrm;

        public UpdateBudgetCache(IBudgetOrm budgetOrm)
        {
            _budgetOrm = budgetOrm;
        }

        public async Task OnTransactionUpdateAsync(
            Models.Persistence.Category beforeCategory,
            DateTimeOffset beforeDate, 
            long beforeSum,
            Models.Persistence.Category afterCategory,
            DateTimeOffset afterDate,
            long afterSum)
        {
            // if a budget relevant property changed then update caches
            if (beforeCategory != afterCategory
                || beforeDate != afterDate
                || beforeSum != afterSum)
            {
                // Both sides either unassigned or income relevant
                if ((beforeCategory is null || beforeCategory.IsIncomeRelevant)
                    && (afterCategory is null || afterCategory.IsIncomeRelevant))
                {
                    if (beforeCategory != afterCategory
                        && ((beforeCategory is null || beforeCategory.IsIncomeRelevant && beforeCategory.IncomeMonthOffset == 0)
                        && (afterCategory is null || afterCategory.IsIncomeRelevant && afterCategory.IncomeMonthOffset == 0)
                        || beforeCategory != null && beforeCategory.IsIncomeRelevant
                        && afterCategory != null && afterCategory.IsIncomeRelevant
                        && beforeCategory.IncomeMonthOffset == afterCategory.IncomeMonthOffset))
                        // in this cases no need to update the cache
                        return;

                    var monthBefore =
                        new DateTimeOffset(beforeDate.Year, beforeDate.Month, 1, 0, 0, 0, TimeSpan.Zero);
                    if (beforeCategory != null) monthBefore = monthBefore.OffsetMonthBy(beforeCategory.IncomeMonthOffset);
                    var monthAfter = new DateTimeOffset(afterDate.Year, afterDate.Month, 1, 0, 0, 0, TimeSpan.Zero);
                    if (afterCategory != null) monthAfter = monthAfter.OffsetMonthBy(afterCategory.IncomeMonthOffset);
                    var month = monthBefore < monthAfter ? monthBefore : monthAfter;
                    await _budgetOrm.UpdateGlobalPotCacheAsync(month)
                        .ConfigureAwait(false);
                }
                // Both sides stay assigned to a not income-relevant category
                else if (beforeCategory != null
                         && beforeCategory.IsIncomeRelevant.Not()
                         && afterCategory != null
                         && afterCategory.IsIncomeRelevant.Not())
                {
                    var monthBefore =
                        new DateTimeOffset(beforeDate.Year, beforeDate.Month, 1, 0, 0, 0, TimeSpan.Zero);
                    var monthAfter = new DateTimeOffset(afterDate.Year, afterDate.Month, 1, 0, 0, 0, TimeSpan.Zero);
                    var month = monthBefore < monthAfter ? monthBefore : monthAfter;

                    await _budgetOrm.UpdateCategorySpecificCacheAsync(
                            afterCategory,
                            month)
                        .ConfigureAwait(false);
                    if (beforeCategory != afterCategory)
                        await _budgetOrm.UpdateCategorySpecificCacheAsync(
                                beforeCategory,
                                month)
                            .ConfigureAwait(false);
                }
                else
                {
                    var globalDate = DateTimeOffset.MaxValue;
                    if (beforeCategory is null || beforeCategory.IsIncomeRelevant)
                    {
                        globalDate = new DateTimeOffset(beforeDate.Year, beforeDate.Month, 1, 0, 0, 0, TimeSpan.Zero);
                        if (beforeCategory != null && beforeCategory.IsIncomeRelevant && beforeCategory.IncomeMonthOffset != 0)
                            globalDate = globalDate.OffsetMonthBy(beforeCategory.IncomeMonthOffset);
                    }
                    if (afterCategory is null || afterCategory.IsIncomeRelevant)
                    {
                        globalDate = new DateTimeOffset(afterDate.Year, afterDate.Month, 1, 0, 0, 0, TimeSpan.Zero);
                        if (afterCategory != null && afterCategory.IsIncomeRelevant && afterCategory.IncomeMonthOffset != 0)
                            globalDate = globalDate.OffsetMonthBy(afterCategory.IncomeMonthOffset);
                    }
                    await _budgetOrm.UpdateGlobalPotCacheAsync(globalDate)
                        .ConfigureAwait(false);
                    if (afterCategory != null && afterCategory.IsIncomeRelevant.Not())
                    {
                        var monthAfter = new DateTimeOffset(afterDate.Year, afterDate.Month, 1, 0, 0, 0, TimeSpan.Zero);
                        await _budgetOrm.UpdateCategorySpecificCacheAsync(
                                afterCategory,
                                monthAfter)
                            .ConfigureAwait(false);
                    }

                    if (beforeCategory != null && beforeCategory.IsIncomeRelevant.Not())
                    {
                        var monthBefore =
                            new DateTimeOffset(beforeDate.Year, beforeDate.Month, 1, 0, 0, 0, TimeSpan.Zero);
                        await _budgetOrm.UpdateCategorySpecificCacheAsync(
                                beforeCategory,
                                monthBefore)
                            .ConfigureAwait(false);
                    }
                }
            }
        }

        public Task OnTransferInsertOrDeleteAsync(
            Models.Persistence.Account fromAccount, 
            Models.Persistence.Account toAccount, 
            DateTimeOffset date)
        {
            if (fromAccount is null && toAccount is null
                || fromAccount != null && toAccount != null) return Task.CompletedTask;

            var month = new DateTimeOffset(
                date.Year,
                date.Month,
                1, 0, 0, 0, TimeSpan.Zero);
            return _budgetOrm.UpdateGlobalPotCacheAsync(month);
        }

        public Task OnTransferUpdateAsync(
            Models.Persistence.Account beforeFromAccount, 
            Models.Persistence.Account beforeToAccount, 
            DateTimeOffset beforeDate,
            long beforeSum, 
            Models.Persistence.Account afterFromAccount,
            Models.Persistence.Account afterToAccount, 
            DateTimeOffset afterDate, 
            long afterSum)
        {
            var sumChanged = beforeSum != afterSum;
            var monthChanged = 
                beforeDate != afterDate 
                && (beforeDate.Year != afterDate.Year || beforeDate.Month != afterDate.Month);
            var remainedBudgetRelevantState =
                beforeFromAccount == afterFromAccount 
                && beforeToAccount == afterToAccount
                && (afterFromAccount is null && afterToAccount != null
                    || afterFromAccount != null && afterToAccount is null);

            var changedAnAccountToNullOrInverse =
                beforeFromAccount is null && afterFromAccount != null
                || beforeToAccount is null && afterToAccount != null
                || beforeFromAccount != null && afterFromAccount is null
                || beforeToAccount != null && afterToAccount is null;

            // if a budget relevant property changed then update caches
            if ((sumChanged || monthChanged) && remainedBudgetRelevantState
                || changedAnAccountToNullOrInverse)
            {
                var monthBefore = new DateTimeOffset(beforeDate.Year, beforeDate.Month, 1, 0, 0, 0, TimeSpan.Zero);
                var monthAfter = new DateTimeOffset(afterDate.Year, afterDate.Month, 1, 0, 0, 0, TimeSpan.Zero);
                var month = monthBefore < monthAfter ? monthBefore : monthAfter;
                return _budgetOrm.UpdateGlobalPotCacheAsync(month);
            }
            return Task.CompletedTask;
        }

        public Task OnCategoryDeletion(Models.Persistence.Category category)
        {
            return _budgetOrm
                .DeleteCategorySpecificCacheAsync(category);
        }

        public Task OnCategoryMergeForTarget(Models.Persistence.Category category)
        {
            return _budgetOrm
                .UpdateCategorySpecificCacheAsync(category, DateTimeOffset.MinValue);
        }

        public Task OnIncomeCategoryChange(
            Models.Persistence.Category category,
            int beforeIncomeMonthOffset,
            int afterIncomeMonthOffset)
        {
            if (beforeIncomeMonthOffset != afterIncomeMonthOffset)
            {
                return _budgetOrm
                    .UpdateCategorySpecificCacheAsync(category, DateTimeOffset.MinValue);
            }
            return Task.CompletedTask;
        }

        public Task OnAccountChange(
            DateTimeOffset beforeStartingDate,
            long beforeStartingBalance,
            DateTimeOffset afterStartingDate,
            long afterStartingBalance)
        {
            if (beforeStartingDate == afterStartingDate && beforeStartingBalance == afterStartingBalance)
                return Task.CompletedTask;

            var minDate = beforeStartingDate < afterStartingDate ? beforeStartingDate : afterStartingDate;
            var month = new DateTimeOffset(minDate.Year, minDate.Month, 1, 0, 0, 0, TimeSpan.Zero);
            return _budgetOrm.UpdateGlobalPotCacheAsync(month);
        }

        public Task OnAccountInsertOrDelete(DateTimeOffset startingDate)
        {
            var month = new DateTimeOffset(startingDate.Year, startingDate.Month, 1, 0, 0, 0, TimeSpan.Zero);
            return _budgetOrm.UpdateGlobalPotCacheAsync(month);
        }

        public Task OnBudgetEntryChange(Models.Persistence.Category category, DateTimeOffset date)
        {
            return _budgetOrm.UpdateCategorySpecificCacheAsync(
                category,
                date);
        }

        public Task OnTransactionInsertOrDeleteAsync(Models.Persistence.Category category, DateTimeOffset date)
        {
            var month = new DateTimeOffset(
                date.Year,
                date.Month,
                1, 0, 0, 0, TimeSpan.Zero);

            if (category != null && category.IsIncomeRelevant && category.IncomeMonthOffset != 0)
                month = month.OffsetMonthBy(category.IncomeMonthOffset);

            return category is null || category.IsIncomeRelevant
                ? _budgetOrm.UpdateGlobalPotCacheAsync(month)
                : _budgetOrm.UpdateCategorySpecificCacheAsync(category, month);
        }
    }
}
