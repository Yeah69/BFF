using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using BFF.DB;
using BFF.Model.Native;
using BFF.Model.Native.Structure;
using BFF.WPFStuff;

namespace BFF.ViewModel
{
    public class TitViewModel : ObservableObject
    {
        public ObservableCollection<TitBase> Tits { get; set; } = new ObservableCollection<TitBase>();
        public ObservableCollection<TitBase> NewTits { get; set; } = new ObservableCollection<TitBase>();

        public string Header => _account?.Name ?? "All Accounts"; //todo: Localize "All Accounts"

        public long AccountBalance => _orm.GetAccountBalance(_account);

        public ObservableCollection<Account> AllAccounts => _orm.AllAccounts;

        public ObservableCollection<Payee> AllPayees => _orm.AllPayees;

        public ObservableCollection<Category> AllCategories => _orm.AllCategories;

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
        private readonly IBffOrm _orm;


        public TitViewModel(IBffOrm orm, Account account = null)
        {
             _orm = orm;
            _account = account;
            Tits = new ObservableCollection<TitBase>((account == null) ? orm.GetAllTits(): orm.GetAllTits(account));
        }
    }
}
