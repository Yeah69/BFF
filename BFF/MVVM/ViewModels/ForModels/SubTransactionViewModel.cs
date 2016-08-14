using System.Windows.Input;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    class SubTransactionViewModel : SubTransIncViewModel<SubTransaction>
    {
        public SubTransactionViewModel(SubTransaction subTransInc, ParentTransIncViewModel<SubTransaction> parent, IBffOrm orm) :
                                           base(subTransInc, parent, orm) { }

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
