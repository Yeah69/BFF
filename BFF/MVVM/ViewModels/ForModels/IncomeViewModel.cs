using System;
using BFF.DB;
using BFF.MVVM.Models.Native;

namespace BFF.MVVM.ViewModels.ForModels
{
    class IncomeViewModel : TransIncViewModel
    {
        public IncomeViewModel(Income transInc, IBffOrm orm) : base(transInc, orm) { }

        protected override void InsertToDb()
        {
            Orm?.Insert(TransInc as Income);
        }

        protected override void UpdateToDb()
        {
            Orm?.Update(TransInc as Income);
        }

        protected override void DeleteFromDb()
        {
            Orm?.Delete(TransInc as Income);
        }
    }
}
