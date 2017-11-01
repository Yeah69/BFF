using System;
using BFF.DB;
using BFF.Helper;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface IBudgetEntryViewModel : IDataModelViewModel, IHaveCategoryViewModel
    {
        IReactiveProperty<DateTime> Month { get; }
        IReactiveProperty<long> Budget { get; }
        IReadOnlyReactiveProperty<long> Outflow { get; }
        IReadOnlyReactiveProperty<long> Balance { get; }
    }

    public class BudgetEntryViewModel : DataModelViewModel, IBudgetEntryViewModel
    {
        public IReactiveProperty<ICategoryViewModel> Category { get; }
        public IReactiveProperty<DateTime> Month { get; }
        public IReactiveProperty<long> Budget { get; }
        public IReadOnlyReactiveProperty<long> Outflow { get; }
        public IReadOnlyReactiveProperty<long> Balance { get; }

        public BudgetEntryViewModel(IBffOrm orm, IBudgetEntry budgetEntry, ICategoryViewModelService categoryViewModelService) : base(orm, budgetEntry)
        {
            Category = budgetEntry.ToReactivePropertyAsSynchronized(be => be.Category, categoryViewModelService.GetViewModel,
                categoryViewModelService.GetModel).AddTo(CompositeDisposable);

            Month = budgetEntry.ToReactivePropertyAsSynchronized(be => be.Month);

            Budget = budgetEntry.ToReactivePropertyAsSynchronized(be => be.Budget);

            Outflow = budgetEntry.ToReadOnlyReactivePropertyAsSynchronized(be => be.Outflow);

            Balance = budgetEntry.ToReadOnlyReactivePropertyAsSynchronized(be => be.Balance);
        }

        public override bool ValidToInsert()
        {
            return Category.Value != null && Budget.Value != 0;
        }
    }
}
