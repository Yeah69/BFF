using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Core.IoC;
using BFF.Model.Models;
using BFF.Model.Models.Structure;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using BFF.ViewModel.ViewModels.ForModels.Utility;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.ViewModel.ViewModels
{
    public interface IBudgetMonthViewModel
    {
        ReadOnlyReactiveCollection<IBudgetEntryViewModel> BudgetEntries { get; }
        DateTime Month { get; }
        long NotBudgetedInPreviousMonth { get; }
        long OverspentInPreviousMonth { get; }
        long IncomeForThisMonth { get; }
        long DanglingTransferForThisMonth { get; }
        long UnassignedTransactionSumForThisMonth { get; }
        long BudgetedThisMonth { get; }
        long BudgetedThisMonthPositive { get; }
        long AvailableToBudget { get; }
        long Outflows { get; }
        long Balance { get; }

        ILazyTransLikeViewModels AssociatedTransElementsViewModel { get; }
        ILazyTransLikeViewModels AssociatedIncomeTransElementsViewModel { get; }

        IRxRelayCommand EmptyCellsBudgetLastMonth { get; }

        IRxRelayCommand EmptyCellsOutflowsLastMonth { get; }

        IRxRelayCommand EmptyCellsAvgOutflowsLastThreeMonths { get; }

        IRxRelayCommand EmptyCellsAvgOutflowsLastYear { get; }

        IRxRelayCommand EmptyCellsBalanceToZero { get; }

        IRxRelayCommand AllCellsZero { get; }
    }

    public sealed class BudgetMonthViewModel : ViewModelBase, IBudgetMonthViewModel, IDisposable, ITransient
    {
        private readonly IBudgetMonth _budgetMonth;
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public BudgetMonthViewModel(
            IBudgetMonth budgetMonth,
            IBudgetOverviewViewModel budgetOverviewViewModel,
            Func<Func<Task<IEnumerable<ITransLikeViewModel>>>, ILazyTransLikeViewModels> lazyTransLikeViewModelsFactory,
            IConvertFromTransBaseToTransLikeViewModel convertFromTransBaseToTransLikeViewModel,
            IBudgetEntryViewModelService budgetEntryViewModelService)
        {
            _budgetMonth = budgetMonth;
            BudgetEntries = budgetMonth.BudgetEntries.ToReadOnlyReactiveCollection(budgetEntryViewModelService.GetViewModel).AddTo(_compositeDisposable);

            budgetMonth
                .ObservePropertyChanges(nameof(budgetMonth.Month))
                .Subscribe(fa => OnPropertyChanged(nameof(Month)))
                .AddTo(_compositeDisposable);
            budgetMonth
                .ObservePropertyChanges(nameof(budgetMonth.NotBudgetedInPreviousMonth))
                .Subscribe(fa => OnPropertyChanged(nameof(NotBudgetedInPreviousMonth)))
                .AddTo(_compositeDisposable);
            budgetMonth
                .ObservePropertyChanges(nameof(budgetMonth.OverspentInPreviousMonth))
                .Subscribe(fa => OnPropertyChanged(nameof(OverspentInPreviousMonth)))
                .AddTo(_compositeDisposable);
            budgetMonth
                .ObservePropertyChanges(nameof(budgetMonth.IncomeForThisMonth))
                .Subscribe(fa => OnPropertyChanged(nameof(IncomeForThisMonth)))
                .AddTo(_compositeDisposable);
            budgetMonth
                .ObservePropertyChanges(nameof(budgetMonth.DanglingTransferForThisMonth))
                .Subscribe(fa => OnPropertyChanged(nameof(DanglingTransferForThisMonth)))
                .AddTo(_compositeDisposable);
            budgetMonth
                .ObservePropertyChanges(nameof(budgetMonth.UnassignedTransactionSumForThisMonth))
                .Subscribe(fa => OnPropertyChanged(nameof(UnassignedTransactionSumForThisMonth)))
                .AddTo(_compositeDisposable);
            budgetMonth
                .ObservePropertyChanges(nameof(budgetMonth.BudgetedThisMonth))
                .Subscribe(fa =>
                {
                    OnPropertyChanged(nameof(BudgetedThisMonth));
                    OnPropertyChanged(nameof(BudgetedThisMonthPositive));
                })
                .AddTo(_compositeDisposable);
            budgetMonth
                .ObservePropertyChanges(nameof(budgetMonth.Outflows))
                .Subscribe(fa => OnPropertyChanged(nameof(Outflows)))
                .AddTo(_compositeDisposable);
            budgetMonth
                .ObservePropertyChanges(nameof(budgetMonth.Balance))
                .Subscribe(fa => OnPropertyChanged(nameof(Balance)))
                .AddTo(_compositeDisposable);
            budgetMonth
                .ObservePropertyChanges(nameof(budgetMonth.AvailableToBudget))
                .Subscribe(fa => OnPropertyChanged(nameof(AvailableToBudget)))
                .AddTo(_compositeDisposable);

            AssociatedTransElementsViewModel = lazyTransLikeViewModelsFactory(async () =>
                    convertFromTransBaseToTransLikeViewModel.Convert(
                        (await budgetMonth.GetAssociatedTransAsync().ConfigureAwait(false))
                        .Where(tb => tb as IHaveCategory is null || tb is IHaveCategory hc && hc.Category as IIncomeCategory is null), 
                        null))
                .AddHere(_compositeDisposable);

            AssociatedIncomeTransElementsViewModel = lazyTransLikeViewModelsFactory(async () => 
                    convertFromTransBaseToTransLikeViewModel.Convert(
                        await budgetMonth.GetAssociatedTransForIncomeCategoriesAsync(),
                        null))
                .AddHere(_compositeDisposable);

            EmptyCellsBudgetLastMonth = new RxRelayCommand(() =>
            {
                using (budgetOverviewViewModel.DeferRefreshUntilDisposal())
                {
                    var lastMonthCategoriesToBudget = budgetOverviewViewModel
                        .GetBudgetMonthViewModel(Month.PreviousMonth())
                        .BudgetEntries
                        .ToDictionary(bevm => bevm.Category, bevm => bevm);
                    foreach (var budgetEntryViewModel in BudgetEntries)
                    {
                        if (budgetEntryViewModel.Budget == 0
                            && lastMonthCategoriesToBudget
                                .ContainsKey(budgetEntryViewModel.Category))
                            budgetEntryViewModel.Budget =
                                lastMonthCategoriesToBudget[budgetEntryViewModel.Category].Budget;
                    }
                }
            })
                .AddHere(_compositeDisposable);

            EmptyCellsOutflowsLastMonth = new RxRelayCommand(() =>
            {
                using (budgetOverviewViewModel.DeferRefreshUntilDisposal())
                {
                    var lastMonthCategoriesToBudget = budgetOverviewViewModel
                        .GetBudgetMonthViewModel(Month.PreviousMonth())
                        ?.BudgetEntries
                        .ToDictionary(bevm => bevm.Category, bevm => bevm) 
                            ?? new Dictionary<ICategoryViewModel, IBudgetEntryViewModel>();
                    foreach (var budgetEntryViewModel in BudgetEntries)
                    {
                        if (budgetEntryViewModel.Budget == 0
                            && lastMonthCategoriesToBudget
                                .ContainsKey(budgetEntryViewModel.Category))
                            budgetEntryViewModel.Budget =
                                lastMonthCategoriesToBudget[budgetEntryViewModel.Category].Outflow * -1;
                    }
                }
            })
                .AddHere(_compositeDisposable);

            EmptyCellsAvgOutflowsLastThreeMonths = new RxRelayCommand(() =>
            {
                using (budgetOverviewViewModel.DeferRefreshUntilDisposal())
                {
                    var budgetMonths = new List<IBudgetMonthViewModel>();
                    var currentMonth = Month.PreviousMonth();
                    for (int i = 0; i < 3; i++)
                    {
                        var currentBudget = budgetOverviewViewModel.GetBudgetMonthViewModel(currentMonth);
                        if (currentBudget is null) break;
                        budgetMonths.Add(currentBudget);
                        currentMonth = currentMonth.PreviousMonth();
                    }

                    var categoryToAverage = budgetMonths
                        .SelectMany(bmvm => bmvm.BudgetEntries)
                        .GroupBy(bevm => bevm.Category)
                        .ToDictionary(g => g.Key, g => (long)g.Average(bevm => bevm.Outflow));
                    foreach (var budgetEntryViewModel in BudgetEntries)
                    {
                        if (budgetEntryViewModel.Budget == 0
                            && categoryToAverage
                                .ContainsKey(budgetEntryViewModel.Category))
                            budgetEntryViewModel.Budget =
                                categoryToAverage[budgetEntryViewModel.Category] * -1;
                    }
                }
            })
                .AddHere(_compositeDisposable);

            EmptyCellsAvgOutflowsLastYear = new RxRelayCommand(() =>
            {
                using (budgetOverviewViewModel.DeferRefreshUntilDisposal())
                {
                    var budgetMonths = new List<IBudgetMonthViewModel>();
                    var currentMonth = Month.PreviousMonth();
                    for (int i = 0; i < 12; i++)
                    {
                        var currentBudget = budgetOverviewViewModel.GetBudgetMonthViewModel(currentMonth);
                        if (currentBudget is null) break;
                        budgetMonths.Add(currentBudget);
                        currentMonth = currentMonth.PreviousMonth();
                    }

                    var categoryToAverage = budgetMonths
                        .SelectMany(bmvm => bmvm.BudgetEntries)
                        .GroupBy(bevm => bevm.Category)
                        .ToDictionary(g => g.Key, g => (long)g.Average(bevm => bevm.Outflow));
                    foreach (var budgetEntryViewModel in BudgetEntries)
                    {
                        if (budgetEntryViewModel.Budget == 0
                            && categoryToAverage
                                .ContainsKey(budgetEntryViewModel.Category))
                            budgetEntryViewModel.Budget =
                                categoryToAverage[budgetEntryViewModel.Category] * -1;
                    }
                }
            })
                .AddHere(_compositeDisposable);

            EmptyCellsBalanceToZero = new RxRelayCommand(() =>
            {
                using (budgetOverviewViewModel.DeferRefreshUntilDisposal())
                {
                    foreach (var budgetEntryViewModel in BudgetEntries)
                    {
                        if (budgetEntryViewModel.Budget == 0)
                            budgetEntryViewModel.Budget -= budgetEntryViewModel.Balance;
                    }
                }
            })
                .AddHere(_compositeDisposable);

            AllCellsZero = new RxRelayCommand(() =>
            {
                using (budgetOverviewViewModel.DeferRefreshUntilDisposal())
                {
                    foreach (var budgetEntryViewModel in BudgetEntries)
                    {
                        budgetEntryViewModel.Budget = 0;
                    }
                }
            })
                .AddHere(_compositeDisposable);
        }

        public ReadOnlyReactiveCollection<IBudgetEntryViewModel> BudgetEntries { get; }
        public DateTime Month => _budgetMonth.Month;
        public long NotBudgetedInPreviousMonth => _budgetMonth.NotBudgetedInPreviousMonth;
        public long OverspentInPreviousMonth => _budgetMonth.OverspentInPreviousMonth;
        public long IncomeForThisMonth => _budgetMonth.IncomeForThisMonth;
        public long DanglingTransferForThisMonth => _budgetMonth.DanglingTransferForThisMonth;
        public long UnassignedTransactionSumForThisMonth => _budgetMonth.UnassignedTransactionSumForThisMonth;
        public long BudgetedThisMonth => _budgetMonth.BudgetedThisMonth;
        public long BudgetedThisMonthPositive => BudgetedThisMonth * -1;

        public long AvailableToBudget => _budgetMonth.AvailableToBudget;

        public long Outflows => _budgetMonth.Outflows;
        public long Balance => _budgetMonth.Balance;
        public ILazyTransLikeViewModels AssociatedTransElementsViewModel { get; }
        public ILazyTransLikeViewModels AssociatedIncomeTransElementsViewModel { get; }
        public IRxRelayCommand EmptyCellsBudgetLastMonth { get; }
        public IRxRelayCommand EmptyCellsOutflowsLastMonth { get; }
        public IRxRelayCommand EmptyCellsAvgOutflowsLastThreeMonths { get; }
        public IRxRelayCommand EmptyCellsAvgOutflowsLastYear { get; }
        public IRxRelayCommand EmptyCellsBalanceToZero { get; }
        public IRxRelayCommand AllCellsZero { get; }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}
