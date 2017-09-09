using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AlphaChiTech.Virtualization;
using BFF.DB;
using BFF.DB.Dapper.ModelRepositories;
using BFF.Helper;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

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
        private readonly Subject<long> _startingBalanceSubject = new Subject<long>();
        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public sealed override IReactiveProperty<long> StartingBalance { get; }

        /// <summary>
        /// Name of the Account Model
        /// </summary>
        public override IReactiveProperty<string> Name //todo Localization
            => new ReactiveProperty<string>("All Accounts");

        /// <summary>
        /// Initializes an SummaryAccountViewModel.
        /// </summary>
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="summaryAccount">The model.</param>
        /// <param name="repository">Repository for accounts.</param>
        public SummaryAccountViewModel(IBffOrm orm,
                                       ISummaryAccount summaryAccount,
                                       IAccountRepository repository) : base(orm, summaryAccount)
        {
            IsOpen.Value = true;
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

            StartingBalance = new ReactiveProperty<long>(
                repository.All.ToObservable().Select(
                    a => repository.All.Sum(acc => acc.StartingBalance)))
                .AddTo(CompositeDisposable);

            NewTransactionCommand.Subscribe(_ =>
            {
                ITransaction transaction = Orm.BffRepository.TransactionRepository.Create();
                transaction.Date = DateTime.Today;
                transaction.Memo = "";
                transaction.Sum = 0L;
                transaction.Cleared = false;
                NewTits.Add(new TransactionViewModel(
                    transaction,
                    hcvm => new NewCategoryViewModel(hcvm, Orm.BffRepository.CategoryRepository, CommonPropertyProvider.CategoryViewModelService),
                    Orm,
                    Orm.CommonPropertyProvider.AccountViewModelService,
                    Orm.CommonPropertyProvider.PayeeViewModelService,
                    Orm.CommonPropertyProvider.CategoryViewModelService));
            }).AddTo(CompositeDisposable);

            NewIncomeCommand.Subscribe(_ =>
            {
                IIncome income = Orm.BffRepository.IncomeRepository.Create();
                income.Date = DateTime.Today;
                income.Memo = "";
                income.Sum = 0L;
                income.Cleared = false;
                NewTits.Add(new IncomeViewModel(
                    income,
                    hcvm => new NewCategoryViewModel(hcvm, Orm.BffRepository.CategoryRepository, CommonPropertyProvider.CategoryViewModelService),
                    Orm,
                    Orm.CommonPropertyProvider.AccountViewModelService,
                    Orm.CommonPropertyProvider.PayeeViewModelService,
                    Orm.CommonPropertyProvider.CategoryViewModelService));
            }).AddTo(CompositeDisposable);

            NewTransferCommand.Subscribe(_ =>
            {
                ITransfer transfer = Orm.BffRepository.TransferRepository.Create();
                transfer.Date = DateTime.Today;
                transfer.Memo = "";
                transfer.Sum = 0L;
                transfer.Cleared = false;
                NewTits.Add(new TransferViewModel(transfer, Orm, Orm.CommonPropertyProvider.AccountViewModelService));
            }).AddTo(CompositeDisposable);

            NewParentTransactionCommand.Subscribe(_ =>
            {
                IParentTransaction parentTransaction = Orm.BffRepository.ParentTransactionRepository.Create();
                parentTransaction.Date = DateTime.Today;
                parentTransaction.Memo = "";
                parentTransaction.Cleared = false;
                NewTits.Add(Orm.ParentTransactionViewModelService.GetViewModel(parentTransaction));
            }).AddTo(CompositeDisposable);

            NewParentIncomeCommand.Subscribe(_ =>
            {
                IParentIncome parentIncome = Orm.BffRepository.ParentIncomeRepository.Create();
                parentIncome.Date = DateTime.Today;
                parentIncome.Memo = "";
                parentIncome.Cleared = false;
                NewTits.Add(Orm.ParentIncomeViewModelService.GetViewModel(parentIncome));
            }).AddTo(CompositeDisposable);

            ApplyCommand = NewTits.ToReadOnlyReactivePropertyAsSynchronized(collection => collection.Count)
                .Select(count => count > 0)
                .ToReactiveCommand();

            ApplyCommand.Subscribe(_ => ApplyTits()).AddTo(CompositeDisposable);
        }

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

        #region ViewModel_Part

        /// <summary>
        /// Lazy loaded collection of TITs belonging to this Account.
        /// </summary>
        public override VirtualizingObservableCollection<ITitLikeViewModel> Tits => _tits ?? 
            (_tits = new VirtualizingObservableCollection<ITitLikeViewModel>(new PaginationManager<ITitLikeViewModel>(new PagedTitBaseProviderAsync(Orm.BffRepository.TitRepository, Orm, null, Orm))));

        /// <summary>
        /// Collection of TITs, which are about to be inserted to this Account.
        /// </summary>
        public sealed override ObservableCollection<ITitLikeViewModel> NewTits { get; } = new ObservableCollection<ITitLikeViewModel>();
        
        /// <summary>
        /// Refreshes the TITs of this Account.
        /// </summary>
        public override void RefreshTits()
        {
            if(IsOpen.Value)
            {
                OnPreVirtualizedRefresh();
                _tits = new VirtualizingObservableCollection<ITitLikeViewModel>(new PaginationManager<ITitLikeViewModel>(new PagedTitBaseProviderAsync(Orm.BffRepository.TitRepository, Orm, null, Orm)));
                OnPropertyChanged(nameof(Tits));
                OnPostVirtualizedRefresh();
            }
        }

        /// <summary>
        /// The sum of all accounts balances.
        /// </summary>
        public override long? Balance => Orm?.GetSummaryAccountBalance();

        /// <summary>
        /// Creates a new Transaction.
        /// </summary>
        public sealed override ReactiveCommand NewTransactionCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// Creates a new Income.
        /// </summary>
        public sealed override ReactiveCommand NewIncomeCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// Creates a new Transfer.
        /// </summary>
        public sealed override ReactiveCommand NewTransferCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// Creates a new ParentTransaction.
        /// </summary>
        public sealed override ReactiveCommand NewParentTransactionCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// Creates a new ParentIncome.
        /// </summary>
        public sealed override ReactiveCommand NewParentIncomeCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// Flushes all valid and not yet inserted TITs to the database.
        /// </summary>
        public sealed override ReactiveCommand ApplyCommand { get; }

        #endregion

        /// <summary>
        /// Refreshes the starting balance.
        /// This is needed for the summary account, because on run-time the user may add a new account.
        /// </summary>
        public void RefreshStartingBalance()
        {
            _startingBalanceSubject.OnNext(CommonPropertyProvider?.Accounts.Sum(account => account.StartingBalance) 
                                           ?? 0L);
        }

        #region Overrides of DataModelViewModel

        /// <summary>
        /// Does only return False, because the summary account may not be inserted to the database. Needed to mimic an Account.
        /// </summary>
        /// <returns>Only False.</returns>
        public override bool ValidToInsert() => false;

        #endregion
    }
}
