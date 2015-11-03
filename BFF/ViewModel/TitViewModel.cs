using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using BFF.Model.Native;
using BFF.Model.Native.Structure;
using BFF.WPFStuff;

namespace BFF.ViewModel
{
    public class TitViewModel : ObservableObject
    {
        public ObservableCollection<TransItemBase> Tits { get; set; }

        public TitViewModel(Account account = null)
        {
            Tits = new ObservableCollection<TransItemBase>((account == null) ? DB.SQLite.SqLiteHelper.GetAllTransactions(): DB.SQLite.SqLiteHelper.GetAllTransactions(account));
        }
    }
}
