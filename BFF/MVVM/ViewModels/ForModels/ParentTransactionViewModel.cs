using BFF.DB;
using BFF.MVVM.Models.Native;

namespace BFF.MVVM.ViewModels.ForModels
{
    class ParentTransactionViewModel : ParentTransIncViewModel<SubTransaction>
    {
        public ParentTransactionViewModel(ParentTransaction transInc, IBffOrm orm) : base(transInc, orm) {}

        protected override void InsertToDb()
        {
            Orm?.Insert(TransInc as ParentTransaction);
        }

        protected override void UpdateToDb()
        {
            Orm?.Update(TransInc as ParentTransaction);
        }

        protected override void DeleteFromDb()
        {
            Orm?.Delete(TransInc as ParentTransaction);
        }

        #region Overrides of ParentTransIncViewModel<SubTransaction>

        protected override SubTransIncViewModel CreateNewSubViewModel(SubTransaction subElement)
        {
            return new SubTransactionViewModel(subElement, Orm);
        }

        #endregion
    }
}
