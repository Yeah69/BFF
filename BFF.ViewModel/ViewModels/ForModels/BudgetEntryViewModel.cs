﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using BFF.ViewModel.ViewModels.ForModels.Utility;
using MrMeeseeks.Extensions;

namespace BFF.ViewModel.ViewModels.ForModels
{
    public interface IBudgetEntryViewModel : IDataModelViewModel
    {
        /// <summary>
        /// Each SubTransaction can be budgeted to a category.
        /// </summary>
        ICategoryViewModel Category { get; }

        DateTime Month { get; }
        long Budget { get; set; }
        long Outflow { get; }
        long Balance { get; }

        long AggregatedBudget { get; }
        long AggregatedOutflow { get; }
        long AggregatedBalance { get; }

        IReadOnlyList<IBudgetEntryViewModel> Children { get; set; }

        ILazyTransLikeViewModels AssociatedTransElementsViewModel { get; }

        ILazyTransLikeViewModels AssociatedAggregatedTransElementsViewModel { get; }

        ICommand BudgetLastMonth { get; }

        ICommand OutflowsLastMonth { get; }

        ICommand AvgOutflowsLastThreeMonths { get; }

        ICommand AvgOutflowsLastYear { get; }

        ICommand BalanceToZero { get; }

        ICommand Zero { get; }
    }

    internal class BudgetEntryViewModel : DataModelViewModel, IBudgetEntryViewModel
    {
        private readonly IBudgetEntry _budgetEntry;
        private IReadOnlyList<IBudgetEntryViewModel> _children;
        
        public ICategoryViewModel Category { get; private set; }
        public DateTime Month => _budgetEntry.Month;
        public long Budget
        {
            get => _budgetEntry.Budget;
            set => _budgetEntry.Budget = value;
        }
        public long Outflow => _budgetEntry.Outflow;
        public long Balance => _budgetEntry.Balance;
        public long AggregatedBudget { get; private set; }
        public long AggregatedOutflow { get; private set; }
        public long AggregatedBalance { get; private set; }

        public IReadOnlyList<IBudgetEntryViewModel> Children
        {
            get => _children;
            set
            {
                if (Equals(_children, value)) return; 
                _children = value;
                OnPropertyChanged();
            }
        }

        public ILazyTransLikeViewModels AssociatedTransElementsViewModel { get; }
        public ILazyTransLikeViewModels AssociatedAggregatedTransElementsViewModel { get; }
        public ICommand BudgetLastMonth { get; }
        public ICommand OutflowsLastMonth { get; }
        public ICommand AvgOutflowsLastThreeMonths { get; }
        public ICommand AvgOutflowsLastYear { get; }
        public ICommand BalanceToZero { get; }
        public ICommand Zero { get; }

        public BudgetEntryViewModel(
            IBudgetEntry budgetEntry,
            Lazy<IBudgetOverviewViewModel> budgetOverviewViewModel,
            Func<Func<Task<IEnumerable<ITransLikeViewModel>>>, ILazyTransLikeViewModels> lazyTransLikeViewModelsFactory, 
            IConvertFromTransBaseToTransLikeViewModel convertFromTransBaseToTransLikeViewModel,
            ICategoryViewModelService categoryViewModelService,
            IRxSchedulerProvider rxSchedulerProvider) : base(budgetEntry, rxSchedulerProvider)
        {
            _budgetEntry = budgetEntry;
            Category = categoryViewModelService.GetViewModel(budgetEntry.Category);
            budgetEntry
                .ObservePropertyChanges(nameof(IBudgetEntry.Category))
                .Do(_ => Category = categoryViewModelService.GetViewModel(budgetEntry.Category))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(Category)))
                .AddForDisposalTo(CompositeDisposable);

            budgetEntry
                .ObservePropertyChanges(nameof(IBudgetEntry.Month))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(Month)))
                .AddForDisposalTo(CompositeDisposable);

            var observeBudgetChanges = budgetEntry.ObservePropertyChanges(nameof(IBudgetEntry.Budget));
            observeBudgetChanges
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(Budget)))
                .AddForDisposalTo(CompositeDisposable);

            observeBudgetChanges
                .Subscribe(async _ => await Task.Factory.StartNew(budgetOverviewViewModel.Value.Refresh))
                .AddForDisposalTo(CompositeDisposable);

            budgetEntry
                .ObservePropertyChanges(nameof(IBudgetEntry.Outflow))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(Outflow)))
                .AddForDisposalTo(CompositeDisposable);

            budgetEntry
                .ObservePropertyChanges(nameof(IBudgetEntry.Balance))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(Balance)))
                .AddForDisposalTo(CompositeDisposable);

            Children = new List<IBudgetEntryViewModel>();

            AggregatedBudget = Budget + Children.Sum(bevm => bevm.AggregatedBudget);
            AggregatedBalance = Balance + Children.Sum(bevm => bevm.AggregatedBalance);
            AggregatedOutflow = Outflow + Children.Sum(bevm => bevm.AggregatedOutflow);

            var updatesFromChildren = new SerialDisposable().AddForDisposalTo(CompositeDisposable);

            var updateAggregates = Observer.Create<Unit>(_ =>
            {
                AggregatedBudget = Budget + Children.Sum(bevm => bevm.AggregatedBudget);
                AggregatedBalance = Balance + Children.Sum(bevm => bevm.AggregatedBalance);
                AggregatedOutflow = Outflow + Children.Sum(bevm => bevm.AggregatedOutflow);
                OnPropertyChanged(nameof(AggregatedBudget));
                OnPropertyChanged(nameof(AggregatedBalance));
                OnPropertyChanged(nameof(AggregatedOutflow));
            });

            this.ObservePropertyChanges(nameof(Children))
                .Do(_ =>
                    {
                        updatesFromChildren.Disposable = Children
                            .Select(bevm =>
                                bevm.ObservePropertyChanges(nameof(AggregatedBudget)))
                            .Concat(Children
                                .Select(bevm =>
                                    bevm.ObservePropertyChanges(nameof(AggregatedOutflow))))
                            .Concat(Children
                                .Select(bevm =>
                                    bevm.ObservePropertyChanges(nameof(AggregatedBalance)))).Merge().Subscribe(updateAggregates);
                    })
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(updateAggregates)
                .AddTo(CompositeDisposable);

            AssociatedTransElementsViewModel = lazyTransLikeViewModelsFactory(async () =>
                convertFromTransBaseToTransLikeViewModel.Convert(
                    await budgetEntry.GetAssociatedTransAsync(), null))
                .AddForDisposalTo(CompositeDisposable);

            AssociatedAggregatedTransElementsViewModel = lazyTransLikeViewModelsFactory(async () =>
                convertFromTransBaseToTransLikeViewModel.Convert(
                    await budgetEntry.GetAssociatedTransIncludingSubCategoriesAsync(), null))
                .AddForDisposalTo(CompositeDisposable);

            BudgetLastMonth = new RxRelayCommand(() =>
                {
                    var previousBudgetMonth = budgetOverviewViewModel
                        .Value
                        .GetBudgetMonthViewModel(Month.PreviousMonth())
                        .BudgetEntries
                        .FirstOrDefault(bevm => bevm.Category == Category);
                    if (previousBudgetMonth != null)
                        Budget = previousBudgetMonth.Budget;
                })
                .AddForDisposalTo(CompositeDisposable);

            OutflowsLastMonth = new RxRelayCommand(() =>
                {
                    var previousBudgetMonth = budgetOverviewViewModel
                        .Value
                        .GetBudgetMonthViewModel(Month.PreviousMonth())
                        .BudgetEntries
                        .FirstOrDefault(bevm => bevm.Category == Category);
                    if (previousBudgetMonth != null)
                        Budget = previousBudgetMonth.Outflow * -1;
                })
                .AddForDisposalTo(CompositeDisposable);

            AvgOutflowsLastThreeMonths = new RxRelayCommand(() =>
                {
                    var budgetEntries = new List<IBudgetEntryViewModel>();
                    var currentMonth = Month.PreviousMonth();
                    for (int i = 0; i < 3; i++)
                    {
                        var currentBudget = budgetOverviewViewModel
                            .Value
                            .GetBudgetMonthViewModel(currentMonth);
                        if (currentBudget is null) break;
                        var budgetEntryViewModel = currentBudget.BudgetEntries.SingleOrDefault(bevm => bevm.Category == Category);
                        if(budgetEntryViewModel != null)
                            budgetEntries.Add(budgetEntryViewModel);
                        currentMonth = currentMonth.PreviousMonth();
                    }

                    Budget = budgetEntries.Sum(bevm => bevm.Outflow) / -3L;
                })
                .AddForDisposalTo(CompositeDisposable);

            AvgOutflowsLastYear = new RxRelayCommand(() =>
            {
                var budgetEntries = new List<IBudgetEntryViewModel>();
                var currentMonth = Month.PreviousMonth();
                for (int i = 0; i < 12; i++)
                {
                    var currentBudget = budgetOverviewViewModel
                        .Value
                        .GetBudgetMonthViewModel(currentMonth);
                    if (currentBudget is null) break;
                    var budgetEntryViewModel = currentBudget.BudgetEntries.SingleOrDefault(bevm => bevm.Category == Category);
                    if (budgetEntryViewModel != null)
                        budgetEntries.Add(budgetEntryViewModel);
                    currentMonth = currentMonth.PreviousMonth();
                }

                Budget = budgetEntries.Sum(bevm => bevm.Outflow) / -12L;
            })
                .AddForDisposalTo(CompositeDisposable);

            BalanceToZero = new RxRelayCommand(() =>
            {
                Budget -= Balance;
            })
                .AddForDisposalTo(CompositeDisposable);

            Zero = new RxRelayCommand(() =>
            {
                Budget = 0;
            })
            .AddForDisposalTo(CompositeDisposable);
        }
    }
}