using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    class IncomeViewModel : TransIncViewModel
    {
        public IncomeViewModel(Income transInc, IBffOrm orm) : base(transInc, orm) { }

        protected override void InsertToDb()
        {
            Orm?.Insert(TransInc as Income);
        }

        protected override void UpdateToDb()
        {
            Orm?.Update(TransInc as Income);
            Messenger.Default.Send(AllAccountMessage.Refresh);
            Messenger.Default.Send(AccountMessage.Refresh, Account);
        }

        protected override void DeleteFromDb()
        {
            Orm?.Delete(TransInc as Income);
        }
    }
}
