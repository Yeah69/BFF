using System.Collections.Generic;
using BFF.Model.Native.Structure;
using BFF.WPFStuff;

namespace BFF.ViewModel
{
    class MainWindowViewModel : ViewModelBase
    {
        public List<ITransactionLike> AllTransactions => DB.SQLite.SqLiteHelper.GetAllTransactions();
    }
}
