using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    /// <summary>
    /// The ViewModel of the Model SubIncome.
    /// </summary>
    class SubIncomeViewModel : SubTransIncViewModel<SubIncome>
    {
        /// <summary>
        /// Initializes a SubIncomeViewModel.
        /// </summary>
        /// <param name="subTransInc">A SubIncome Model.</param>
        /// <param name="parent">The ViewModel of the Model's ParentTransaction.</param>
        /// <param name="orm">Used for the database accesses.</param>
        public SubIncomeViewModel(SubIncome subTransInc, ParentTransIncViewModel<SubIncome> parent, IBffOrm orm) : 
            base(subTransInc, parent, orm) { }
        
        /// <summary>
        /// Uses the OR mapper to insert the model into the database. Inner function for the Insert method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void InsertToDb()
        {
            Orm?.Insert(SubTransInc as SubIncome);
        }

        /// <summary>
        /// Uses the OR mapper to update the model in the database. Inner function for the Update method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void UpdateToDb()
        {
            Orm?.Update(SubTransInc as SubIncome);
        }

        /// <summary>
        /// Uses the OR mapper to delete the model from the database. Inner function for the Delete method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void DeleteFromDb()
        {
            Orm?.Delete(SubTransInc as SubIncome);
        }
    }
}