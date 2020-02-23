using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.DataVirtualizingCollection.SlidingWindow;
using BFF.ViewModel.ViewModels.ForModels;
using MrMeeseeks.Extensions;

namespace BFF.ViewModel.ViewModels
{
    public interface IBudgetOverviewTableViewModel : ITableViewModel<IBudgetMonthViewModel, ICategoryViewModel, IBudgetEntryViewModel>
    { }

    internal class BudgetOverviewTableViewModel : ObservableObject,
        IBudgetOverviewTableViewModel,
        IDisposable
    {
        private readonly ISlidingWindow<IBudgetMonthViewModel> _budgetMonthSlidingWindow;

        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public BudgetOverviewTableViewModel(
            // parameters
            ISlidingWindow<IBudgetMonthViewModel> budgetMonthSlidingWindow,

            // dependencies
            IRxSchedulerProvider rxSchedulerProvider)
        {
            _budgetMonthSlidingWindow = budgetMonthSlidingWindow;

            _budgetMonthSlidingWindow
                .ObservePropertyChanges(nameof(ICollection.Count))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => 
                {
                    OnPropertyChanged(nameof(ColumnCount));
                    RefreshRows();
                })
                .AddForDisposalTo(_compositeDisposable);

            _budgetMonthSlidingWindow
                .ObserveCollectionChanges()
                .Subscribe(_ => RefreshRows())
                .AddForDisposalTo(_compositeDisposable);

            void RefreshRows()
            {
                Rows = budgetMonthSlidingWindow
                    .Select((budgetMonth, i) => (budgetMonth, i))
                    .SelectMany(t => t.budgetMonth.BudgetEntries.Select(budgetEntry => (budgetEntry, t.i)))
                    .GroupBy(t => t.budgetEntry.Category)
                    .Select(g =>
                        new BudgetOverviewTableRowViewModel(g.Key,
                            g.OrderBy(t => t.i).Select(t => t.budgetEntry).ToArray()))
                    .ToReadOnlyList();
                OnPropertyChanged(nameof(ColumnHeaders));
                OnPropertyChanged(nameof(Rows));
            }
        }

        public IReadOnlyList<IBudgetMonthViewModel> ColumnHeaders => _budgetMonthSlidingWindow;
        public IReadOnlyList<ITableRowViewModel<ICategoryViewModel, IBudgetEntryViewModel>> Rows { get; private set; }
        public int ColumnCount => ((ICollection)_budgetMonthSlidingWindow).Count;

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }

    internal class BudgetOverviewTableRowViewModel : ITableRowViewModel<ICategoryViewModel, IBudgetEntryViewModel>
    {
        private readonly IBudgetEntryViewModel[] _budgetEntryViewModels;

        public BudgetOverviewTableRowViewModel(
            ICategoryViewModel rowHeader,
            IBudgetEntryViewModel[] budgetEntryViewModels)
        {
            _budgetEntryViewModels = budgetEntryViewModels;
            RowHeader = rowHeader;
        }

        public ICategoryViewModel RowHeader { get; }

        public IBudgetEntryViewModel this[int index] => _budgetEntryViewModels[index];
    }
}
