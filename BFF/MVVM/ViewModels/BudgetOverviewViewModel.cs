using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.DataVirtualizingCollections;
using BFF.DB;
using BFF.DB.Dapper;
using BFF.DB.Dapper.ModelRepositories;
using BFF.Helper;
using BFF.Helper.Extensions;
using BFF.MVVM.Managers;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels
{
    public interface IBudgetOverviewViewModel
    {
        IList<IBudgetMonthViewModel> BudgetMonths { get; }

        int CurrentMonthStartIndex { get; }

        bool IsOpen { get; set; }
        int SelectedIndex { get; set; }

        IRxRelayCommand IncreaseMonthStartIndex { get; }

        IRxRelayCommand DecreaseMonthStartIndex { get; }

        ITransDataGridColumnManager TransDataGridColumnManager { get; }

        Task Refresh();
    }

    public class BudgetOverviewViewModel : ViewModelBase, IBudgetOverviewViewModel, IOncePerBackend, IDisposable
    {
        private static readonly int LastMonthIndex = MonthToIndex(DateTime.MaxValue);

        private readonly IBudgetMonthRepository _budgetMonthRepository;
        private readonly Func<IBudgetMonth, IBudgetMonthViewModel> _budgetMonthViewModelFactory;
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private int _selectedIndex;
        private int _currentMonthStartIndex;
        private bool _isOpen;
        public IList<IBudgetMonthViewModel> BudgetMonths { get; private set; }

        public ReadOnlyReactiveCollection<ICategoryViewModel> Categories { get; }

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

        public int CurrentMonthStartIndex
        {
            get => _currentMonthStartIndex;
            set
            {
                if (_currentMonthStartIndex == value) return;
                _currentMonthStartIndex = value;
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
            }
        }

        public IRxRelayCommand IncreaseMonthStartIndex { get; }

        public IRxRelayCommand DecreaseMonthStartIndex { get; }
        public ITransDataGridColumnManager TransDataGridColumnManager { get; }

        public BudgetOverviewViewModel(
            IBudgetMonthRepository budgetMonthRepository,
            ICultureManager cultureManager,
            Func<IBudgetMonth, IBudgetMonthViewModel> budgetMonthViewModelFactory,
            ITransDataGridColumnManager transDataGridColumnManager,
            IRxSchedulerProvider rxSchedulerProvider,
            ICategoryViewModelService categoryViewModelService,
            ICategoryRepository categoryRepository)
        {
            TransDataGridColumnManager = transDataGridColumnManager;
            _budgetMonthRepository = budgetMonthRepository;
            _budgetMonthViewModelFactory = budgetMonthViewModelFactory;
            _rxSchedulerProvider = rxSchedulerProvider;

            SelectedIndex = -1;

            Categories =
                categoryRepository
                    .All
                    .ToReadOnlyReactiveCollection(categoryViewModelService.GetViewModel);

            BudgetMonths = CreateBudgetMonths();
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

            IsOpen = false;

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
        }

        public async Task Refresh()
        {
            BudgetMonths = await Task.Run(() => CreateBudgetMonths());
            _rxSchedulerProvider.UI.MinimalSchedule(() => OnPropertyChanged(nameof(BudgetMonths)));
        }

        private IDataVirtualizingCollection<IBudgetMonthViewModel> CreateBudgetMonths()
        {
            return CollectionBuilder<IBudgetMonthViewModel>
                .CreateBuilder()
                .BuildAHoardingPreloadingTaskBasedSyncCollection(
                    new RelayBasicTaskBasedSyncDataAccess<IBudgetMonthViewModel>(
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
                                    bevm.Children = bevm
                                        .Category
                                        .Categories
                                        .Select(cvm => categoriesToBudgetEntries[cvm]).ToList();
                                }
                            }

                            return budgetMonthViewModels;
                        },
                        () =>  Task.FromResult(LastMonthIndex)),
                    pageSize: 12);
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