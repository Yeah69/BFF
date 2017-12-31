using System;
using BFF.DB;
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
    public class SubTransactionViewModel : TransLikeViewModel, ISubTransactionViewModel
    {
        /// <summary>
        /// Initializes a SubTransactionViewModel.
        /// </summary>
        /// <param name="subTransaction">A SubTransaction Model.</param>
        /// <param name="parent">The ViewModel of the Model's ParentTransaction.</param>
        /// <param name="orm">Used for the database accesses.</param>
        public SubTransactionViewModel(
            ISubTransaction subTransaction,
            Func<IHaveCategoryViewModel, INewCategoryViewModel> newCategoryViewModelFactory,
            IBffOrm orm,
            ICategoryBaseViewModelService categoryViewModelService) :
            base(orm, subTransaction)
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
        public override IReactiveProperty<long> Sum { get;  }

        /// <summary>
        /// Before a model object is inserted into the database, it has to be valid.
        /// This function should guarantee that the object is valid to be inserted.
        /// </summary>
        /// <returns>True if valid, else false</returns>
        public override bool ValidToInsert()
        {
            return Category.Value != null;
        }
    }
}