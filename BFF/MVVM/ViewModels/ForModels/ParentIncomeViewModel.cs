using BFF.DB;
using BFF.MVVM.Models.Native;

namespace BFF.MVVM.ViewModels.ForModels
{
    class ParentIncomeViewModel : ParentTransIncViewModel<SubIncome>
    {
        public ParentIncomeViewModel(ParentIncome transInc, IBffOrm orm) : base(transInc, orm) { }

        #region Overrides of DbViewModelBase

        protected override void InsertToDb()
        {
            Orm?.Insert(TransInc as ParentIncome);
        }

        protected override void UpdateToDb()
        {
            Orm?.Update(TransInc as ParentIncome);
        }

        protected override void DeleteFromDb()
        {
            Orm?.Delete(TransInc as ParentIncome);
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
