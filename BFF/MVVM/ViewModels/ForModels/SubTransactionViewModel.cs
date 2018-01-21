using System;
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

    /// <summary>
    /// The ViewModel of the Model SubTransaction.
    /// </summary>
    public sealed class SubTransactionViewModel : TransLikeViewModel, ISubTransactionViewModel
    {
        /// <summary>
        /// Initializes a SubTransactionViewModel.
        /// </summary>
        /// <param name="subTransaction">A SubTransaction Model.</param>
        /// <param name="newCategoryViewModelFactory">Creates a category factory.</param>
        /// <param name="createSumEdit">Creates a sum editing viewmodel.</param>
        /// <param name="categoryViewModelService">Fetches categories.</param>
        public SubTransactionViewModel(
            ISubTransaction subTransaction,
            Func<IHaveCategoryViewModel, INewCategoryViewModel> newCategoryViewModelFactory,
            Func<Func<long>, Action<long>, ISumEditViewModel> createSumEdit,
            ICategoryBaseViewModelService categoryViewModelService)
            : base(subTransaction, createSumEdit)
        {
            Category = subTransaction.ToReactivePropertyAsSynchronized(
                sti => sti.Category,
                categoryViewModelService.GetViewModel,
                categoryViewModelService.GetModel,
                ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);
            Sum = subTransaction.ToReactivePropertyAsSynchronized(sti => sti.Sum, ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);

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
    }
}