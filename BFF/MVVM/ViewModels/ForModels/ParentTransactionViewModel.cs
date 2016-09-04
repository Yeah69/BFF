using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    /// <summary>
    /// The ViewModel of the Model ParentTransaction.
    /// </summary>
    class ParentTransactionViewModel : ParentTransIncViewModel<SubTransaction>
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
        protected override SubTransIncViewModel<SubTransaction> CreateNewSubViewModel(SubTransaction subElement)
        {
            return new SubTransactionViewModel(subElement, this, Orm);
        }

        /// <summary>
        /// Provides a new SubElement.
        /// </summary>
        /// <returns>A new SubElement.</returns>
        public override SubTransaction CreateNewSubElement()
        {
            return new SubTransaction();
        }

        #endregion
    }
}
