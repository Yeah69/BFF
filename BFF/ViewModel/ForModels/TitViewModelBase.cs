using BFF.DB;
using BFF.WPFStuff;

namespace BFF.ViewModel.ForModels
{
    abstract class TitViewModelBase : ObservableObject
    {
        protected IBffOrm Orm;
        public abstract string Memo { get; set; }
        public abstract long Sum { get; set; }

        protected TitViewModelBase(IBffOrm orm)
        {
            Orm = orm;
        }
    }
}
