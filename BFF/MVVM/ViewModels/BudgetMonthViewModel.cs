using System;
using System.Reactive.Linq;
using BFF.Helper;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels;
using Reactive.Bindings;


namespace BFF.MVVM.ViewModels
{
    public interface IBudgetMonthViewModel
    {
        ReadOnlyReactiveCollection<IBudgetEntryViewModel> BudgetEntries { get; }
        IReadOnlyReactiveProperty<DateTime> Month { get; }
        IReadOnlyReactiveProperty<long> NotBudgetedInPreviousMonth { get; }
        IReadOnlyReactiveProperty<long> OverspentInPreviousMonth { get; }
        IReadOnlyReactiveProperty<long> IncomeForThisMonth { get; }
        IReadOnlyReactiveProperty<long> BudgetedThisMonth { get; }
        IReadOnlyReactiveProperty<long> BudgetedThisMonthPositive { get; }
        IReadOnlyReactiveProperty<long> AvailableToBudget { get; }
        IReadOnlyReactiveProperty<long> Outflows { get; }
        IReadOnlyReactiveProperty<long> Balance { get; }
    }

    public class BudgetMonthViewModel : IBudgetMonthViewModel
    {
        public BudgetMonthViewModel(IBudgetMonth budgetMonth, IBudgetEntryViewModelService budgetEntryViewModelService)
        {
            BudgetEntries = budgetMonth.BudgetEntries.ToReadOnlyReactiveCollection(budgetEntryViewModelService.GetViewModel);
            Month = budgetMonth.ToReadOnlyReactivePropertyAsSynchronized(bm => bm.Month);
            NotBudgetedInPreviousMonth = budgetMonth.ToReadOnlyReactivePropertyAsSynchronized(bm => bm.NotBudgetedInPreviousMonth);
            OverspentInPreviousMonth = budgetMonth.ToReadOnlyReactivePropertyAsSynchronized(bm => bm.OverspentInPreviousMonth);
            IncomeForThisMonth = budgetMonth.ToReadOnlyReactivePropertyAsSynchronized(bm => bm.IncomeForThisMonth);
            BudgetedThisMonth = budgetMonth.ToReadOnlyReactivePropertyAsSynchronized(bm => bm.BudgetedThisMonth);
            BudgetedThisMonthPositive = BudgetedThisMonth.Select(l => -1 * l).ToReadOnlyReactiveProperty();
            Outflows = budgetMonth.ToReadOnlyReactivePropertyAsSynchronized(bm => bm.Outflows);
            Balance = budgetMonth.ToReadOnlyReactivePropertyAsSynchronized(bm => bm.Balance);
            AvailableToBudget = budgetMonth.ToReadOnlyReactivePropertyAsSynchronized(bm => bm.AvailableToBudget);
        }

        public ReadOnlyReactiveCollection<IBudgetEntryViewModel> BudgetEntries { get; }
        public IReadOnlyReactiveProperty<DateTime> Month { get; }
        public IReadOnlyReactiveProperty<long> NotBudgetedInPreviousMonth { get; }
        public IReadOnlyReactiveProperty<long> OverspentInPreviousMonth { get; }
        public IReadOnlyReactiveProperty<long> IncomeForThisMonth { get; }
        public IReadOnlyReactiveProperty<long> BudgetedThisMonth { get; }
        public IReadOnlyReactiveProperty<long> BudgetedThisMonthPositive { get; }

        public IReadOnlyReactiveProperty<long> AvailableToBudget { get; }

        public IReadOnlyReactiveProperty<long> Outflows { get; }
        public IReadOnlyReactiveProperty<long> Balance { get; }
    }
}
