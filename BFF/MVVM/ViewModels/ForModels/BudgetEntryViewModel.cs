using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BFF.Core;
using BFF.Core.Extensions;
using BFF.DB.Dapper.ModelRepositories;
using BFF.Helper;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;
using BFF.MVVM.ViewModels.ForModels.Utility;

namespace BFF.MVVM.ViewModels.ForModels
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

    public class BudgetEntryViewModel : DataModelViewModel, IBudgetEntryViewModel
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
            ITransRepository transRepository,
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
                .AddTo(CompositeDisposable);

            budgetEntry
                .ObservePropertyChanges(nameof(IBudgetEntry.Month))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(Month)))
                .AddTo(CompositeDisposable);

            var observeBudgetChanges = budgetEntry.ObservePropertyChanges(nameof(IBudgetEntry.Budget));
            observeBudgetChanges
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(Budget)))
                .AddTo(CompositeDisposable);

            observeBudgetChanges
                .Subscribe(async _ => await Task.Factory.StartNew(budgetOverviewViewModel.Value.Refresh))
                .AddTo(CompositeDisposable);

            budgetEntry
                .ObservePropertyChanges(nameof(IBudgetEntry.Outflow))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(Outflow)))
                .AddTo(CompositeDisposable);

            budgetEntry
                .ObservePropertyChanges(nameof(IBudgetEntry.Balance))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(Balance)))
                .AddTo(CompositeDisposable);

            Children = new List<IBudgetEntryViewModel>();

            AggregatedBudget = Budget + Children.Sum(bevm => bevm.AggregatedBudget);
            AggregatedBalance = Balance + Children.Sum(bevm => bevm.AggregatedBalance);
            AggregatedOutflow = Outflow + Children.Sum(bevm => bevm.AggregatedOutflow);

            var updatesFromChildren = new SerialDisposable().AddHere(CompositeDisposable);

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
                    await transRepository.GetFromMonthAndCategoryAsync(Month,
                        categoryViewModelService.GetModel(Category)), null))
                .AddHere(CompositeDisposable);

            AssociatedAggregatedTransElementsViewModel = lazyTransLikeViewModelsFactory(async () =>
                convertFromTransBaseToTransLikeViewModel.Convert(
                    await transRepository.GetFromMonthAndCategoriesAsync(Month,
                        categoryViewModelService.GetModel(Category).IterateRootBreadthFirst(c => c.Categories)), null))
                .AddHere(CompositeDisposable);

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
                .AddHere(CompositeDisposable);

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
                .AddHere(CompositeDisposable);

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
                .AddHere(CompositeDisposable);

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
                .AddHere(CompositeDisposable);

            BalanceToZero = new RxRelayCommand(() =>
            {
                Budget -= Balance;
            })
                .AddHere(CompositeDisposable);

            Zero = new RxRelayCommand(() =>
            {
                Budget = 0;
            })
            .AddHere(CompositeDisposable);
        }
    }
}
