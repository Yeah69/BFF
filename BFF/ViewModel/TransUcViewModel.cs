using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using BFF.Model.Native;
using BFF.Model.Native.Structure;
using BFF.WPFStuff;

namespace BFF.ViewModel
{
    class TransUcViewModel : ViewModelBase
    {
        public ObservableCollection<TransItemBase> AllTransactions { get; set; }

        public CollectionViewSource ViewSource { get; set; }

        public TransUcViewModel(Account account = null)
        {
            AllTransactions = new ObservableCollection<TransItemBase>((account == null) ? DB.SQLite.SqLiteHelper.GetAllTransactions(): DB.SQLite.SqLiteHelper.GetAllTransactions(account));
            ViewSource = new CollectionViewSource {Source = AllTransactions};
            //ViewSource.SortDescriptions.Add(new SortDescription("Date", ListSortDirection.Ascending));
            //ViewSource.View.Refresh();
        }
    }
}
