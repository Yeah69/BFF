using BFF.DB;
using BFF.MVVM.Models.Native;

namespace BFF.MVVM.ViewModels.ForModels
{
    class TransactionViewModel : TransIncViewModel
    {
        public TransactionViewModel(Transaction transInc, IBffOrm orm) : base(transInc, orm) { }

        public override void Insert()
        {
            Orm?.Insert(TransInc as Transaction);
        }
    }
}
