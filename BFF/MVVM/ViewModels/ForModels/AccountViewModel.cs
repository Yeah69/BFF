using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using AlphaChiTech.Virtualization;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    /// <summary>
    /// Tits can be added to an Account
    /// </summary>
    public class AccountViewModel : AccountBaseViewModel
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

        public override long Id => Account.Id;
        internal override bool ValidToInsert()
        {
            return !string.IsNullOrWhiteSpace(Name);
        }

        protected override void InsertToDb()
        {
            Orm?.Insert(Account);
        }

        protected override void UpdateToDb()
        {
            Orm?.Update(Account);
        }

        protected override void DeleteFromDb()
        {
            Orm?.Delete(Account);
        }

        /// <summary>
        /// Initializes the object
        /// </summary>
        public AccountViewModel(Account account, IBffOrm orm) : base(orm)
        {
            Account = account;
            Messenger.Default.Register<AccountMessage>(this, message =>
            {
                switch(message)
                {
                    case AccountMessage.Refresh:
                        RefreshTits();
                        RefreshBalance();
                        break;
                    case AccountMessage.RefreshBalance:
                        RefreshBalance();
                        break;
                    case AccountMessage.RefreshTits:
                        RefreshTits();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }, Account);
        }

        #region ViewModel_Part

        public override VirtualizingObservableCollection<TitLikeViewModel> Tits => _tits ?? 
            (_tits = new VirtualizingObservableCollection<TitLikeViewModel>(new PaginationManager<TitLikeViewModel>(new PagedTitBaseProviderAsync(Orm, Account, Orm))));
        
        public override ObservableCollection<TitLikeViewModel> NewTits { get; set; } = new ObservableCollection<TitLikeViewModel>();

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
            Messenger.Default.Send(AllAccountMessage.RefreshBalance);
        }

        public override void RefreshTits()
        {
            OnPreVirtualizedRefresh();
            _tits = new VirtualizingObservableCollection<TitLikeViewModel>(new PaginationManager<TitLikeViewModel>(new PagedTitBaseProviderAsync(Orm, Account, Orm)));
            OnPropertyChanged(nameof(Tits));
            OnPostVirtualizedRefresh();
        }

        public override ICommand NewTransactionCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new TransactionViewModel(new Transaction(DateTime.Today, Account, memo: "", sum: 0L, cleared: false), Orm));
        });
        
        public override ICommand NewIncomeCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new IncomeViewModel(new Income(DateTime.Today, Account, memo: "", sum: 0L, cleared: false), Orm));
        });
        
        public override ICommand NewTransferCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new TransferViewModel(new Transfer(DateTime.Today, memo: "", sum: 0L, cleared: false), Orm));
        });
        
        public override ICommand NewParentTransactionCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new ParentTransactionViewModel(new ParentTransaction(DateTime.Today, Account, memo: "", cleared: false), Orm));
        });
        
        public override ICommand NewParentIncomeCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new ParentIncomeViewModel(new ParentIncome(DateTime.Today, Account, memo: "", cleared: false), Orm));
        });
        
        public override ICommand ApplyCommand => new RelayCommand(obj =>
        {
            ApplyTits();
        }, obj => NewTits.Count > 0);

        #endregion
    }
}
