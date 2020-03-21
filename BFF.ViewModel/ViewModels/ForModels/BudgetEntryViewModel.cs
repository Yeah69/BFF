using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.ViewModel.Helper;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using BFF.ViewModel.ViewModels.ForModels.Utility;
using MrMeeseeks.Extensions;

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

        public BudgetEntryViewModel(
            IBudgetEntry budgetEntry,
            Lazy<IBudgetOverviewViewModel> budgetOverviewViewModel,
            Func<Func<Task<IEnumerable<ITransLikeViewModel>>>, ILazyTransLikeViewModels> lazyTransLikeViewModelsFactory, 
            IConvertFromTransBaseToTransLikeViewModel convertFromTransBaseToTransLikeViewModel,
            IRxSchedulerProvider rxSchedulerProvider) : base(budgetEntry, rxSchedulerProvider)
        {
            _budgetEntry = budgetEntry;

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
                    // var previousBudgetMonth = budgetOverviewViewModel
                    //     .Value
                    //     .GetBudgetMonthViewModel(Month.PreviousMonth())
                    //     .BudgetEntries
                    //     .FirstOrDefault(bevm => bevm.Category == Category);
                    // if (previousBudgetMonth != null)
                    //     Budget = previousBudgetMonth.Budget;
                })
                .AddForDisposalTo(CompositeDisposable);

            OutflowsLastMonth = new RxRelayCommand(() =>
                {
                    // var previousBudgetMonth = budgetOverviewViewModel
                    //     .Value
                    //     .GetBudgetMonthViewModel(Month.PreviousMonth())
                    //     .BudgetEntries
                    //     .FirstOrDefault(bevm => bevm.Category == Category);
                    // if (previousBudgetMonth != null)
                    //     Budget = previousBudgetMonth.Outflow * -1;
                })
                .AddForDisposalTo(CompositeDisposable);

            AvgOutflowsLastThreeMonths = new RxRelayCommand(() =>
                {
                    // var budgetEntries = new List<IBudgetEntryViewModel>();
                    // var currentMonth = Month.PreviousMonth();
                    // for (int i = 0; i < 3; i++)
                    // {
                    //     var currentBudget = budgetOverviewViewModel
                    //         .Value
                    //         .GetBudgetMonthViewModel(currentMonth);
                    //     if (currentBudget is null) break;
                    //     var budgetEntryViewModel = currentBudget.BudgetEntries.SingleOrDefault(bevm => bevm.Category == Category);
                    //     if(budgetEntryViewModel != null)
                    //         budgetEntries.Add(budgetEntryViewModel);
                    //     currentMonth = currentMonth.PreviousMonth();
                    // }
                    //
                    // Budget = budgetEntries.Sum(bevm => bevm.Outflow) / -3L;
                })
                .AddForDisposalTo(CompositeDisposable);

            AvgOutflowsLastYear = new RxRelayCommand(() =>
            {
                // var budgetEntries = new List<IBudgetEntryViewModel>();
                // var currentMonth = Month.PreviousMonth();
                // for (int i = 0; i < 12; i++)
                // {
                //     var currentBudget = budgetOverviewViewModel
                //         .Value
                //         .GetBudgetMonthViewModel(currentMonth);
                //     if (currentBudget is null) break;
                //     var budgetEntryViewModel = currentBudget.BudgetEntries.SingleOrDefault(bevm => bevm.Category == Category);
                //     if (budgetEntryViewModel != null)
                //         budgetEntries.Add(budgetEntryViewModel);
                //     currentMonth = currentMonth.PreviousMonth();
                // }
                //
                // Budget = budgetEntries.Sum(bevm => bevm.Outflow) / -12L;
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
