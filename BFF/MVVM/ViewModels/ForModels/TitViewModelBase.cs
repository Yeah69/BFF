using System;
using BFF.DB;

namespace BFF.MVVM.ViewModels.ForModels
{
    public abstract class TitViewModelBase : ObservableObject
    {
        protected IBffOrm Orm;

        public abstract string Memo { get; set; }
        public abstract long Sum { get; set; }

        public abstract bool ValidToInsert();

        public abstract void Insert();

        protected TitViewModelBase(IBffOrm orm)
        {
            Orm = orm;
        }
    }
}
