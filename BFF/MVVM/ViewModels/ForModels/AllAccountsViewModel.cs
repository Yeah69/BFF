using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AlphaChiTech.Virtualization;
using BFF.DB;
using BFF.MVVM.Models.Native;

namespace BFF.MVVM.ViewModels.ForModels
{
    /// <summary>
    /// Tits can be added to an Account
    /// </summary>
    public class AllAccountsViewModel : AccountViewModelBase
    {
        private long _id;

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

        public override VirtualizingObservableCollection<TitViewModelBase> Tits => _tits ?? 
            (_tits = new VirtualizingObservableCollection<TitViewModelBase>(new PaginationManager<TitViewModelBase>(new PagedTitBaseProviderAsync(Orm, null, Orm))));
        
        public override ObservableCollection<TitViewModelBase> NewTits { get; set; } = new ObservableCollection<TitViewModelBase>();
        
        public override void RefreshTits()
        {
            OnPreVirtualizedRefresh();
            _tits = new VirtualizingObservableCollection<TitViewModelBase>(new PaginationManager<TitViewModelBase>(new PagedTitBaseProviderAsync(Orm, null, Orm)));
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
            NewTits.Add(new TransactionViewModel(new Transaction(DateTime.Today, null, memo: "", sum: 0L, cleared: false), Orm));
        });
        
        public override ICommand NewIncomeCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new IncomeViewModel(new Income(DateTime.Today, null, memo: "", sum: 0L, cleared: false), Orm));
        });

        public override ICommand NewTransferCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new TransferViewModel(new Transfer(DateTime.Today, null, null, "", 0L, false), Orm));
        });

        public override ICommand NewParentTransactionCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new ParentTransactionViewModel(new ParentTransaction(DateTime.Today, null, memo: "", cleared: false), Orm));
        });
        
        public override ICommand NewParentIncomeCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new ParentIncomeViewModel(new ParentIncome(DateTime.Today, null, memo: "", cleared: false), Orm));
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

        #region Overrides of DbViewModelBase

        public override long Id => -3;

        internal override bool ValidToInsert() => false;

        protected override void InsertToDb() { }

        protected override void UpdateToDb() { }

        protected override void DeleteFromDb() { }

        #endregion
    }
}
