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
    public class TitContentViewModel : EmptyContentViewModel
    {
        protected TitViewModel _allAccountsViewModel;

        protected IBffOrm _orm;

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

        public Account NewAccount { get; set; } = new Account {Id = -1, Name = "", StartingBalance = 0L};
        
        public ICommand NewAccountCommand => new RelayCommand(param =>
        {
            _orm.Insert(NewAccount);
            NewAccount = new Account { Id = -1, Name = "", StartingBalance = 0L };
            OnPropertyChanged(nameof(NewAccount));
        }
        , param => !string.IsNullOrEmpty(NewAccount.Name));
        
        public Func<string, Payee> CreatePayeeFunc => name => 
        {
            Payee ret = new Payee {Name = name};
            _orm.Insert(ret);
            return ret;
        }; 

        public TitContentViewModel(IBffOrm orm)
        {
            _orm = orm;
            AllAccountsViewModel = new TitViewModel(_orm);
        }
    }
}
