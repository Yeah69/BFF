using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    internal interface ISubTransactionViewModel : ISubTransIncViewModel {}

    /// <summary>
    /// The ViewModel of the Model SubTransaction.
    /// </summary>
    public class SubTransactionViewModel : SubTransIncViewModel, ISubTransactionViewModel
    {
        /// <summary>
        /// Initializes a SubTransactionViewModel.
        /// </summary>
        /// <param name="subTransInc">A SubTransaction Model.</param>
        /// <param name="parent">The ViewModel of the Model's ParentTransaction.</param>
        /// <param name="orm">Used for the database accesses.</param>
        public SubTransactionViewModel(ISubTransaction subTransInc, IParentTransIncViewModel parent, IBffOrm orm) :
                                           base(subTransInc, parent, orm) { }
    }
}