using BFF.DB;
using BFF.MVVM.Models.Native;

namespace BFF.MVVM.ViewModels.ForModels
{
    class ParentIncomeViewModel : ParentTransIncViewModel<SubIncome>
    {
        public ParentIncomeViewModel(ParentIncome transInc, IBffOrm orm) : base(transInc, orm) { }

        public override void Insert()
        {
            Orm?.Insert(TransInc as ParentIncome);
        }
    }
}
