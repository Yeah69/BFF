﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.ViewModel.Extensions;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Managers;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using BFF.ViewModel.ViewModels.ForModels.Utility;
using MrMeeseeks.Reactive.Extensions;
using MrMeeseeks.Windows;

namespace BFF.ViewModel.ViewModels.ForModels
{
    public interface IBudgetEntryViewModel : IDataModelViewModel
    {
        long Budget { get; set; }
        long Outflow { get; }
        long Balance { get; }

        long AggregatedBudget { get; }
        long AggregatedOutflow { get; }
        long AggregatedBalance { get; }

        ILazyTransLikeViewModels? AssociatedTransElementsViewModel { get; }

        ILazyTransLikeViewModels? AssociatedAggregatedTransElementsViewModel { get; }

        ICommand BudgetLastMonth { get; }

        ICommand OutflowsLastMonth { get; }

        ICommand AvgOutflowsLastThreeMonths { get; }

        ICommand AvgOutflowsLastYear { get; }

        ICommand BalanceToZero { get; }

        ICommand Zero { get; }

        Task SetBudgetToAverageBudgetOfLastMonths(int monthCount);
        
        Task SetBudgetToAverageOutflowOfLastMonths(int monthCount);
    }

    public class BudgetEntryViewModel : DataModelViewModel, IBudgetEntryViewModel
    {
        private readonly IBudgetEntry _budgetEntry;

        public long Budget
        {
            get => _budgetEntry.Budget;
            set => _budgetEntry.Budget = value;
        }
        
        public long Outflow => _budgetEntry.Outflow;
        public long Balance => _budgetEntry.Balance;
        public long AggregatedBudget => _budgetEntry.AggregatedBudget;
        public long AggregatedOutflow => _budgetEntry.AggregatedOutflow;
        public long AggregatedBalance => _budgetEntry.AggregatedBalance;

        public ILazyTransLikeViewModels AssociatedTransElementsViewModel { get; }
        public ILazyTransLikeViewModels AssociatedAggregatedTransElementsViewModel { get; }
        public ICommand BudgetLastMonth { get; }
        public ICommand OutflowsLastMonth { get; }
        public ICommand AvgOutflowsLastThreeMonths { get; }
        public ICommand AvgOutflowsLastYear { get; }
        public ICommand BalanceToZero { get; }
        public ICommand Zero { get; }
        
        public async Task SetBudgetToAverageBudgetOfLastMonths(int monthCount) => 
            await _budgetEntry.SetBudgetToAverageBudgetOfLastMonths(monthCount).ConfigureAwait(false);

        public async Task SetBudgetToAverageOutflowOfLastMonths(int monthCount) => 
            await _budgetEntry.SetBudgetToAverageOutflowOfLastMonths(monthCount).ConfigureAwait(false);

        public BudgetEntryViewModel(
            // parameters
            IBudgetEntry budgetEntry,
            
            // dependencies
            ICategoryViewModelService categoryViewModelService,
            IBudgetRefreshes budgetRefreshes,
            Func<Func<Task<IEnumerable<ITransLikeViewModel>>>, ILazyTransLikeViewModels> lazyTransLikeViewModelsFactory, 
            IConvertFromTransBaseToTransLikeViewModel convertFromTransBaseToTransLikeViewModel,
            IRxSchedulerProvider rxSchedulerProvider) : base(budgetEntry, rxSchedulerProvider)
        {
            _budgetEntry = budgetEntry;
            var category = categoryViewModelService.GetViewModel(budgetEntry.Category) ?? throw new NullReferenceException("Shouldn't be null");

            var observeBudgetChanges = budgetEntry.ObservePropertyChanged(nameof(IBudgetEntry.Budget));
            observeBudgetChanges
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(Budget)))
                .CompositeDisposalWith(CompositeDisposable);

            observeBudgetChanges
                .Subscribe(_ => budgetRefreshes.Refresh(category))
                .CompositeDisposalWith(CompositeDisposable);

            budgetEntry
                .ObservePropertyChanged(nameof(IBudgetEntry.Outflow))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(Outflow)))
                .CompositeDisposalWith(CompositeDisposable);

            budgetEntry
                .ObservePropertyChanged(nameof(IBudgetEntry.Balance))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(Balance)))
                .CompositeDisposalWith(CompositeDisposable);

            AssociatedTransElementsViewModel = lazyTransLikeViewModelsFactory(async () =>
                convertFromTransBaseToTransLikeViewModel.Convert(
                    await budgetEntry.GetAssociatedTransAsync(), null))
                .CompositeDisposalWith(CompositeDisposable);

            AssociatedAggregatedTransElementsViewModel = lazyTransLikeViewModelsFactory(async () =>
                convertFromTransBaseToTransLikeViewModel.Convert(
                    await budgetEntry.GetAssociatedTransIncludingSubCategoriesAsync(), null))
                .CompositeDisposalWith(CompositeDisposable);

            BudgetLastMonth = RxCommand
                .CanAlwaysExecute()
                .StandardCaseAsync(
                    CompositeDisposable,
                    async () => await SetBudgetToAverageBudgetOfLastMonths(1).ConfigureAwait(false));

            OutflowsLastMonth = RxCommand
                .CanAlwaysExecute()
                .StandardCaseAsync(
                    CompositeDisposable,
                    async () => await SetBudgetToAverageOutflowOfLastMonths(1).ConfigureAwait(false));

            AvgOutflowsLastThreeMonths = RxCommand
                .CanAlwaysExecute()
                .StandardCaseAsync(
                    CompositeDisposable,
                    async () => await SetBudgetToAverageOutflowOfLastMonths(3).ConfigureAwait(false));

            AvgOutflowsLastYear = RxCommand
                .CanAlwaysExecute()
                .StandardCaseAsync(
                    CompositeDisposable,
                    async () => await SetBudgetToAverageOutflowOfLastMonths(12).ConfigureAwait(false));

            BalanceToZero = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    CompositeDisposable,
                    () => Budget -= Balance);

            Zero = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    CompositeDisposable,
                    () => Budget = 0);
        }
    }
}
