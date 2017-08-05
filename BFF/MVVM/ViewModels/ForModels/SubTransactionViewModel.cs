using BFF.DB;
using BFF.Helper;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;
using Reactive.Bindings;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface ISubTransactionViewModel : ISubTransIncViewModel {}

    /// <summary>
    /// The ViewModel of the Model SubTransaction.
    /// </summary>
    public class SubTransactionViewModel : SubTransIncViewModel, ISubTransactionViewModel
    {
        /// <summary>
        /// Initializes a SubTransactionViewModel.
        /// </summary>
        /// <param name="subIncome">A SubTransaction Model.</param>
        /// <param name="parent">The ViewModel of the Model's ParentTransaction.</param>
        /// <param name="orm">Used for the database accesses.</param>
        public SubTransactionViewModel(
            ISubTransaction subIncome,
            IBffOrm orm, 
            CategoryViewModelService categoryViewModelService,
            ParentTransactionViewModelService parentTransactionViewModelService) :
            base(subIncome, orm, categoryViewModelService)
        {
            Parent = 
                subIncome.ToReadOnlyReactivePropertyAsSynchronized(
                    st => st.Parent,
                    pt => parentTransactionViewModelService.GetViewModel(pt as IParentTransaction));
        }

        public override IReadOnlyReactiveProperty<IParentTransIncViewModel> Parent { get; }
    }
}