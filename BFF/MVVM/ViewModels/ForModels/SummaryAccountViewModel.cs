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
            get { return CommonPropertyProvider?.Accounts.Sum(account => account.StartingBalance) ?? 0L; }
            set => OnPropertyChanged();
        }

        /// <summary>
        /// Name of the Account Model
        /// </summary>
        public override string Name //todo Localization
        {
            get => "All Accounts";
            set => OnPropertyChanged();
        }

        /// <summary>
        /// Initializes an SummaryAccountViewModel.
        /// </summary>
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="summaryAccount">The model.</param>
        public SummaryAccountViewModel(IBffOrm orm, ISummaryAccount summaryAccount) : base(orm, summaryAccount)
        {
            Account = summaryAccount;
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
            Transaction transaction = Orm.BffRepository.TransactionRepository.Create();
            transaction.Date = DateTime.Today;
            transaction.Memo = "";
            transaction.Sum = 0L;
            transaction.Cleared = false;
            NewTits.Add(new TransactionViewModel(transaction, Orm));
        });

        /// <summary>
        /// Creates a new Income.
        /// </summary>
        public override ICommand NewIncomeCommand => new RelayCommand(obj =>
        {
            Income income = Orm.BffRepository.IncomeRepository.Create();
            income.Date = DateTime.Today;
            income.Memo = "";
            income.Sum = 0L;
            income.Cleared = false;
            NewTits.Add(new IncomeViewModel(income, Orm));
        });

        /// <summary>
        /// Creates a new Transfer.
        /// </summary>
        public override ICommand NewTransferCommand => new RelayCommand(obj =>
        {
            Transfer transfer = Orm.BffRepository.TransferRepository.Create();
            transfer.Date = DateTime.Today;
            transfer.Memo = "";
            transfer.Sum = 0L;
            transfer.Cleared = false;
            NewTits.Add(new TransferViewModel(transfer, Orm));
        });

        /// <summary>
        /// Creates a new ParentTransaction.
        /// </summary>
        public override ICommand NewParentTransactionCommand => new RelayCommand(obj =>
        {
            ParentTransaction parentTransaction = Orm.BffRepository.ParentTransactionRepository.Create();
            parentTransaction.Date = DateTime.Today;
            parentTransaction.Memo = "";
            parentTransaction.Cleared = false;
            NewTits.Add(new ParentTransactionViewModel(parentTransaction, Orm));
        });

        /// <summary>
        /// Creates a new ParentIncome.
        /// </summary>
        public override ICommand NewParentIncomeCommand => new RelayCommand(obj =>
        {
            ParentIncome parentIncome = Orm.BffRepository.ParentIncomeRepository.Create();
            parentIncome.Date = DateTime.Today;
            parentIncome.AccountId = Account.Id;
            parentIncome.Memo = "";
            parentIncome.Cleared = false;
            NewTits.Add(new ParentIncomeViewModel(parentIncome, Orm));
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

        #endregion
    }
}
