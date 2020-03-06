using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.DataVirtualizingCollection.SlidingWindow;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels;
using MrMeeseeks.Extensions;
using MuVaViMo;

namespace BFF.ViewModel.ViewModels
{
    public interface IBudgetOverviewTableViewModel : ITableViewModel<IBudgetMonthViewModel, ICategoryViewModel, IBudgetEntryViewModel>
    { }

    internal class BudgetOverviewTableViewModel : ObservableObject,
        IBudgetOverviewTableViewModel,
        IDisposable
    {
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private ISlidingWindow<IBudgetMonthViewModel> _budgetMonths;

        public BudgetOverviewTableViewModel(
            // parameter
            (int ColumnCount, int MonthOffset) initial,
            
            // dependencies
            IRxSchedulerProvider rxSchedulerProvider,
            ICategoryViewModelService categoryViewModelService,
            IBudgetMonthRepository budgetMonthRepository,
            Func<IBudgetMonth, IBudgetMonthViewModel> budgetMonthViewModelFactory)
        {
            Rows = categoryViewModelService
                .All
                .Transform(cvm => 
                    new BudgetOverviewTableRowViewModel(
                        cvm, 
                        initial));
            
            _budgetMonths = SlidingWindowBuilder<IBudgetMonthViewModel>
                .Build(pageSize: 12, initialOffset: initial.MonthOffset, windowSize: initial.ColumnCount, notificationScheduler: rxSchedulerProvider.UI)
                .NonPreloading()
                .Hoarding()
                .TaskBasedFetchers(
                    async (offset, pageSize) =>
                    {
                        var budgetMonthViewModels = await Task.Run(async () => (await budgetMonthRepository.FindAsync(BudgetOverviewViewModel.IndexToMonth(offset).Year).ConfigureAwait(false))
                            .Select(budgetMonthViewModelFactory)
                            .ToArray()).ConfigureAwait(false);

                        // foreach (var bmvm in budgetMonthViewModels)
                        // {
                        //     var categoriesToBudgetEntries = bmvm.BudgetEntries.ToDictionary(bevm => bevm.Category, bevm => bevm);
                        //     foreach (var bevm in bmvm.BudgetEntries)
                        //     {
                        //         bevm.Children = bevm
                        //             .Category
                        //             .Categories
                        //             .Select(cvm => categoriesToBudgetEntries[cvm]).ToList();
                        //     }
                        // }

                        return budgetMonthViewModels;
                    },
                    () => Task.FromResult(DateTimeExtensions.CountOfMonths()))
                .AsyncIndexAccess(
                    (pageKey, pageIndex) => 
                        new BudgetMonthViewModelPlaceholder(BudgetOverviewViewModel.IndexToMonth(pageKey * 12 + pageIndex)),
                    rxSchedulerProvider.Task);

            ColumnCount = initial.ColumnCount;
        }

        public IReadOnlyList<IBudgetMonthViewModel> ColumnHeaders => _budgetMonths;
        public IReadOnlyList<ITableRowViewModel<ICategoryViewModel, IBudgetEntryViewModel>> Rows { get; }
        public int ColumnCount { get; }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }

    internal class BudgetOverviewTableRowViewModel : ITableRowViewModel<ICategoryViewModel, IBudgetEntryViewModel>
    {
        private readonly IBudgetCategoryViewModel _budgetCategoryViewModel;

        public BudgetOverviewTableRowViewModel(
            ICategoryViewModel rowHeader,
            (int EntryCount, int MonthOffset) initial)
        {
            RowHeader = rowHeader;
            _budgetCategoryViewModel = rowHeader.CreateBudgetCategory(initial.EntryCount, initial.MonthOffset);
        }

        public ICategoryViewModel RowHeader { get; }

        public IBudgetEntryViewModel this[int index] => ((IReadOnlyList<IBudgetEntryViewModel>) _budgetCategoryViewModel.BudgetEntries)[index];
    }
}
