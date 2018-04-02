using System;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface IBudgetEntryViewModel : IDataModelViewModel
    {
        /// <summary>
        /// Each SubTransaction can be budgeted to a category.
        /// </summary>
        IReactiveProperty<ICategoryViewModel> Category { get; }

        IReadOnlyReactiveProperty<DateTime> Month { get; }
        IReactiveProperty<long> Budget { get; }
        IReadOnlyReactiveProperty<long> Outflow { get; }
        IReadOnlyReactiveProperty<long> Balance { get; }
    }

    public class BudgetEntryViewModel : DataModelViewModel, IBudgetEntryViewModel
    {
        public IReactiveProperty<ICategoryViewModel> Category { get; }
        public IReadOnlyReactiveProperty<DateTime> Month { get; }
        public IReactiveProperty<long> Budget { get; }
        public IReadOnlyReactiveProperty<long> Outflow { get; }
        public IReadOnlyReactiveProperty<long> Balance { get; }

        public BudgetEntryViewModel(IBudgetEntry budgetEntry, ICategoryViewModelService categoryViewModelService) : base(budgetEntry)
        {
            Category = budgetEntry
                .ToReactivePropertyAsSynchronized(
                    be => be.Category, categoryViewModelService.GetViewModel,
                    categoryViewModelService.GetModel, 
                    ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            Month = budgetEntry
                .ToReadOnlyReactivePropertyAsSynchronized(be => be.Month, ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            Budget = budgetEntry
                .ToReactivePropertyAsSynchronized(be => be.Budget, ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            Budget
                .Subscribe(l => Messenger.Default.Send(BudgetOverviewMessage.Refresh))
                .AddTo(CompositeDisposable);

            Outflow = budgetEntry
                .ToReadOnlyReactivePropertyAsSynchronized(be => be.Outflow, ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            Balance = budgetEntry
                .ToReadOnlyReactivePropertyAsSynchronized(be => be.Balance, ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);
        }
    }
}
