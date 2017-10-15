using System;
using BFF.DB;
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
        long Outflow { get; }
        long Balance { get; }
    }

    public class BudgetEntryViewModel : DataModelViewModel, IBudgetEntryViewModel
    {
        public override bool ValidToInsert()
        {
            return Category.Value != null && Budget.Value != 0;
        }

        public IReactiveProperty<ICategoryViewModel> Category { get; }
        public IReactiveProperty<DateTime> Month { get; }
        public IReactiveProperty<long> Budget { get; }
        public long Outflow { get; }
        public long Balance { get; }

        public BudgetEntryViewModel(IBffOrm orm, IBudgetEntry budgetEntry, ICategoryViewModelService categoryViewModelService) : base(orm, budgetEntry)
        {
            Category = budgetEntry.ToReactivePropertyAsSynchronized(be => be.Category, categoryViewModelService.GetViewModel,
                categoryViewModelService.GetModel).AddTo(CompositeDisposable);

            Month = budgetEntry.ToReactivePropertyAsSynchronized(be => be.Month);

            Budget = budgetEntry.ToReactivePropertyAsSynchronized(be => be.Budget);
        }
    }
}
