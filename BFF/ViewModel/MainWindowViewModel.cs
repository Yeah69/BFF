using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows.Input;
using BFF.DB;
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
        private bool _fileFlyoutIsOpen;
        private TitViewModel _allAccountsViewModel;
        private string _title;

        private IDb _database;

        public ObservableCollection<Account> AllAccounts => SqLiteHelper.AllAccounts;

        public ObservableCollection<Payee> AllPayees => SqLiteHelper.AllPayees;

        public ObservableCollection<Category> AllCategories => SqLiteHelper.AllCategories; 

        public TitViewModel AllAccountsViewModel
        {
            get { return _allAccountsViewModel; }
            set
            {
                _allAccountsViewModel = value;
                OnPropertyChanged();
            }
        }

        public Account NewAccount { get; set; } = new Account {Id = -1, Name = "", StartingBalance = 0L};

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public ICommand NewAccountCommand => new RelayCommand(param =>
        {
            SqLiteHelper.AddAccount(NewAccount);
            NewAccount = new Account { Id = -1, Name = "", StartingBalance = 0L };
            OnPropertyChanged(nameof(NewAccount));
        }
        , param => !string.IsNullOrEmpty(NewAccount.Name));

        public ICommand NewBudgetPlanCommand => new RelayCommand(param => NewBudgetPlan(), param => true);
        public ICommand OpenBudgetPlanCommand => new RelayCommand(param => OpenBudgetPlan(), param => true);
        public ICommand ImportBudgetPlanCommand => new RelayCommand(ImportBudgetPlan, param => true);

        public Func<string, Payee> CreatePayeeFunc => name => 
        {
            Payee ret = new Payee {Name = name};
            SqLiteHelper.InsertPayee(ret);
            return ret;
        }; 

        public MainWindowViewModel(IDb database)
        {
            _database = database;
            if (File.Exists(Settings.Default.DBLocation))
            {
                SqLiteHelper.OpenDatabase(Settings.Default.DBLocation);

                SetTabPages(Settings.Default.DBLocation);
            }
        }

        private void NewBudgetPlan()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Create a new Budget Plan", /* todo: Localize */
                Filter = "BFF Budget Plan (*.sqlite)|*.sqlite", /* todo: Localize? */
                DefaultExt = "*.sqlite"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                Reset();
                SqLiteHelper.CreateNewDatabase(saveFileDialog.FileName, CultureInfo.CurrentCulture);
                SetTabPages(saveFileDialog.FileName);

                Settings.Default.DBLocation = saveFileDialog.FileName;
                Settings.Default.Save();
            }
        }

        private void OpenBudgetPlan()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Open an existing Budget Plan", /* todo: Localize */
                Filter = "BFF Budget Plan (*.sqlite)|*.sqlite", /* todo: Localize? */
                DefaultExt = "*.sqlite"
            }; ;
            if (openFileDialog.ShowDialog() == true)
            {
                Reset();
                SqLiteHelper.OpenDatabase(openFileDialog.FileName);
                SetTabPages(openFileDialog.FileName);

                Settings.Default.DBLocation = openFileDialog.FileName;
                Settings.Default.Save();
            }
        }

        private void ImportBudgetPlan(object importableObject)
        {
            Reset();
            string savePath = ((IImportable) importableObject).Import();
            SqLiteHelper.OpenDatabase(savePath);
            SetTabPages(savePath);

            Settings.Default.DBLocation = savePath;
            Settings.Default.Save();
        }

        private void Reset()
        {
            AllAccountsViewModel = null;
            while(AllAccounts.Count > 0)
                AllAccounts.RemoveAt(0);
            Title = "BFF";
        }

        private void SetTabPages(string dbPath)
        {
            FileInfo fi = new FileInfo(dbPath);
            Title = $"BFF - {fi.Name}";

            AllAccountsViewModel = new TitViewModel(AllAccounts, AllPayees, AllCategories);
        }
    }
}
