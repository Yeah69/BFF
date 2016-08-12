﻿using BFF.DB;
using BFF.MVVM.Models.Native;

namespace BFF.MVVM.ViewModels.ForModels
{
    class TransactionViewModel : TransIncViewModel
    {
        public TransactionViewModel(Transaction transInc, IBffOrm orm) : base(transInc, orm) { }

        protected override void InsertToDb()
        {
            Orm?.Insert(TransInc as Transaction);
        }

        protected override void UpdateToDb()
        {
            Orm?.Update(TransInc as Transaction);
        }

        protected override void DeleteFromDb()
        {
            Orm?.Delete(TransInc as Transaction);
        }
    }
}
