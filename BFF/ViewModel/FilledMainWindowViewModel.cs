using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using BFF.DB;
using BFF.Model.Native;
using BFF.Properties;
using BFF.WPFStuff;

namespace BFF.ViewModel
{
    public class FilledMainWindowViewModel : ObservableObject
    {
        protected TitViewModel _allAccountsViewModel;

        protected readonly IBffOrm _orm;
        protected string _title;

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Account> AllAccounts => _orm.AllAccounts;

        public ObservableCollection<Payee> AllPayees => _orm.AllPayees;

        public ObservableCollection<Category> AllCategories => _orm.AllCategories;

        public TitViewModel AllAccountsViewModel
        {
            get { return _allAccountsViewModel; }
            set
            {
                _allAccountsViewModel = value;
                OnPropertyChanged();
            }
        }

        public Account NewAccount { get; set; } = new Account { Id = -1, Name = "", StartingBalance = 0L };

        public ICommand NewAccountCommand => new RelayCommand(param =>
        {
            _orm.Insert(NewAccount);
            NewAccount = new Account { Id = -1, Name = "", StartingBalance = 0L };
            OnPropertyChanged(nameof(NewAccount));
        }
        , param => !string.IsNullOrEmpty(NewAccount.Name));

        public Func<string, Payee> CreatePayeeFunc => name =>
        {
            Payee ret = new Payee { Name = name };
            _orm.Insert(ret);
            return ret;
        };

        public FilledMainWindowViewModel(IBffOrm orm) : base()
        {
            _orm = orm;
            if (File.Exists(Settings.Default.DBLocation))
            {
                //SqLiteHelper.OpenDatabase(Settings.Default.DBLocation); todo

                SetTabPages(Settings.Default.DBLocation);
            }
        }

        private void Reset()
        {
            AllAccountsViewModel = null;
            while (AllAccounts.Count > 0)
                AllAccounts.RemoveAt(0);
        }

        private void SetTabPages(string dbPath)
        {
            FileInfo fi = new FileInfo(dbPath);
            Title = $"BFF - {fi.Name}";

            AllAccountsViewModel = new TitViewModel(_orm);
        }
    }
}
