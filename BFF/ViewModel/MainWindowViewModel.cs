using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection.Emit;
using System.Windows.Input;
using BFF.DB.SQLite;
using BFF.Helper.Import;
using BFF.Model.Native;
using BFF.Properties;
using BFF.WPFStuff;
using Microsoft.Win32;

namespace BFF.ViewModel
{
    public class MainWindowViewModel : ObservableObject
    {
        private ObservableCollection<TabItem> _tabItems;
        private bool _fileFlyoutIsOpen;

        public ObservableCollection<TabItem> TabItems
        {
            get { return _tabItems; }
            set { _tabItems = value; }
        }

        public ICommand NewBudgetPlanCommand => new RelayCommand(param => NewBudgetPlan(), param => true);
        public ICommand OpenBudgetPlanCommand => new RelayCommand(param => OpenBudgetPlan(), param => true);
        public ICommand ImportBudgetPlanCommand => new RelayCommand(ImportBudgetPlan, param => true);

        public MainWindowViewModel()
        {
            //NewBudgetPlanCommand = new RelayCommand(param => OpenBudgetPlan(), param => true);
            //OnPropertyChanged(nameof(NewBudgetPlanCommand));
            TabItems = new ObservableCollection<TabItem>();
            if (File.Exists(Settings.Default.DBLocation))
            {
                SqLiteHelper.OpenDatabase(Settings.Default.DBLocation);
                SetTabPages();
            }
        }

        private void NewBudgetPlan()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                this.Reset();
                SqLiteHelper.CreateNewDatabase(saveFileDialog.FileName);
                SetTabPages();

                Settings.Default.DBLocation = saveFileDialog.FileName;
                Settings.Default.Save();
            }
        }

        private void OpenBudgetPlan()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                this.Reset();
                SqLiteHelper.OpenDatabase(openFileDialog.FileName);
                SetTabPages();

                Settings.Default.DBLocation = openFileDialog.FileName;
                Settings.Default.Save();
            }
        }

        private void ImportBudgetPlan(object importableObject)
        {
            this.Reset();
            string savePath = ((IImportable) importableObject).Import();
            SqLiteHelper.OpenDatabase(savePath);
            SetTabPages();

            Settings.Default.DBLocation = savePath;
            Settings.Default.Save();
        }

        private void Reset()
        {
            TabItems.Clear();
        }

        private void SetTabPages()
        {
            IEnumerable<Account> accounts = SqLiteHelper.GetAllAccounts();

            TransUcViewModel viewModel = new TransUcViewModel();
            TabItems.Add(new TabItem { Header = null, ViewModel = viewModel });

            foreach (Account account in accounts)
            {
                TabItems.Add(new TabItem { Header = account.Name, ViewModel = new TransUcViewModel(account) });
            }
        }
    }

    public class TabItem
    {
        public string Header { get; set; }
        public TransUcViewModel ViewModel { get; set; }
    }
}
