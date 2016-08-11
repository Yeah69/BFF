using System;
using BFF.DB;
using BFF.MVVM.Models.Native;

namespace BFF.MVVM.ViewModels.ForModels
{
    class IncomeViewModel : TransIncViewModel
    {
        public IncomeViewModel(Income transInc, IBffOrm orm) : base(transInc, orm) { }

        public override void Insert()
        {
            Orm?.Insert(TransInc as Income);
        }
    }
}
