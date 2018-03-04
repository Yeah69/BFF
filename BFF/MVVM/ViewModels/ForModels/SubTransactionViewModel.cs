using System;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface ISubTransactionViewModel : ITransLikeViewModel, IHaveCategoryViewModel
    {
        INewCategoryViewModel NewCategoryViewModel { get; }
    }
    
    public sealed class SubTransactionViewModel : TransLikeViewModel, ISubTransactionViewModel
    {
        public SubTransactionViewModel(
            ISubTransaction subTransaction,
            Func<IHaveCategoryViewModel, INewCategoryViewModel> newCategoryViewModelFactory,
            Func<IReactiveProperty<long>, ISumEditViewModel> createSumEdit,
            ICategoryBaseViewModelService categoryViewModelService)
            : base(subTransaction)
        {
            Category = subTransaction.ToReactivePropertyAsSynchronized(
                sti => sti.Category,
                categoryViewModelService.GetViewModel,
                categoryViewModelService.GetModel,
                ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);
            Sum = subTransaction.ToReactivePropertyAsSynchronized(sti => sti.Sum, ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);

            SumEdit = createSumEdit(Sum);

            NewCategoryViewModel = newCategoryViewModelFactory(this);
        }

        public INewCategoryViewModel NewCategoryViewModel { get; }

        /// <summary>
        /// Each SubTransaction can be budgeted to a category.
        /// </summary>
        public IReactiveProperty<ICategoryBaseViewModel> Category { get; }

        /// <summary>
        /// The amount of money of the exchange of the SubTransaction.
        /// </summary>
        public override IReactiveProperty<long> Sum { get; }

        public override ISumEditViewModel SumEdit { get; }

        public override bool IsInsertable() => base.IsInsertable() && Category.Value.IsNotNull();
    }
}