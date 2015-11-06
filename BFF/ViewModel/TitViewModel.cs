using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using BFF.DB.SQLite;
using BFF.Model.Native;
using BFF.Model.Native.Structure;
using BFF.WPFStuff;

namespace BFF.ViewModel
{
    public class TitViewModel : ObservableObject
    {
        public ObservableCollection<TitBase> Tits { get; set; }

        public long AccountBalance => SqLiteHelper.GetAccountBalance(_account);

        private readonly Account _account;

        public TitViewModel(Account account = null)
        {
            _account = account;
            Tits = new ObservableCollection<TitBase>((account == null) ? DB.SQLite.SqLiteHelper.GetAllTransactions(): DB.SQLite.SqLiteHelper.GetAllTransactions(account));
        }
    }
}
