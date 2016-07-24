using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using AlphaChiTech.Virtualization;
using BFF.DB;
using BFF.Model.Native;
using BFF.Model.Native.Structure;
using BFF.WPFStuff;

namespace BFF.ViewModel.ForModels
{
    /// <summary>
    /// Tits can be added to an Account
    /// </summary>
    public class AccountViewModel : AccountViewModelBase
    {
        public Action RefreshDataGrid;

        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public override long StartingBalance
        {
            get { return Account.StartingBalance; }
            set
            {
                Account.StartingBalance = value;
                OnPropertyChanged();
                Orm.CommonPropertyProvider.AllAccountsViewModel.RefreshStartingBalance();
            }
        }

        public override string Name
        {
            get { return Account.Name; }
            set
            {
                Account.Name = value;
                OnPropertyChanged();
            }
        }

        public long Id => Account.Id;

        /// <summary>
        /// Initializes the object
        /// </summary>
        public AccountViewModel(Account account, IBffOrm orm) : base(orm)
        {
            Account = account;
        }

        #region ViewModel_Part

        public override VirtualizingObservableCollection<TitBase> Tits => _tits ?? 
            (_tits = new VirtualizingObservableCollection<TitBase>(new PaginationManager<TitBase>(new PagedTitBaseProviderAsync(Orm, Account))));
        
        public override ObservableCollection<TitBase> NewTits { get; set; } = new ObservableCollection<TitBase>();

        public override long? Balance
        {
            get
            {
                return Orm?.GetAccountBalance(Account);
            }
            set { OnPropertyChanged(); }
        }
        public override void RefreshBalance()
        {
            OnPropertyChanged(nameof(Balance));
            //_allAccounts?.RefreshBalance(); todo
        }

        public override void RefreshTits()
        {
            OnPreVirtualizedRefresh();
            _tits = new VirtualizingObservableCollection<TitBase>(new PaginationManager<TitBase>(new PagedTitBaseProviderAsync(Orm, Account)));
            OnPropertyChanged(nameof(AccountViewModel.Tits));
            OnPostVirtualizedRefresh();
        }

        public override ICommand NewTransactionCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new Transaction(DateTime.Today, Account, memo: "", sum: 0L, cleared: false));
        });
        
        public override ICommand NewIncomeCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new Income(DateTime.Today, Account, memo: "", sum: 0L, cleared: false));
        });
        
        public override ICommand NewTransferCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new Transfer(DateTime.Today, memo: "", sum: 0L, cleared: false));
        });
        
        public override ICommand NewParentTransactionCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new ParentTransaction(DateTime.Today, Account, memo: "", cleared: false));
        });
        
        public override ICommand NewParentIncomeCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new ParentIncome(DateTime.Today, Account, memo: "", cleared: false));
        });
        
        public override ICommand ApplyCommand => new RelayCommand(obj =>
        {
            ApplyTits();
        }, obj => NewTits.Count > 0);

        #endregion
    }
}
