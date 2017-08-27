using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using AlphaChiTech.Virtualization;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface IAccountViewModel : IAccountBaseViewModel
    {
    }

    /// <summary>
    /// Tits can be added to an Account
    /// </summary>
    public class AccountViewModel : AccountBaseViewModel, IAccountViewModel
    {
        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public sealed override IReactiveProperty<long> StartingBalance { get; }

        /// <summary>
        /// Before a model object is inserted into the database, it has to be valid.
        /// This function should guarantee that the object is valid to be inserted.
        /// </summary>
        /// <returns>True if valid, else false</returns>
        public override bool ValidToInsert()
        {
            return !string.IsNullOrWhiteSpace(Name.Value);
        }

        /// <summary>
        /// Uses the OR mapper to insert the model into the database. Inner function for the Insert method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void OnInsert()
        {
            Messenger.Default.Send(SummaryAccountMessage.RefreshStartingBalance);
            Messenger.Default.Send(SummaryAccountMessage.RefreshBalance);
        }

        /// <summary>
        /// Initializes an AccountViewModel.
        /// </summary>
        /// <param name="account">An Account Model.</param>
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="summaryAccountViewModel">This account summarizes all accounts.</param>
        public AccountViewModel(IAccount account, IBffOrm orm, ISummaryAccountViewModel summaryAccountViewModel) 
            : base(orm, account)
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
            }, this);

            StartingBalance = account.ToReactivePropertyAsSynchronized(a => a.StartingBalance)
                                     .AddTo(CompositeDisposable);
            StartingBalance.Subscribe(_ => summaryAccountViewModel.RefreshStartingBalance())
                           .AddTo(CompositeDisposable);
            summaryAccountViewModel.RefreshStartingBalance();
        }

        #region ViewModel_Part

        /// <summary>
        /// Lazy loaded collection of TITs belonging to this Account.
        /// </summary>
        public override VirtualizingObservableCollection<ITitLikeViewModel> Tits => _tits ?? 
            (_tits = new VirtualizingObservableCollection<ITitLikeViewModel>(new PaginationManager<ITitLikeViewModel>(new PagedTitBaseProviderAsync(Orm.BffRepository.TitRepository, Orm, Account, Orm))));

        /// <summary>
        /// Collection of TITs, which are about to be inserted to this Account.
        /// </summary>
        public override ObservableCollection<ITitLikeViewModel> NewTits { get; } = new ObservableCollection<ITitLikeViewModel>();

        /// <summary>
        /// The current Balance of this Account.
        /// </summary>
        public override long? Balance => Orm?.GetAccountBalance(Account);

        /// <summary>
        /// Refreshes the Balance.
        /// </summary>
        public override void RefreshBalance()
        {
            if (IsOpen.Value)
            {
                OnPropertyChanged(nameof(Balance));
            }
        }

        /// <summary>
        /// Refreshes the TITs of this Account.
        /// </summary>
        public override void RefreshTits()
        {
            if(IsOpen.Value)
            {
                OnPreVirtualizedRefresh();
                _tits = new VirtualizingObservableCollection<ITitLikeViewModel>(new PaginationManager<ITitLikeViewModel>(new PagedTitBaseProviderAsync(Orm.BffRepository.TitRepository, Orm, Account, Orm)));
                OnPropertyChanged(nameof(Tits));
                OnPostVirtualizedRefresh();
            }
        }

        /// <summary>
        /// Creates a new Transaction.
        /// </summary>
        public override ICommand NewTransactionCommand => new RelayCommand(obj =>
        {
            ITransaction transaction = Orm.BffRepository.TransactionRepository.Create();
            transaction.Date = DateTime.Today;
            transaction.Account = Account;
            transaction.Memo = "";
            transaction.Sum = 0L;
            transaction.Cleared = false;
            NewTits.Add(new TransactionViewModel(
                transaction, 
                Orm,
                Orm.CommonPropertyProvider.AccountViewModelService,
                Orm.CommonPropertyProvider.PayeeViewModelService,
                Orm.CommonPropertyProvider.CategoryViewModelService));
        });

        /// <summary>
        /// Creates a new Income.
        /// </summary>
        public override ICommand NewIncomeCommand => new RelayCommand(obj =>
        {
            IIncome income = Orm.BffRepository.IncomeRepository.Create();
            income.Date = DateTime.Today;
            income.Account = Account;
            income.Memo = "";
            income.Sum = 0L;
            income.Cleared = false;
            NewTits.Add(new IncomeViewModel(
                income, 
                Orm,
                Orm.CommonPropertyProvider.AccountViewModelService,
                Orm.CommonPropertyProvider.PayeeViewModelService,
                Orm.CommonPropertyProvider.CategoryViewModelService));
        });

        /// <summary>
        /// Creates a new Transfer.
        /// </summary>
        public override ICommand NewTransferCommand => new RelayCommand(obj =>
        {
            ITransfer transfer = Orm.BffRepository.TransferRepository.Create();
            transfer.Date = DateTime.Today;
            transfer.Memo = "";
            transfer.Sum = 0L;
            transfer.Cleared = false;
            NewTits.Add(new TransferViewModel(transfer, Orm, Orm.CommonPropertyProvider.AccountViewModelService));
        });

        /// <summary>
        /// Creates a new ParentTransaction.
        /// </summary>
        public override ICommand NewParentTransactionCommand => new RelayCommand(obj =>
        {
            IParentTransaction parentTransaction = Orm.BffRepository.ParentTransactionRepository.Create();
            parentTransaction.Date = DateTime.Today;
            parentTransaction.Account = Account;
            parentTransaction.Memo = "";
            parentTransaction.Cleared = false;
            NewTits.Add(Orm.ParentTransactionViewModelService.GetViewModel(parentTransaction));
        });

        /// <summary>
        /// Creates a new ParentIncome.
        /// </summary>
        public override ICommand NewParentIncomeCommand => new RelayCommand(obj =>
        {
            IParentIncome parentIncome = Orm.BffRepository.ParentIncomeRepository.Create();
            parentIncome.Date = DateTime.Today;
            parentIncome.Account = Account;
            parentIncome.Memo = "";
            parentIncome.Cleared = false;
            NewTits.Add(Orm.ParentIncomeViewModelService.GetViewModel(parentIncome));
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
