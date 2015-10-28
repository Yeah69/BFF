using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BFF.DB.SQLite;
using BFF.Model.Native;
using BFF.WPFStuff;
using WPFLocalizeExtension.Engine;

namespace BFF.ViewModel
{
    class MainWindowViewModel : ViewModelBase
    {

        public ObservableCollection<TabItem> TabItems { get; set; }

        public TransUcViewModel AllAccsViewModel => new TransUcViewModel();

        public MainWindowViewModel()
        {
            IEnumerable<Account> accounts = SqLiteHelper.GetAllAccounts();

            TransUcViewModel viewModel = AllAccsViewModel;
            TabItems = new ObservableCollection<TabItem>{ new TabItem {Header = null, ViewModel = viewModel} };

            foreach (Account account in accounts)
            {
                TabItems.Add(new TabItem {Header = account.Name, ViewModel = new TransUcViewModel(account)});
                // todo: Account specific viewModels
            }
        }
    }

    class TabItem
    {
        public string Header { get; set; }
        public TransUcViewModel ViewModel { get; set; }
    }
}
