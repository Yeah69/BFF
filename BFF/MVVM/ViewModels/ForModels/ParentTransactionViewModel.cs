using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    internal interface IParentTransactionViewModel : IParentTransIncViewModel {}

    /// <summary>
    /// The ViewModel of the Model ParentTransaction.
    /// </summary>
    class ParentTransactionViewModel : ParentTransIncViewModel, IParentTransactionViewModel
    {
        /// <summary>
        /// Initializes a ParentTransactionViewModel.
        /// </summary>
        /// <param name="transInc">A ParentTransaction Model.</param>
        /// <param name="orm">Used for the database accesses.</param>
        public ParentTransactionViewModel(IParentTransaction transInc, IBffOrm orm) : base(transInc, orm) {}

        #region Overrides of ParentTransIncViewModel<SubTransaction>

        /// <summary>
        /// Provides a ViewModel of the given SubElement.
        /// </summary>
        /// <param name="subElement">The SubElement, which gets a ViewModel.</param>
        /// <returns>A new ViewModel for a SubElement.</returns>
        protected override ISubTransIncViewModel CreateNewSubViewModel(ISubTransInc subElement)
        {
            return new SubTransactionViewModel(subElement as ISubTransaction, this, Orm);
        }

        /// <summary>
        /// Provides a new SubElement.
        /// </summary>
        /// <returns>A new SubElement.</returns>
        public override ISubTransInc CreateNewSubElement()
        {
            return new SubTransaction();
        }

        #endregion
    }
}
