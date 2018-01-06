using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using BFF.Helper;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;


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

    public sealed class BudgetMonthViewModel : IBudgetMonthViewModel, IDisposable, ITransientViewModel
    {
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public BudgetMonthViewModel(IBudgetMonth budgetMonth, IBudgetEntryViewModelService budgetEntryViewModelService)
        {
            BudgetEntries = budgetMonth.BudgetEntries.ToReadOnlyReactiveCollection(budgetEntryViewModelService.GetViewModel).AddTo(_compositeDisposable);
            Month = budgetMonth.ToReadOnlyReactivePropertyAsSynchronized(bm => bm.Month).AddTo(_compositeDisposable);
            NotBudgetedInPreviousMonth = budgetMonth.ToReadOnlyReactivePropertyAsSynchronized(bm => bm.NotBudgetedInPreviousMonth).AddTo(_compositeDisposable);
            OverspentInPreviousMonth = budgetMonth.ToReadOnlyReactivePropertyAsSynchronized(bm => bm.OverspentInPreviousMonth).AddTo(_compositeDisposable);
            IncomeForThisMonth = budgetMonth.ToReadOnlyReactivePropertyAsSynchronized(bm => bm.IncomeForThisMonth).AddTo(_compositeDisposable);
            BudgetedThisMonth = budgetMonth.ToReadOnlyReactivePropertyAsSynchronized(bm => bm.BudgetedThisMonth).AddTo(_compositeDisposable);
            BudgetedThisMonthPositive = BudgetedThisMonth.Select(l => -1 * l).ToReadOnlyReactiveProperty().AddTo(_compositeDisposable);
            Outflows = budgetMonth.ToReadOnlyReactivePropertyAsSynchronized(bm => bm.Outflows).AddTo(_compositeDisposable);
            Balance = budgetMonth.ToReadOnlyReactivePropertyAsSynchronized(bm => bm.Balance).AddTo(_compositeDisposable);
            AvailableToBudget = budgetMonth.ToReadOnlyReactivePropertyAsSynchronized(bm => bm.AvailableToBudget).AddTo(_compositeDisposable);
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

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}
