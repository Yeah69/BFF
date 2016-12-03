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
            get { return Account.StartingBalance; }
            set
            {
                if(Account.StartingBalance == value) return;
                Account.StartingBalance = value;
                Account.Update(Orm);
                OnPropertyChanged();
                Orm.CommonPropertyProvider.SummaryAccountViewModel.RefreshStartingBalance();
            }
        }

        /// <summary>
        /// Name of the Account Model
        /// </summary>
        public override string Name
        {
            get { return Account.Name; }
            set
            {
                if(Account.Name == value) return;
                Account.Name = value;
                Account.Update(Orm);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The object's Id in the table of the database.
        /// </summary>
        public override long Id => Account.Id;

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
            Orm?.CommonPropertyProvider.Add(Account);
            Messenger.Default.Send(SummaryAccountMessage.RefreshStartingBalance);
            Messenger.Default.Send(SummaryAccountMessage.RefreshBalance);
        }

        /// <summary>
        /// Uses the OR mapper to update the model in the database. Inner function for the Update method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void UpdateToDb()
        {
            Account.Update(Orm);
        }

        /// <summary>
        /// Uses the OR mapper to delete the model from the database. Inner function for the Delete method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void DeleteFromDb()
        {
            Orm?.CommonPropertyProvider?.Remove(Account);
        }

        /// <summary>
        /// Initializes an AccountViewModel.
        /// </summary>
        /// <param name="account">An Account Model.</param>
        /// <param name="orm">Used for the database accesses.</param>
        public AccountViewModel(IAccount account, IBffOrm orm) : base(orm)
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
            NewTits.Add(new TransactionViewModel(new Transaction(DateTime.Today, Account, memo: "", sum: 0L, cleared: false), Orm));
        });

        /// <summary>
        /// Creates a new Income.
        /// </summary>
        public override ICommand NewIncomeCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new IncomeViewModel(new Income(DateTime.Today, Account, memo: "", sum: 0L, cleared: false), Orm));
        });

        /// <summary>
        /// Creates a new Transfer.
        /// </summary>
        public override ICommand NewTransferCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new TransferViewModel(new Transfer(DateTime.Today, memo: "", sum: 0L, cleared: false), Orm));
        });

        /// <summary>
        /// Creates a new ParentTransaction.
        /// </summary>
        public override ICommand NewParentTransactionCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new ParentTransactionViewModel(new ParentTransaction(DateTime.Today, Account, memo: "", cleared: false), Orm));
        });

        /// <summary>
        /// Creates a new ParentIncome.
        /// </summary>
        public override ICommand NewParentIncomeCommand => new RelayCommand(obj =>
        {
            NewTits.Add(new ParentIncomeViewModel(new ParentIncome(DateTime.Today, Account, memo: "", cleared: false), Orm));
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
