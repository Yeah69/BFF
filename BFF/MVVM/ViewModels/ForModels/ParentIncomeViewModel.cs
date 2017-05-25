using System.Collections.Generic;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    internal interface IParentIncomeViewModel : IParentTransIncViewModel {}

    /// <summary>
    /// The ViewModel of the Model ParentIncome.
    /// </summary>
    public class ParentIncomeViewModel : ParentTransIncViewModel, IParentIncomeViewModel
    {
        private readonly IParentIncome _parentIncome;

        /// <summary>
        /// Initializes a ParentIncomeViewModel.
        /// </summary>
        /// <param name="transInc">A ParentIncome Model.</param>
        /// <param name="orm">Used for the database accesses.</param>
        public ParentIncomeViewModel(IParentIncome transInc, IBffOrm orm) : base(transInc, orm)
        {
            _parentIncome = transInc;
        }

        #region Overrides of ParentTransIncViewModel<SubIncome>

        /// <summary>
        /// Provides a ViewModel of the given SubElement.
        /// </summary>
        /// <param name="subElement">The SubElement, which gets a ViewModel.</param>
        /// <returns>A new ViewModel for a SubElement.</returns>
        protected override ISubTransIncViewModel CreateNewSubViewModel(ISubTransInc subElement)
        {
            return new SubIncomeViewModel(subElement as ISubIncome, this, Orm);
        }

        /// <summary>
        /// Provides a new SubElement.
        /// </summary>
        /// <returns>A new SubElement.</returns>
        public override ISubTransInc CreateNewSubElement()
        {
            return new SubIncome();
        }

        protected override IEnumerable<ISubTransInc> GetSubTransInc()
        {
            return Orm?.GetSubTransInc<SubIncome>(_parentIncome.Id);
        }

        #endregion
    }
}
