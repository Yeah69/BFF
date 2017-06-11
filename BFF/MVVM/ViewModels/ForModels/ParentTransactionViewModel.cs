using System.Collections.Generic;
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
    public class ParentTransactionViewModel : ParentTransIncViewModel, IParentTransactionViewModel
    {
        private readonly IParentTransaction _parentTransaction;

        /// <summary>
        /// Initializes a ParentTransactionViewModel.
        /// </summary>
        /// <param name="transInc">A ParentTransaction Model.</param>
        /// <param name="orm">Used for the database accesses.</param>
        public ParentTransactionViewModel(IParentTransaction transInc, IBffOrm orm) : base(transInc, orm)
        {
            _parentTransaction = transInc;
        }

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
            return Orm.BffRepository.SubIncomeRepository.Create();
        }

        protected override IEnumerable<ISubTransInc> GetSubTransInc()
        {
            return Orm?.GetSubTransInc<SubTransaction>(_parentTransaction.Id);
        }

        #endregion
    }
}
