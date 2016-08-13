using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    class ParentIncomeViewModel : ParentTransIncViewModel<SubIncome>
    {
        public ParentIncomeViewModel(ParentIncome transInc, IBffOrm orm) : base(transInc, orm) { }

        #region Overrides of DataModelViewModel

        protected override void InsertToDb()
        {
            Orm?.Insert(ParentTransInc as ParentIncome);
        }

        protected override void UpdateToDb()
        {
            Orm?.Update(ParentTransInc as ParentIncome);
        }

        protected override void DeleteFromDb()
        {
            Orm?.Delete(ParentTransInc as ParentIncome);
        }

        #endregion

        #region Overrides of ParentTransIncViewModel<SubIncome>

        protected override SubTransIncViewModel CreateNewSubViewModel(SubIncome subElement)
        {
            return new SubIncomeViewModel(subElement, Orm);
        }

        #endregion
    }
}
