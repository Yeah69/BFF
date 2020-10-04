using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using BFF.Core.Helper;
using BFF.Core.IoC;
using BFF.ViewModel.Managers;
using BFF.ViewModel.ViewModels.ForModels.Utility;
using MrMeeseeks.Reactive.Extensions;
using Reactive.Bindings.Extensions;

namespace BFF.ViewModel.ViewModels
{
    public interface IBudgetOverviewViewModel
    {
        bool IsOpen { get; set; }

        ITransDataGridColumnManager TransDataGridColumnManager { get; }

        IBudgetMonthMenuTitles BudgetMonthMenuTitles { get; }

        IBudgetOverviewTableViewModel Table { get; }
    }

    internal class BudgetOverviewViewModel : ViewModelBase, IBudgetOverviewViewModel, IOncePerBackend, IDisposable
    {
        
        private readonly IBffSettings _bffSettings;
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private bool _isOpen;
        
        public IBudgetMonthMenuTitles BudgetMonthMenuTitles { get; }
        public IBudgetOverviewTableViewModel Table { get; }

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
            IBudgetOverviewTableViewModel budgetOverviewTableViewModel,
            IBffSettings bffSettings,
            IBudgetMonthMenuTitles budgetMonthMenuTitles,
            ITransDataGridColumnManager transDataGridColumnManager,
            IRxSchedulerProvider rxSchedulerProvider)
        {
            BudgetMonthMenuTitles = budgetMonthMenuTitles;
            TransDataGridColumnManager = transDataGridColumnManager;
            _bffSettings = bffSettings;

            Table = budgetOverviewTableViewModel;

            IsOpen = _bffSettings.OpenMainTab == "BudgetOverview";

            this
                .ObservePropertyChanged(nameof(IsOpen))
                .Where(_ => IsOpen)
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(b => Table.Reset())
                .AddTo(_compositeDisposable);
        }

        public void Dispose() => 
            _compositeDisposable.Dispose();
    }
}