using System;
using System.Linq;
using System.Reactive.Disposables;
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
        ICategoryViewModel Category { get; }
        ISlidingWindow<IBudgetEntryViewModel> BudgetEntries { get; }
    }

    internal sealed class BudgetCategoryViewModel : ViewModelBase, IBudgetCategoryViewModel, IDisposable, ITransient
    {
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public BudgetCategoryViewModel(
            // parameter
            IBudgetCategory budgetCategory,
            (int EntryCount, int MonthOffset) initial,
            
            // dependencies
            ICategoryViewModelService categoryViewModelService,
            IBudgetEntryViewModelService budgetEntryViewModelService,
            IRxSchedulerProvider rxSchedulerProvider)
        {
            Category = categoryViewModelService.GetViewModel(budgetCategory.Category);

            BudgetEntries = SlidingWindowBuilder<IBudgetEntryViewModel>
                .Build(initial.EntryCount, initial.MonthOffset, rxSchedulerProvider.UI, 12)
                .Preloading()
                .LeastRecentlyUsed(4)
                .TaskBasedFetchers(
                    async (page, _) =>
                        (await budgetCategory
                            .GetBudgetEntriesFor(BudgetOverviewViewModel.IndexToMonth(page).Year)
                            .ConfigureAwait(false))
                        .Select(budgetEntryViewModelService.GetViewModel)
                        .ToArray(),
                    () =>
                        Task.FromResult(
                            (DateTime.MaxValue.Year - DateTime.MinValue.Year - 2) * 12
                            + 12 - DateTime.MinValue.Month - 1 +
                            DateTime.MaxValue.Month))
                //.SyncIndexAccess()
                .AsyncIndexAccess((_, __) => BudgetEntryViewModelPlaceholder.Instance, rxSchedulerProvider.Task)
                .AddForDisposalTo(_compositeDisposable);
        }

        public ICategoryViewModel Category { get; }
        
        public ISlidingWindow<IBudgetEntryViewModel> BudgetEntries { get; }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}
