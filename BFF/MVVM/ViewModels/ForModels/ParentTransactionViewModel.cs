using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    class ParentTransactionViewModel : ParentTransIncViewModel<SubTransaction>
    {
        public ParentTransactionViewModel(ParentTransaction transInc, IBffOrm orm) : base(transInc, orm) {}

        protected override void InsertToDb()
        {
            Orm?.Insert(ParentTransInc as ParentTransaction);
        }

        protected override void UpdateToDb()
        {
            Orm?.Update(ParentTransInc as ParentTransaction);
        }

        protected override void DeleteFromDb()
        {
            Orm?.Delete(ParentTransInc as ParentTransaction);
        }

        #region Overrides of ParentTransIncViewModel<SubTransaction>

        protected override SubTransIncViewModel<SubTransaction> CreateNewSubViewModel(SubTransaction subElement)
        {
            return new SubTransactionViewModel(subElement, this, Orm);
        }

        public override SubTransaction CreateNewSubElement()
        {
            return new SubTransaction();
        }

        #endregion
    }
}
