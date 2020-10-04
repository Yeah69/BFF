using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.DataVirtualizingCollection.SlidingWindow;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.ViewModel.Extensions;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Managers;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels;
using MrMeeseeks.Extensions;
using MrMeeseeks.Reactive.Extensions;
using MuVaViMo;

namespace BFF.ViewModel.ViewModels
{
    public interface
        IBudgetOverviewTableViewModel : ITableViewModel<IBudgetMonthViewModel, ICategoryViewModel, IBudgetEntryViewModel>, IObservableObject
    {
        public long AvailableToBudgetInCurrentMonth { get; }

        IRxRelayCommand IncreaseMonthStartIndex { get; }

        IRxRelayCommand DecreaseMonthStartIndex { get; }
        
        bool ShowAggregates { get; set; }
        
        IMonthViewModel SelectedMonthViewModel { get; }
        
        IReadOnlyList<IMonthViewModel> MonthViewModels { get; }
        
        int SelectedYear { get; }
    }
    public interface
        IBudgetOverviewTableRowViewModel : ITableRowViewModel<ICategoryViewModel, IBudgetEntryViewModel>, IObservableObject
    {
        void JumpTo(int index);
    }

    internal class BudgetOverviewTableViewModel : ObservableObject,
        IBudgetOverviewTableViewModel,
        IDisposable
    {
        private static readonly int LastMonthIndex = DateTime.MaxValue.ToMonthIndex();
        
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private readonly ISlidingWindow<IBudgetMonthViewModel> _budgetMonths;

        private int _monthIndexOffset;
        private bool _showAggregates;
        private readonly IObservableReadOnlyList<IBudgetOverviewTableRowViewModel> _rows;

        public BudgetOverviewTableViewModel(
            IRxSchedulerProvider rxSchedulerProvider,
            Func<int, IMonthViewModel> monthViewModelFactory,
            ICategoryViewModelService categoryViewModelService,
            IBackendCultureManager cultureManager,
            IBudgetMonthRepository budgetMonthRepository,
            IBudgetRefreshes budgetRefreshes,
            Func<IBudgetMonth, IBudgetMonthViewModel> budgetMonthViewModelFactory,
            Func<ICategoryViewModel, (int EntryCount, int MonthOffset), IBudgetOverviewTableRowViewModel> budgetOverviewTableRowViewModelFactory)
        {
            this.MonthViewModels = Enumerable.Range(1, 12).Select(monthViewModelFactory).ToReadOnlyList();
            
            ColumnCount = 5;
            _monthIndexOffset = DateTime.Now.ToMonthIndex() - 41;
            
            _rows = categoryViewModelService
                .All
                .Transform(cvm => 
                    budgetOverviewTableRowViewModelFactory(
                        cvm,
                        (ColumnCount, MonthIndexOffset)));

            _budgetMonths = SlidingWindowBuilder
                .Build<IBudgetMonthViewModel>(
                    pageSize: 12, 
                    initialOffset: MonthIndexOffset, 
                    windowSize: ColumnCount,
                    notificationScheduler: rxSchedulerProvider.UI,
                    backgroundScheduler: rxSchedulerProvider.Task)
                .Preloading((pageKey, pageIndex) =>
                    new BudgetMonthViewModelPlaceholder(DateTimeExtensions.FromMonthIndex(pageKey * 12 + pageIndex)))
                .Hoarding()
                .TaskBasedFetchers(
                    (offset, pageSize) => 
                        Task.Run(async () =>
                            (await budgetMonthRepository.FindAsync(DateTimeExtensions.FromMonthIndex(offset).Year)
                                .ConfigureAwait(false))
                            .Select(budgetMonthViewModelFactory)
                            .ToArray()),
                    () => Task.FromResult(DateTimeExtensions.CountOfMonths))
                .AsyncIndexAccess(
                     (pageKey, pageIndex) => 
                         new BudgetMonthViewModelPlaceholder(DateTimeExtensions.FromMonthIndex(pageKey * 12 + pageIndex)))
                .AddForDisposalTo(_compositeDisposable);
            
            IncreaseMonthStartIndex = this.ObservePropertyChanged(nameof(MonthIndexOffset))
                .Select(_ => MonthIndexOffset < LastMonthIndex - ColumnCount + 1)
                .ToRxRelayCommand(() => MonthIndexOffset++)
                .AddForDisposalTo(_compositeDisposable);
            
            DecreaseMonthStartIndex = this.ObservePropertyChanged(nameof(MonthIndexOffset))
                .Select(_ => MonthIndexOffset > 0)
                .ToRxRelayCommand(() => MonthIndexOffset--)
                .AddForDisposalTo(_compositeDisposable);

            budgetRefreshes
                .ObserveHeadersRefreshes
                .Subscribe(_ => _budgetMonths.Reset())
                .AddForDisposalTo(_compositeDisposable);

            budgetRefreshes
                .ObserveCompleteRefreshes
                .Subscribe(_ => Reset())
                .AddForDisposalTo(_compositeDisposable);

            budgetRefreshes
                .ObserveHeadersRefreshes
                .Merge(budgetRefreshes.ObserveCompleteRefreshes)
                .SelectMany(_ => budgetMonthRepository.GetAvailableToBudgetOfCurrentMonth())
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(availableToBudgetInCurrentMonth =>
                    AvailableToBudgetInCurrentMonth = availableToBudgetInCurrentMonth);

            cultureManager.RefreshSignal.Subscribe(message =>
            {
                switch (message)
                {
                    case CultureMessage.Refresh:
                        break;
                    case CultureMessage.RefreshDate:
                    case CultureMessage.RefreshCurrency:
                        this.Reset();
                        break;
                    default:
                        throw new InvalidEnumArgumentException();
                }
            }).AddTo(_compositeDisposable);

            SetAvailableToBudgetInCurrentMonth();

            async Task SetAvailableToBudgetInCurrentMonth() => 
                AvailableToBudgetInCurrentMonth = await budgetMonthRepository.GetAvailableToBudgetOfCurrentMonth();
        }

        public IMonthViewModel SelectedMonthViewModel
        {
            get => this.MonthViewModels[this.MonthIndexOffset % 12];
            set => this.MonthIndexOffset = (this.SelectedYear - 1) * 12 + value.Number - 1;
        }
        
        public IReadOnlyList<IMonthViewModel> MonthViewModels { get; }
        public int SelectedYear 
        {
            get => this.MonthIndexOffset / 12 + 1;
            set => this.MonthIndexOffset = (value - 1) * 12 + this.SelectedMonthViewModel.Number - 1;
        }

        public IList<IBudgetMonthViewModel> ColumnHeaders => _budgetMonths;
        public IReadOnlyList<ITableRowViewModel<ICategoryViewModel, IBudgetEntryViewModel>> Rows => 
            this._rows;
        public int ColumnCount { get; }

        public void Dispose() => 
            _compositeDisposable.Dispose();

        public int MonthIndexOffset
        {
            get => _monthIndexOffset;
            set
            {
                JumpTo(value);
                SetIfChangedAndRaise(ref _monthIndexOffset, value);
                
                void JumpTo(int index)
                {
                    _budgetMonths.JumpTo(index);
                    foreach (var row in _rows)
                    {
                        row.JumpTo(index);
                    }
                }
                OnPropertyChanged(nameof(SelectedYear), nameof(SelectedMonthViewModel));
            }
        }

        private long _availableToBudgetInCurrentMonth;
        
        public long AvailableToBudgetInCurrentMonth 
        { 
            get => _availableToBudgetInCurrentMonth; 
            private set => SetIfChangedAndRaise(ref _availableToBudgetInCurrentMonth, value); 
        }

        public IRxRelayCommand IncreaseMonthStartIndex { get; }
        
        public IRxRelayCommand DecreaseMonthStartIndex { get; }

        public bool ShowAggregates
        {
            get => _showAggregates;
            set => SetIfChangedAndRaise(ref _showAggregates, value);
        }

        public void Reset()
        {
            _budgetMonths.Reset();
            foreach (var row in this.Rows)
            {
                row.Reset();
            }
        }
    }

    internal class BudgetOverviewTableRowViewModel : ObservableObject, IBudgetOverviewTableRowViewModel, IDisposable
    {
        private readonly IBudgetCategoryViewModel _budgetCategoryViewModel;

        private readonly IDisposable _compositeDisposable;

        public BudgetOverviewTableRowViewModel(
            // parameters
            ICategoryViewModel rowHeader,
            (int EntryCount, int MonthOffset) initial,
            
            // dependencies
            IBudgetRefreshes budgetRefreshes)
        {
            RowHeader = rowHeader;
            _budgetCategoryViewModel = rowHeader.CreateBudgetCategory(initial.EntryCount, initial.MonthOffset);

            _compositeDisposable = budgetRefreshes.ObserveCategoryRefreshes(rowHeader).Subscribe(_ => this.Reset());
        }

        public ICategoryViewModel RowHeader { get; }

        public IList<IBudgetEntryViewModel> Cells => 
            _budgetCategoryViewModel.BudgetEntries;

        public void JumpTo(int index) => 
            _budgetCategoryViewModel.BudgetEntries.JumpTo(index);

        public void Reset() => 
            _budgetCategoryViewModel.BudgetEntries.Reset();

        public void Dispose() => 
            _compositeDisposable.Dispose();
    }
}
