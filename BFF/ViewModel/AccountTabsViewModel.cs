using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using BFF.DB;
using BFF.Model.Native;
using BFF.WPFStuff;

namespace BFF.ViewModel
{
    public class AccountTabsViewModel : EmptyContentViewModel
    {
        protected AllAccounts _allAccountsViewModel;

        protected readonly IBffOrm _orm;

        public IBffOrm Orm
        {
            get { return _orm; }
        }

        public ObservableCollection<Account> AllAccounts => _orm.AllAccounts;

        public ObservableCollection<Category> AllCategories => _orm.AllCategories; 

        public AllAccounts AllAccountsViewModel
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
            NewAccount.RefreshBalance();
        }
        , param => !string.IsNullOrEmpty(NewAccount.Name));
        
        public Func<string, Payee> CreatePayeeFunc => name => 
        {
            Payee ret = new Payee {Name = name};
            _orm.Insert(ret);
            return ret;
        }; 

        public AccountTabsViewModel(IBffOrm orm)
        {
            _orm = orm;
            AllAccountsViewModel = Account.allAccounts;
        }
    }
}
