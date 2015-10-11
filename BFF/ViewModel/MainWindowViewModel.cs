using System.Collections.Generic;
using BFF.Model.Native;
using BFF.WPFStuff;

namespace BFF.ViewModel
{
    class MainWindowViewModel : ViewModelBase
    {
        public List<Transaction> AllTransactions => DB.SQLite.Helper.GetAllTransactions();
    }
}
