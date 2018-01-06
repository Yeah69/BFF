using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.DataVirtualizingCollections;
using BFF.DB.Dapper.ModelRepositories;
using BFF.Helper;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface IAccountViewModel : IAccountBaseViewModel
    {
        IReactiveProperty<DateTime> StartingDate { get; }
    }

    /// <summary>
    /// Tits can be added to an Account
    /// </summary>
    public class AccountViewModel : AccountBaseViewModel, IAccountViewModel
    {
        private readonly IAccount _account;
        private readonly ITransRepository _transRepository;
        private readonly IAccountRepository _accountRepository;

        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public sealed override IReactiveProperty<long> StartingBalance { get; }


        public IReactiveProperty<DateTime> StartingDate { get; }

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
        /// <param name="summaryAccountViewModel">This account summarizes all accounts.</param>
        public AccountViewModel(
            IAccount account, 
            ISummaryAccountViewModel summaryAccountViewModel,
            ITransRepository transRepository,
            IAccountRepository accountRepository,
            Lazy<IAccountViewModelService> accountViewModelService,
            IParentTransactionViewModelService parentTransactionViewModelService,
            Func<IAccount, ITransactionViewModel> transactionViewModelFactory,
            Func<ITransferViewModel> transferViewModelFactory,
            Func<IAccount, IParentTransactionViewModel> parentTransactionViewModelFactory,
            Func<ITransaction, ITransactionViewModel> dependingTransactionViewModelFactory,
            Func<ITransfer, ITransferViewModel> dependingTransferViewModelFactory) 
            : base(
                account,
                accountViewModelService, 
                parentTransactionViewModelService, 
                dependingTransactionViewModelFactory, 
                dependingTransferViewModelFactory)
        {
            _account = account;
            _transRepository = transRepository;
            _accountRepository = accountRepository;
            StartingBalance = account
                .ToReactivePropertyAsSynchronized(a => a.StartingBalance, ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);
            StartingBalance.Subscribe(_ => summaryAccountViewModel.RefreshStartingBalance())
                           .AddTo(CompositeDisposable);

            StartingDate = account
                .ToReactivePropertyAsSynchronized(a => a.StartingDate, ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            //summaryAccountViewModel.RefreshStartingBalance();

            NewTransactionCommand.Subscribe(_ => NewTits.Add(transactionViewModelFactory(_account))).AddTo(CompositeDisposable);

            NewTransferCommand.Subscribe(_ => NewTits.Add(transferViewModelFactory())).AddTo(CompositeDisposable);

            NewParentTransactionCommand.Subscribe(_ => NewTits.Add(parentTransactionViewModelFactory(_account))).AddTo(CompositeDisposable);

            ApplyCommand = NewTits.ToReadOnlyReactivePropertyAsSynchronized(collection => collection.Count)
                .Select(count => count > 0)
                .ToReactiveCommand();

            ApplyCommand.Subscribe(_ => ApplyTits()).AddTo(CompositeDisposable);

        }

        #region ViewModel_Part

        protected override IBasicAsyncDataAccess<ITransLikeViewModel> BasicAccess
            => new RelayBasicAsyncDataAccess<ITransLikeViewModel>(
                (offset, pageSize) => CreatePacket(_transRepository.GetPage(offset, pageSize, _account)),
                () => _transRepository.GetCount(_account),
                () => new TransLikeViewModelPlaceholder());

        /// <summary>
        /// Lazy loaded collection of TITs belonging to this Account.
        /// </summary>
        public override IDataVirtualizingCollection<ITransLikeViewModel> Tits => _tits ?? CreateDataVirtualizingCollection();
        
        /// <summary>
        /// Collection of TITs, which are about to be inserted to this Account.
        /// </summary>
        public sealed override ObservableCollection<ITransLikeViewModel> NewTits { get; } = new ObservableCollection<ITransLikeViewModel>();

        /// <summary>
        /// The current Balance of this Account.
        /// </summary>
        public override long? Balance => _accountRepository.GetBalance(_account);

        /// <summary>
        /// The current Balance of this Account.
        /// </summary>
        public override long? BalanceUntilNow => _accountRepository.GetBalanceUntilNow(_account);

        /// <summary>
        /// Refreshes the Balance.
        /// </summary>
        public override void RefreshBalance()
        {
            if (IsOpen.Value)
            {
                OnPropertyChanged(nameof(Balance));
                OnPropertyChanged(nameof(BalanceUntilNow));
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
                var temp = _tits;
                _tits = CreateDataVirtualizingCollection();
                OnPropertyChanged(nameof(Tits));
                OnPostVirtualizedRefresh();
                Task.Run(() => temp?.Dispose());
            }
        }

        /// <summary>
        /// Creates a new Transaction.
        /// </summary>
        public sealed override ReactiveCommand NewTransactionCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// Creates a new Transfer.
        /// </summary>
        public sealed override ReactiveCommand NewTransferCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// Creates a new ParentTransaction.
        /// </summary>
        public sealed override ReactiveCommand NewParentTransactionCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// Flushes all valid and not yet inserted TITs to the database.
        /// </summary>
        public sealed override ReactiveCommand ApplyCommand { get; }

        #endregion
    }
}
