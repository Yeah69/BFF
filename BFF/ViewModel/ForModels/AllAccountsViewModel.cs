using System;
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
    public class AllAccountsViewModel : AccountViewModelBase
    {
        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public override long StartingBalance
        {
            get { return Orm?.CommonPropertyProvider?.Accounts.Sum(account => account.StartingBalance) ?? 0L; }
            set
            {
                OnPropertyChanged();
            }
        }

        public override string Name //todo Localization
        {
            get { return "All Accounts"; }
            set { OnPropertyChanged(); }
        }
        
        /// <summary>
        /// Initializes the object
        /// </summary>
        public AllAccountsViewModel(IBffOrm orm) : base(orm)
        {
            Account = new AllAccounts();
            Messenger.Default.Register<AllAccountMessage>(this, message =>
            {
                switch (message)
                {
                    case AllAccountMessage.Refresh:
                        RefreshTits();
                        RefreshBalance();
                        RefreshStartingBalance();
                        break;
                    case AllAccountMessage.RefreshBalance:
                        RefreshBalance();
                        break;
                    case AllAccountMessage.RefreshStartingBalance:
                        RefreshStartingBalance();
                        break;
                    case AllAccountMessage.RefreshTits:
                        RefreshTits();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            });
        }

        public override void RefreshBalance()
        {
            OnPropertyChanged(nameof(Balance));
        }

        #region ViewModel_Part

        public override VirtualizingObservableCollection<TitBase> Tits => _tits ?? 
            (_tits = new VirtualizingObservableCollection<TitBase>(new PaginationManager<TitBase>(new PagedTitBaseProviderAsync(Orm, null))));
        
        public override ObservableCollection<TitBase> NewTits { get; set; } = new ObservableCollection<TitBase>();
        
        public override void RefreshTits()
        {
            OnPreVirtualizedRefresh();
            _tits = new VirtualizingObservableCollection<TitBase>(new PaginationManager<TitBase>(new PagedTitBaseProviderAsync(Orm, null)));
            OnPropertyChanged(nameof(Tits));
            OnPostVirtualizedRefresh();
        }

        public override long? Balance
        {
            get { return Orm?.CommonPropertyProvider?.AccountViewModels?.Sum(account => account.Balance); }
            set { OnPropertyChanged(); }
        }
        
        public override ICommand NewTransactionCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new Transaction(DateTime.Today, null, memo: "", sum: 0L, cleared: false));
        });
        
        public override ICommand NewIncomeCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new Income(DateTime.Today, null, memo: "", sum: 0L, cleared: false));
        });

        public override ICommand NewTransferCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new Transfer(DateTime.Today, null, null, "", 0L, false));
        });

        public override ICommand NewParentTransactionCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new ParentTransaction(DateTime.Today, null, memo: "", cleared: false));
        });
        
        public override ICommand NewParentIncomeCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new ParentIncome(DateTime.Today, null, memo: "", cleared: false));
        });
        
        public override ICommand ApplyCommand => new RelayCommand(obj =>
        {
            ApplyTits();
            OnPropertyChanged(nameof(Balance));
        }, obj => NewTits.Count > 0);

        #endregion

        public void RefreshStartingBalance()
        {
            OnPropertyChanged(nameof(StartingBalance));
        }
    }
}
