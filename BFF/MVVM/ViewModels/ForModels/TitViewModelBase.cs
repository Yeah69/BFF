using BFF.DB;

namespace BFF.MVVM.ViewModels.ForModels
{
    public abstract class TitViewModelBase : DbViewModelBase
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
