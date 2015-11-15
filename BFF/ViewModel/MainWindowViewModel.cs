using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
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
        private bool _fileFlyoutIsOpen;
        private TitViewModel _allAccountsViewModel;

        public ObservableCollection<Account> AllAccounts => SqLiteHelper.AllAccounts;

        public ObservableCollection<Payee> AllPayees => SqLiteHelper.AllPayees;

        public ObservableCollection<Category> AllCategoryRoots => SqLiteHelper.AllCategoryRoots; 

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

        public MainWindowViewModel()
        {
            //AllAccounts = new ObservableCollection<Account>();
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
                SqLiteHelper.CreateNewDatabase(saveFileDialog.FileName, CultureInfo.CurrentCulture);
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
            AllAccountsViewModel = null;
            AllAccounts.Clear();
        }

        private void SetTabPages()
        {

            //todo: maybe just ref to AllAccounts from SqlLiteHelper
            //IEnumerable<Account> accounts = SqLiteHelper.AllAccounts;

            AllAccountsViewModel = new TitViewModel();

            /*foreach (Account account in accounts)
            {
                AllAccounts.Add(account);
            }*/
        }
    }
}
