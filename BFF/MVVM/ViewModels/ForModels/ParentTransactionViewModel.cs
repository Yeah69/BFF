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

        /// <summary>
        /// Uses the OR mapper to insert the model into the database. Inner function for the Insert method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void InsertToDb()
        {
            Orm?.Insert(ParentTransInc as ParentTransaction);
        }

        /// <summary>
        /// Uses the OR mapper to update the model in the database. Inner function for the Update method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void UpdateToDb()
        {
            Orm?.Update(ParentTransInc as ParentTransaction);
        }

        /// <summary>
        /// Uses the OR mapper to delete the model from the database. Inner function for the Delete method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void DeleteFromDb()
        {
            Orm?.Delete(ParentTransInc as ParentTransaction);
        }

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
