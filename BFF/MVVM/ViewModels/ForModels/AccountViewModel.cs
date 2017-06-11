using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using AlphaChiTech.Virtualization;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface IAccountViewModel : IAccountBaseViewModel
    {
        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        new long StartingBalance { get; set; }
    }

    /// <summary>
    /// Tits can be added to an Account
    /// </summary>
    public class AccountViewModel : AccountBaseViewModel, IAccountViewModel
    {
        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public override long StartingBalance
        {
            get => Account.StartingBalance;
            set
            {
                if(Account.StartingBalance == value) return;
                Account.StartingBalance = value;
                OnPropertyChanged();
                Orm.CommonPropertyProvider.SummaryAccountViewModel.RefreshStartingBalance();
            }
        }

        /// <summary>
        /// Before a model object is inserted into the database, it has to be valid.
        /// This function should guarantee that the object is valid to be inserted.
        /// </summary>
        /// <returns>True if valid, else false</returns>
        public override bool ValidToInsert()
        {
            return !string.IsNullOrWhiteSpace(Name);
        }

        /// <summary>
        /// Uses the OR mapper to insert the model into the database. Inner function for the Insert method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void InsertToDb()
        {
            CommonPropertyProvider.Add(Account);
            Messenger.Default.Send(SummaryAccountMessage.RefreshStartingBalance);
            Messenger.Default.Send(SummaryAccountMessage.RefreshBalance);
        }

        /// <summary>
        /// Uses the OR mapper to delete the model from the database. Inner function for the Delete method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void DeleteFromDb()
        {
            CommonPropertyProvider?.Remove(Account);
        }

        /// <summary>
        /// Initializes an AccountViewModel.
        /// </summary>
        /// <param name="account">An Account Model.</param>
        /// <param name="orm">Used for the database accesses.</param>
        public AccountViewModel(IAccount account, IBffOrm orm) : base(orm, account)
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

        /// <summary>
        /// Lazy loaded collection of TITs belonging to this Account.
        /// </summary>
        public override VirtualizingObservableCollection<ITitLikeViewModel> Tits => _tits ?? 
            (_tits = new VirtualizingObservableCollection<ITitLikeViewModel>(new PaginationManager<ITitLikeViewModel>(new PagedTitBaseProviderAsync(Orm, Account, Orm))));

        /// <summary>
        /// Collection of TITs, which are about to be inserted to this Account.
        /// </summary>
        public override ObservableCollection<ITitLikeViewModel> NewTits { get; set; } = new ObservableCollection<ITitLikeViewModel>();

        /// <summary>
        /// The current Balance of this Account.
        /// </summary>
        public override long? Balance => Orm?.GetAccountBalance(Account);

        /// <summary>
        /// Refreshes the Balance.
        /// </summary>
        public override void RefreshBalance()
        {
            OnPropertyChanged(nameof(Balance));
            Messenger.Default.Send(SummaryAccountMessage.RefreshBalance); //todo: Necsessary?
        }

        /// <summary>
        /// Refreshes the TITs of this Account.
        /// </summary>
        public override void RefreshTits()
        {
            OnPreVirtualizedRefresh();
            _tits = new VirtualizingObservableCollection<ITitLikeViewModel>(new PaginationManager<ITitLikeViewModel>(new PagedTitBaseProviderAsync(Orm, Account, Orm)));
            OnPropertyChanged(nameof(Tits));
            OnPostVirtualizedRefresh();
        }

        /// <summary>
        /// Creates a new Transaction.
        /// </summary>
        public override ICommand NewTransactionCommand => new RelayCommand(obj =>
        {
            Transaction transaction = Orm.BffRepository.TransactionRepository.Create();
            transaction.Date = DateTime.Today;
            transaction.AccountId = Account.Id;
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
            income.AccountId = Account.Id;
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
            parentTransaction.AccountId = Account.Id;
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
        }, obj => NewTits.Count > 0);

        #endregion
    }
}
