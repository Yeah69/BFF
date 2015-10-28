using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using BFF.DB.SQLite;
using BFF.Model.Native;
using BFF.Properties;
using BFF.WPFStuff;
using WPFLocalizeExtension.Engine;

namespace BFF.ViewModel
{
    class MainWindowViewModel : ViewModelBase
    {

        public ObservableCollection<TabItem> TabItems { get; set; }

        public MainWindowViewModel()
        {
            if (File.Exists(Settings.Default.DBLocation))
            {
                SqLiteHelper.OpenDatabase(Settings.Default.DBLocation);
                SetTabPages();
            }
        }

        private void SetTabPages()
        {
            IEnumerable<Account> accounts = SqLiteHelper.GetAllAccounts();

            TransUcViewModel viewModel = new TransUcViewModel();
            TabItems = new ObservableCollection<TabItem> { new TabItem { Header = null, ViewModel = viewModel } };

            foreach (Account account in accounts)
            {
                TabItems.Add(new TabItem { Header = account.Name, ViewModel = new TransUcViewModel(account) });
            }
        }
    }

    class TabItem
    {
        public string Header { get; set; }
        public TransUcViewModel ViewModel { get; set; }
    }
}
