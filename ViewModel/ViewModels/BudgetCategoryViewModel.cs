using System;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Core.IoC;
using BFF.DataVirtualizingCollection.SlidingWindow;
using BFF.Model.Models;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels;
using MrMeeseeks.Extensions;

namespace BFF.ViewModel.ViewModels
{
    public interface IBudgetCategoryViewModel
    {
        ISlidingWindow<IBudgetEntryViewModel> BudgetEntries { get; }
    }

    internal sealed class BudgetCategoryViewModel : ViewModelBase, IBudgetCategoryViewModel, IAsyncDisposable, ITransient
    {
        private readonly IAsyncDisposable _disposable;

        public BudgetCategoryViewModel(
            // parameter
            IBudgetCategory budgetCategory,
            (int EntryCount, int MonthOffset) initial,
            
            // dependencies
            IBudgetEntryViewModelService budgetEntryViewModelService,
            IRxSchedulerProvider rxSchedulerProvider)
        {
            BudgetEntries = SlidingWindowBuilder
                .Build<IBudgetEntryViewModel>(
                    initial.EntryCount, 
                    initial.MonthOffset, 
                    12,
                    rxSchedulerProvider.UI,
                    rxSchedulerProvider.Task)
                .Preloading((_, __) => BudgetEntryViewModelPlaceholder.Instance)
                .LeastRecentlyUsed(4)
                .TaskBasedFetchers(
                    async (page, _, _) =>
                        (await budgetCategory
                            .GetBudgetEntriesFor(DateTimeExtensions.FromMonthIndex(page).Year)
                            .ConfigureAwait(false))
                        .Select(budgetEntryViewModelService.GetViewModel)
                        .WhereNotNullRef()
                        .ToArray(),
                    _ =>
                        Task.FromResult(
                            (DateTime.MaxValue.Year - DateTime.MinValue.Year - 2) * 12
                            + 12 - DateTime.MinValue.Month - 1 +
                            DateTime.MaxValue.Month))
                .AsyncIndexAccess((_, __) => BudgetEntryViewModelPlaceholder.Instance);
            _disposable = BudgetEntries;
        }
        
        public ISlidingWindow<IBudgetEntryViewModel> BudgetEntries { get; }

        public ValueTask DisposeAsync() => _disposable.DisposeAsync();
    }
}
