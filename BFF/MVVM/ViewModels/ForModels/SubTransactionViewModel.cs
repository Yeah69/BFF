using BFF.DB;
using BFF.MVVM.Models.Native;

namespace BFF.MVVM.ViewModels.ForModels
{
    class SubTransactionViewModel : SubTransIncViewModel
    {
        public SubTransactionViewModel(SubTransaction subTransInc, IBffOrm orm) : base(subTransInc, orm) { }

        protected override void InsertToDb()
        {
            Orm?.Insert(SubTransInc as SubTransaction);
        }

        protected override void UpdateToDb()
        {
            Orm?.Update(SubTransInc as SubTransaction);
        }

        protected override void DeleteFromDb()
        {
            Orm?.Delete(SubTransInc as SubTransaction);
        }
    }
}
