using System;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.Services;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface ISubTransIncViewModel : ITitLikeViewModel, IHaveCategoryViewModel
    {
        INewCategoryViewModel NewCategoryViewModel { get; }
    }

    /// <summary>
    /// Base class for ViewModels of the Models SubTransaction
    /// </summary>
    public abstract class SubTransIncViewModel : TitLikeViewModel, ISubTransIncViewModel
    {
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
        /// Initializes a SubTransIncViewModel.
        /// </summary>
        /// <param name="subTransaction">The associated Model of this ViewModel.</param>
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="categoryViewModelService">Service for categories.</param>
        protected SubTransIncViewModel(
            ISubTransInc subTransaction, 
            Func<IHaveCategoryViewModel, INewCategoryViewModel> newCategoryViewModelFactory, 
            IBffOrm orm,
            ICategoryBaseViewModelService categoryViewModelService) : base(orm, subTransaction)
        {
            Category = subTransaction.ToReactivePropertyAsSynchronized(
                sti => sti.Category,
                categoryViewModelService.GetViewModel, 
                categoryViewModelService.GetModel).AddTo(CompositeDisposable);
            Sum = subTransaction.ToReactivePropertyAsSynchronized(sti => sti.Sum).AddTo(CompositeDisposable);

            NewCategoryViewModel = newCategoryViewModelFactory(this);
        }

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
