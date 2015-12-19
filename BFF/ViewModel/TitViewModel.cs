using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using BFF.DB.SQLite;
using BFF.Model.Native;
using BFF.Model.Native.Structure;
using BFF.WPFStuff;

namespace BFF.ViewModel
{
    public class TitViewModel : ObservableObject
    {
        public ObservableCollection<TitBase> Tits { get; set; } = new ObservableCollection<TitBase>();
        public ObservableCollection<TitBase> NewTits { get; set; } = new ObservableCollection<TitBase>();

        public long AccountBalance => SqLiteHelper.GetAccountBalance(_account);

        public ObservableCollection<Account> AllAccounts
        {
            get { return _allAccounts; }
            set
            {
                _allAccounts = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Payee> AllPayees
        {
            get { return _allPayees; }
            set
            {
                _allPayees = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Category> AllCategories
        {
            get { return _allCategories; }
            set
            {
                _allCategories = value;
                OnPropertyChanged();
            }
        }

        public ICommand NewTransactionCommand => new RelayCommand((obj) =>
        {
            NewTits.Add(new Transaction { Date = DateTime.Today, Memo = "", Sum=0L, Cleared = false, Account = _account});
        });

        public ICommand NewIncomeCommand => new RelayCommand((obj) =>
        {
            NewTits.Add(new Income { Date = DateTime.Today, Memo = "", Sum = 0L, Cleared = false, Account = _account });
        });

        public ICommand NewTransferCommand => new RelayCommand((obj) =>
        {
            NewTits.Add(new Transfer { Date = DateTime.Today, Memo = "", Sum = 0L, Cleared = false });
        });

        public ICommand ApplyCommand => new RelayCommand((obj) =>
        {
            foreach (TitBase tit in NewTits)
            {
                Tits.Add(tit);
            }
            OnPropertyChanged(nameof(Tits));//todo:Validate correctness and Save in DB, too
            NewTits.Clear(); 
        }, (obj) => NewTits.Count > 0);

        private readonly Account _account;
        private ObservableCollection<Account> _allAccounts;
        private ObservableCollection<Payee> _allPayees;
        private ObservableCollection<Category> _allCategories;

        public TitViewModel(ObservableCollection<Account> allAccounts, ObservableCollection<Payee> allPayees, ObservableCollection<Category> allCategories, Account account = null)
        {
            AllAccounts = allAccounts;
            AllPayees = allPayees;
            AllCategories = allCategories;
            _account = account;
            Tits = new ObservableCollection<TitBase>((account == null) ? SqLiteHelper.GetAllTransactions(): SqLiteHelper.GetAllTransactions(account));
        }
    }
}
