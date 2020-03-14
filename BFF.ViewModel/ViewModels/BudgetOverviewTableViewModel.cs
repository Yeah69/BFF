using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.DataVirtualizingCollection.SlidingWindow;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.ViewModel.Extensions;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels;
using MrMeeseeks.Extensions;
using MuVaViMo;

namespace BFF.ViewModel.ViewModels
{
    public interface
        IBudgetOverviewTableViewModel : ITableViewModel<IBudgetMonthViewModel, ICategoryViewModel, IBudgetEntryViewModel>
    {

        IRxRelayCommand IncreaseMonthStartIndex { get; }

        IRxRelayCommand DecreaseMonthStartIndex { get; }
    }
    public interface
        IBudgetOverviewTableRowViewModel : ITableRowViewModel<ICategoryViewModel, IBudgetEntryViewModel>
    {
        void SlideLeft();
        void SlideRight();
        void JumpTo(int index);
    }

    internal class BudgetOverviewTableViewModel : ObservableObject,
        IBudgetOverviewTableViewModel,
        IDisposable
    {
        private static readonly int LastMonthIndex = BudgetOverviewViewModel.MonthToIndex(DateTime.MaxValue);
        
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private readonly ISlidingWindow<IBudgetMonthViewModel> _budgetMonths;

        private int _currentMonthStartIndex = BudgetOverviewViewModel.MonthToIndex(DateTime.Now) - 41;

        public BudgetOverviewTableViewModel(
            IRxSchedulerProvider rxSchedulerProvider,
            ICategoryViewModelService categoryViewModelService,
            IBudgetMonthRepository budgetMonthRepository,
            Func<IBudgetMonth, IBudgetMonthViewModel> budgetMonthViewModelFactory)
        {
            ColumnCount = 5;
            
            var rows = categoryViewModelService
                .All
                .Transform(cvm => 
                    new BudgetOverviewTableRowViewModel(
                        cvm,
                        (ColumnCount, _currentMonthStartIndex)));

            Rows = rows;

            _budgetMonths = SlidingWindowBuilder<IBudgetMonthViewModel>
                .Build(pageSize: 12, initialOffset: _currentMonthStartIndex, windowSize: ColumnCount,
                    notificationScheduler: rxSchedulerProvider.UI)
                .Preloading()
                .Hoarding()
                .TaskBasedFetchers(
                    async (offset, pageSize) =>
                    {
                        var budgetMonthViewModels = await Task.Run(async () =>
                            (await budgetMonthRepository.FindAsync(BudgetOverviewViewModel.IndexToMonth(offset).Year)
                                .ConfigureAwait(false))
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
                //.SyncIndexAccess()
                .AsyncIndexAccess(
                     (pageKey, pageIndex) => 
                         new BudgetMonthViewModelPlaceholder(BudgetOverviewViewModel.IndexToMonth(pageKey * 12 + pageIndex)),
                     rxSchedulerProvider.Task)
                .AddForDisposalTo(_compositeDisposable);

            ISubject<int> currentMonthStartIndexChanged = new Subject<int>().AddForDisposalTo(_compositeDisposable);
            
            IncreaseMonthStartIndex = currentMonthStartIndexChanged
                .Select(i => i < LastMonthIndex - ColumnCount + 1)
                .ToRxRelayCommand(() =>
                {
                    _budgetMonths.SlideRight();
                    foreach (var row in rows)
                    {
                        row.SlideRight();
                    }

                    _currentMonthStartIndex++;
                    currentMonthStartIndexChanged.OnNext(_currentMonthStartIndex);
                })
                .AddForDisposalTo(_compositeDisposable);
            
            DecreaseMonthStartIndex = currentMonthStartIndexChanged
                .Select(i => i > 0)
                .ToRxRelayCommand(() =>
                {
                    _budgetMonths.SlideLeft();
                    foreach (var row in rows)
                    {
                        row.SlideLeft();
                    }

                    _currentMonthStartIndex--;
                    currentMonthStartIndexChanged.OnNext(_currentMonthStartIndex);
                })
                .AddForDisposalTo(_compositeDisposable);
        }

        public IReadOnlyList<IBudgetMonthViewModel> ColumnHeaders => _budgetMonths;
        public IReadOnlyList<ITableRowViewModel<ICategoryViewModel, IBudgetEntryViewModel>> Rows { get; }
        public int ColumnCount { get; }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }

        public IRxRelayCommand IncreaseMonthStartIndex { get; }
        
        public IRxRelayCommand DecreaseMonthStartIndex { get; }
    }

    internal class BudgetOverviewTableRowViewModel : IBudgetOverviewTableRowViewModel
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

        public IReadOnlyList<IBudgetEntryViewModel> Cells => _budgetCategoryViewModel.BudgetEntries;
        public void SlideLeft()
        {
            _budgetCategoryViewModel.BudgetEntries.SlideLeft();
        }

        public void SlideRight()
        {
            _budgetCategoryViewModel.BudgetEntries.SlideRight();
        }

        public void JumpTo(int index)
        {
            _budgetCategoryViewModel.BudgetEntries.JumpTo(index);
        }
    }
}
