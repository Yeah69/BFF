using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.Core.IoC;
using BFF.DataVirtualizingCollection;
using BFF.DataVirtualizingCollection.DataVirtualizingCollections;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.ViewModel.Extensions;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Managers;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels;
using BFF.ViewModel.ViewModels.ForModels.Utility;
using MrMeeseeks.Extensions;
using MuVaViMo;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.ViewModel.ViewModels
{
    public interface IBudgetOverviewViewModel
    {
        IList<IBudgetMonthViewModel> BudgetMonths { get; }

        IBudgetMonthViewModel CurrentBudgetMonth { get; }

        int CurrentMonthStartIndex { get; }

        bool IsOpen { get; set; }
        int SelectedIndex { get; set; }

        DateTime SelectedMonth { get; set; }

        IRxRelayCommand IncreaseMonthStartIndex { get; }

        IRxRelayCommand DecreaseMonthStartIndex { get; }

        ITransDataGridColumnManager TransDataGridColumnManager { get; }

        Task Refresh();

        IDisposable DeferRefreshUntilDisposal();

        IBudgetMonthViewModel GetBudgetMonthViewModel(DateTime month);

        ReadOnlyReactiveCollection<ICategoryViewModel> Categories { get; }

        IBudgetMonthMenuTitles BudgetMonthMenuTitles { get; }
    }

    internal class BudgetOverviewViewModel : ViewModelBase, IBudgetOverviewViewModel, IOncePerBackend, IDisposable
    {
        private static readonly int LastMonthIndex = MonthToIndex(DateTime.MaxValue);

        private readonly IBudgetMonthRepository _budgetMonthRepository;
        private readonly IBffSettings _bffSettings;
        private readonly Func<IBudgetMonth, IBudgetMonthViewModel> _budgetMonthViewModelFactory;
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private int _selectedIndex;
        private int _currentMonthStartIndex;
        private bool _isOpen;
        private bool _canRefresh = true;
        private DateTime _selectedMonth;
        private IDataVirtualizingCollection<IBudgetMonthViewModel> _budgetMonths;

        public IList<IBudgetMonthViewModel> BudgetMonths =>
            _budgetMonths ??= CreateBudgetMonths();

        public IBudgetMonthViewModel CurrentBudgetMonth => BudgetMonths[MonthToIndex(DateTime.Now)];

        public ReadOnlyReactiveCollection<ICategoryViewModel> Categories { get; }
        public IBudgetMonthMenuTitles BudgetMonthMenuTitles { get; }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex == value) return;
                _selectedIndex = value;
                OnPropertyChanged();
            }
        }

        public DateTime SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                if (_selectedMonth == value) return;
                _selectedMonth = value;
                CurrentMonthStartIndex = MonthToIndex(_selectedMonth);
                OnPropertyChanged();
            }
        }

        public int CurrentMonthStartIndex
        {
            get => _currentMonthStartIndex;
            set
            {
                if (_currentMonthStartIndex == value) return;
                _currentMonthStartIndex = value;
                SelectedMonth = IndexToMonth(_currentMonthStartIndex);
                OnPropertyChanged();
            }
        }

        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                if (_isOpen == value) return;
                _isOpen = value;
                OnPropertyChanged();
                if (_isOpen)
                {
                    _bffSettings.OpenMainTab = "BudgetOverview";
                }
            }
        }

        public IRxRelayCommand IncreaseMonthStartIndex { get; }

        public IRxRelayCommand DecreaseMonthStartIndex { get; }
        public ITransDataGridColumnManager TransDataGridColumnManager { get; }

        public BudgetOverviewViewModel(
            IBudgetMonthRepository budgetMonthRepository,
            ICultureManager cultureManager,
            IBffSettings bffSettings,
            Func<IBudgetMonth, IBudgetMonthViewModel> budgetMonthViewModelFactory,
            IBudgetMonthMenuTitles budgetMonthMenuTitles,
            ITransDataGridColumnManager transDataGridColumnManager,
            IRxSchedulerProvider rxSchedulerProvider,
            ICategoryViewModelService categoryViewModelService,
            ICategoryRepository categoryRepository)
        {
            BudgetMonthMenuTitles = budgetMonthMenuTitles;
            TransDataGridColumnManager = transDataGridColumnManager;
            _budgetMonthRepository = budgetMonthRepository;
            _bffSettings = bffSettings;
            _budgetMonthViewModelFactory = budgetMonthViewModelFactory;
            _rxSchedulerProvider = rxSchedulerProvider;

            SelectedIndex = -1;

            Categories =
                categoryRepository
                    .All
                    .ToReadOnlyObservableCollection()
                    .ToReadOnlyReactiveCollection(categoryViewModelService.GetViewModel);

            CurrentMonthStartIndex = MonthToIndex(DateTime.Now) - 1;

            var currentMonthStartIndexChanges = this
                .ObservePropertyChanges(nameof(CurrentMonthStartIndex))
                .Select(_ => CurrentMonthStartIndex);

            IncreaseMonthStartIndex = currentMonthStartIndexChanges
                .Select(i => i < LastMonthIndex - 1)
                .ToRxRelayCommand(() => CurrentMonthStartIndex = CurrentMonthStartIndex + 1)
                .AddTo(_compositeDisposable);
            DecreaseMonthStartIndex = currentMonthStartIndexChanges
                .Select(i => i < LastMonthIndex - 1)
                .ToRxRelayCommand(
                    () => CurrentMonthStartIndex = CurrentMonthStartIndex - 1)
                .AddTo(_compositeDisposable);

            IsOpen = _bffSettings.OpenMainTab == "BudgetOverview";

            this
                .ObservePropertyChanges(nameof(IsOpen))
                .Where(_ => IsOpen)
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(b => Task.Factory.StartNew(Refresh))
                .AddTo(_compositeDisposable);

            cultureManager.RefreshSignal.Subscribe(message =>
            {
                switch (message)
                {
                    case CultureMessage.Refresh:
                        break;
                    case CultureMessage.RefreshCurrency:
                    case CultureMessage.RefreshDate:
                        Task.Factory.StartNew(Refresh);
                        break;
                    default:
                        throw new InvalidEnumArgumentException();
                }
            }).AddTo(_compositeDisposable);

            Disposable
                .Create(() => _budgetMonths?.Dispose())
                .AddTo(_compositeDisposable);
        }

        public async Task Refresh()
        {
            if (_canRefresh.Not()) return;
            var temp = _budgetMonths;
            _budgetMonths = await Task.Run(() => CreateBudgetMonths());
            _rxSchedulerProvider.UI.MinimalSchedule(() => OnPropertyChanged(nameof(BudgetMonths)));
            await Task.Run(() => temp.Dispose());
        }

        public IDisposable DeferRefreshUntilDisposal()
        {
            _canRefresh = false;
            return Disposable.Create(async () =>
            {
                _canRefresh = true;
                await Refresh();
            });
        }

        public IBudgetMonthViewModel GetBudgetMonthViewModel(DateTime month)
        {
            var index = MonthToIndex(month);
            return index < 0 ? null : BudgetMonths[index];
        }

        private IDataVirtualizingCollection<IBudgetMonthViewModel> CreateBudgetMonths()
        {
            return DataVirtualizingCollectionBuilder<IBudgetMonthViewModel>
                .Build(pageSize: 12)
                .Hoarding()
                .Preloading()
                .TaskBasedFetchers(
                    async (offset, pageSize) =>
                    {
                        var budgetMonthViewModels = await Task.Run(async () => (await _budgetMonthRepository.FindAsync(IndexToMonth(offset).Year).ConfigureAwait(false))
                            .Select(bm => _budgetMonthViewModelFactory(bm))
                            .ToArray()).ConfigureAwait(false);

                        foreach (var bmvm in budgetMonthViewModels)
                        {
                            var categoriesToBudgetEntries = bmvm.BudgetEntries.ToDictionary(bevm => bevm.Category, bevm => bevm);
                            foreach (var bevm in bmvm.BudgetEntries)
                            {
                                await bevm.Category.CategoriesInitialized.ConfigureAwait(false);
                                bevm.Children = bevm
                                    .Category
                                    .Categories
                                    .Select(cvm => categoriesToBudgetEntries[cvm]).ToList();
                            }
                        }

                        return budgetMonthViewModels;
                    },
                    async () => await Task.FromResult(LastMonthIndex))
                .SyncIndexAccess();
        }

        private static DateTime IndexToMonth(int index)
        {
            int year = index / 12 + 1;
            int month = index % 12 + 1;

            return new DateTime(year, month, 1);
        }

        private static int MonthToIndex(DateTime month)
        {
            return (month.Year - 1) * 12 + month.Month - 1;
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}