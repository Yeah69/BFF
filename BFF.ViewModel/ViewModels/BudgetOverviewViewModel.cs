using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.Core.IoC;
using BFF.ViewModel.Extensions;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Managers;
using BFF.ViewModel.ViewModels.ForModels;
using BFF.ViewModel.ViewModels.ForModels.Utility;
using MrMeeseeks.Extensions;
using Reactive.Bindings.Extensions;

namespace BFF.ViewModel.ViewModels
{
    public interface IBudgetOverviewViewModel
    {
        bool ShowBudgetMonths { get; }

        IBudgetMonthViewModel CurrentBudgetMonth { get; }

        bool IsOpen { get; set; }

        DateTime SelectedMonth { get; set; }

        ITransDataGridColumnManager TransDataGridColumnManager { get; }

        Task Refresh();

        IDisposable DeferRefreshUntilDisposal();

        IBudgetMonthMenuTitles BudgetMonthMenuTitles { get; }

        IBudgetOverviewTableViewModel Table { get; }
    }

    internal class BudgetOverviewViewModel : ViewModelBase, IBudgetOverviewViewModel, IOncePerBackend, IDisposable
    {

        private readonly IBffSettings _bffSettings;
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private int _selectedIndex;
        private int _currentMonthStartIndex;
        private bool _isOpen;
        private bool _canRefresh = true;
        private DateTime _selectedMonth;
        private bool _showBudgetMonths;

        public bool ShowBudgetMonths
        {
            get => _showBudgetMonths;
            private set
            {
                if (value == _showBudgetMonths) return;

                _showBudgetMonths = value;
                OnPropertyChanged();
            }
        }

        public IBudgetMonthViewModel CurrentBudgetMonth => null; // BudgetMonths[MonthToIndex(DateTime.Now)];
        public IBudgetMonthMenuTitles BudgetMonthMenuTitles { get; }
        public IBudgetOverviewTableViewModel Table { get; }

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
        
        public ITransDataGridColumnManager TransDataGridColumnManager { get; }

        public BudgetOverviewViewModel(
            Func<IBudgetOverviewTableViewModel> budgetOverviewTableViewModel,
            ICultureManager cultureManager,
            IBffSettings bffSettings,
            IBudgetMonthMenuTitles budgetMonthMenuTitles,
            ITransDataGridColumnManager transDataGridColumnManager,
            IRxSchedulerProvider rxSchedulerProvider)
        {
            BudgetMonthMenuTitles = budgetMonthMenuTitles;
            TransDataGridColumnManager = transDataGridColumnManager;
            _bffSettings = bffSettings;
            _rxSchedulerProvider = rxSchedulerProvider;

            SelectedIndex = -1;

            CurrentMonthStartIndex = MonthToIndex(DateTime.Now) - 41;

            Table = budgetOverviewTableViewModel();

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

            //this.Refresh();
        }

        public async Task Refresh()
        {
            if (_canRefresh.Not()) return;
            ShowBudgetMonths = false;
            //var temp = _budgetMonths;
            //await _budgetMonths.InitializationCompleted.ConfigureAwait(false);
            ShowBudgetMonths = true;
            _rxSchedulerProvider.UI.MinimalSchedule(() =>
            {
                OnPropertyChanged(nameof(Table));
            });
            //await Task.Run(() => temp?.Dispose()).ConfigureAwait(false);
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

        public static DateTime IndexToMonth(int index)
        {
            int year = index / 12 + 1;
            int month = index % 12 + 1;

            return new DateTime(year, month, 1);
        }

        public static int MonthToIndex(DateTime month)
        {
            return (month.Year - 1) * 12 + month.Month - 1;
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}