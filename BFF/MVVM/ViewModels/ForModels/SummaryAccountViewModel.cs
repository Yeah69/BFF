using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AlphaChiTech.Virtualization;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface ISummaryAccountViewModel : IAccountBaseViewModel {
        /// <summary>
        /// Refreshes the starting balance.
        /// This is needed for the summary account, because on run-time the user may add a new account.
        /// </summary>
        void RefreshStartingBalance();
    }

    /// <summary>
    /// Tits can be added to an Account
    /// </summary>
    public class SummaryAccountViewModel : AccountBaseViewModel, ISummaryAccountViewModel
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

        /// <summary>
        /// Name of the Account Model
        /// </summary>
        public override string Name //todo Localization
        {
            get { return "All Accounts"; }
            set { OnPropertyChanged(); }
        }

        /// <summary>
        /// Initializes an SummaryAccountViewModel.
        /// </summary>
        /// <param name="orm">Used for the database accesses.</param>
        public SummaryAccountViewModel(IBffOrm orm) : base(orm)
        {
            Account = new SummaryAccount();
            Messenger.Default.Register<SummaryAccountMessage>(this, message =>
            {
                switch (message)
                {
                    case SummaryAccountMessage.Refresh:
                        RefreshTits();
                        RefreshBalance();
                        RefreshStartingBalance();
                        break;
                    case SummaryAccountMessage.RefreshBalance:
                        RefreshBalance();
                        break;
                    case SummaryAccountMessage.RefreshStartingBalance:
                        RefreshStartingBalance();
                        break;
                    case SummaryAccountMessage.RefreshTits:
                        RefreshTits();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            });
        }

        /// <summary>
        /// Refreshes the Balance.
        /// </summary>
        public override void RefreshBalance()
        {
            OnPropertyChanged(nameof(Balance));
        }

        #region ViewModel_Part

        /// <summary>
        /// Lazy loaded collection of TITs belonging to this Account.
        /// </summary>
        public override VirtualizingObservableCollection<ITitLikeViewModel> Tits => _tits ?? 
            (_tits = new VirtualizingObservableCollection<ITitLikeViewModel>(new PaginationManager<ITitLikeViewModel>(new PagedTitBaseProviderAsync(Orm, null, Orm))));

        /// <summary>
        /// Collection of TITs, which are about to be inserted to this Account.
        /// </summary>
        public override ObservableCollection<ITitLikeViewModel> NewTits { get; set; } = new ObservableCollection<ITitLikeViewModel>();
        
        /// <summary>
        /// Refreshes the TITs of this Account.
        /// </summary>
        public override void RefreshTits()
        {
            OnPreVirtualizedRefresh();
            _tits = new VirtualizingObservableCollection<ITitLikeViewModel>(new PaginationManager<ITitLikeViewModel>(new PagedTitBaseProviderAsync(Orm, null, Orm)));
            OnPropertyChanged(nameof(Tits));
            OnPostVirtualizedRefresh();
        }

        /// <summary>
        /// The sum of all accounts balances.
        /// </summary>
        public override long? Balance => Orm?.GetSummaryAccountBalance();

        /// <summary>
        /// Creates a new Transaction.
        /// </summary>
        public override ICommand NewTransactionCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new TransactionViewModel(new Transaction(DateTime.Today, null, memo: "", sum: 0L, cleared: false), Orm));
        });

        /// <summary>
        /// Creates a new Income.
        /// </summary>
        public override ICommand NewIncomeCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new IncomeViewModel(new Income(DateTime.Today, null, memo: "", sum: 0L, cleared: false), Orm));
        });

        /// <summary>
        /// Creates a new Transfer.
        /// </summary>
        public override ICommand NewTransferCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new TransferViewModel(new Transfer(DateTime.Today, null, null, "", 0L, false), Orm));
        });

        /// <summary>
        /// Creates a new ParentTransaction.
        /// </summary>
        public override ICommand NewParentTransactionCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new ParentTransactionViewModel(new ParentTransaction(DateTime.Today, null, memo: "", cleared: false), Orm));
        });

        /// <summary>
        /// Creates a new ParentIncome.
        /// </summary>
        public override ICommand NewParentIncomeCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new ParentIncomeViewModel(new ParentIncome(DateTime.Today, null, memo: "", cleared: false), Orm));
        });

        /// <summary>
        /// Flushes all valid and not yet inserted TITs to the database.
        /// </summary>
        public override ICommand ApplyCommand => new RelayCommand(obj =>
        {
            ApplyTits();
            OnPropertyChanged(nameof(Balance));
        }, obj => NewTits.Count > 0);

        #endregion

        /// <summary>
        /// Refreshes the starting balance.
        /// This is needed for the summary account, because on run-time the user may add a new account.
        /// </summary>
        public void RefreshStartingBalance()
        {
            OnPropertyChanged(nameof(StartingBalance));
        }

        #region Overrides of DataModelViewModel

        /// <summary>
        /// Needed to mimic an Account.
        /// </summary>
        public override long Id => -1;

        /// <summary>
        /// Does only return False, because the summary account may not be inserted to the database. Needed to mimic an Account.
        /// </summary>
        /// <returns>Only False.</returns>
        public override bool ValidToInsert() => false;

        /// <summary>
        /// Does nothing, because this is the summary account. Needed to mimic an Account.
        /// </summary>
        protected override void InsertToDb() { }

        /// <summary>
        /// Does nothing, because this is the summary account. Needed to mimic an Account.
        /// </summary>
        protected override void UpdateToDb() { }

        /// <summary>
        /// Does nothing, because this is the summary account. Needed to mimic an Account.
        /// </summary>
        protected override void DeleteFromDb() { }

        #endregion
    }
}
