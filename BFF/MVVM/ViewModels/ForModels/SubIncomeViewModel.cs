using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    class SubIncomeViewModel : SubTransIncViewModel<SubIncome>
    {
        public SubIncomeViewModel(SubIncome subTransInc, ParentTransIncViewModel<SubIncome> parent, IBffOrm orm) : 
            base(subTransInc, parent, orm) { }

        protected override void InsertToDb()
        {
            Orm?.Insert(SubTransInc as SubIncome);
        }

        protected override void UpdateToDb()
        {
            Orm?.Update(SubTransInc as SubIncome);
        }

        protected override void DeleteFromDb()
        {
            Orm?.Delete(SubTransInc as SubIncome);
        }
    }
}
