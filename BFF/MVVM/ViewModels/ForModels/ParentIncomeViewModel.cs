using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    /// <summary>
    /// The ViewModel of the Model ParentIncome.
    /// </summary>
    class ParentIncomeViewModel : ParentTransIncViewModel<SubIncome>
    {
        /// <summary>
        /// Initializes a ParentIncomeViewModel.
        /// </summary>
        /// <param name="transInc">A ParentIncome Model.</param>
        /// <param name="orm">Used for the database accesses.</param>
        public ParentIncomeViewModel(IParentIncome transInc, IBffOrm orm) : base(transInc, orm) { }

        #region Overrides of ParentTransIncViewModel<SubIncome>

        /// <summary>
        /// Provides a ViewModel of the given SubElement.
        /// </summary>
        /// <param name="subElement">The SubElement, which gets a ViewModel.</param>
        /// <returns>A new ViewModel for a SubElement.</returns>
        protected override SubTransIncViewModel<SubIncome> CreateNewSubViewModel(SubIncome subElement)
        {
            return new SubIncomeViewModel(subElement, this, Orm);
        }

        /// <summary>
        /// Provides a new SubElement.
        /// </summary>
        /// <returns>A new SubElement.</returns>
        public override SubIncome CreateNewSubElement()
        {
            return new SubIncome();
        }

        #endregion
    }
}
