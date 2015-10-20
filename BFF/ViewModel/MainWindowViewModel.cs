using System.Collections.Generic;
using System.Data.SQLite;
using BFF.Model.Native;
using BFF.Model.Native.Structure;
using BFF.WPFStuff;
using Dapper.Contrib.Extensions;

namespace BFF.ViewModel
{
    class MainWindowViewModel : ViewModelBase
    {
        public List<ITransactionLike> AllTransactions => BFF.DB.SQLite.Helper.GetAllTransactions();
    }
}
