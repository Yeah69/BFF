using BFF.DB;
using BFF.MVVM.Models.Native;

namespace BFF.MVVM.ViewModels.ForModels
{
    class SubIncomeViewModel : SubTransIncViewModel
    {
        public SubIncomeViewModel(SubIncome subTransInc, IBffOrm orm) : base(subTransInc, orm) { }

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
