using BFF.DB;
using BFF.MVVM.Models.Native;

namespace BFF.MVVM.ViewModels.ForModels
{
    class ParentTransactionViewModel : ParentTransIncViewModel<SubTransaction>
    {
        public ParentTransactionViewModel(ParentTransaction transInc, IBffOrm orm) : base(transInc, orm) {}
        
        public override void Insert()
        {
            Orm?.Insert(TransInc as ParentTransaction);
        }
    }
}
