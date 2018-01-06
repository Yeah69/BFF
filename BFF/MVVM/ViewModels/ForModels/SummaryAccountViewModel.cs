using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.DataVirtualizingCollections;
using BFF.DB;
using BFF.DB.Dapper.ModelRepositories;
using BFF.Helper;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
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
    public class SummaryAccountViewModel : AccountBaseViewModel, ISummaryAccountViewModel, IOncePerBackend
    {
        private readonly ISummaryAccount _summaryAccount;
        private readonly IAccountRepository _accountRepository;
        private readonly Lazy<IAccountViewModelService> _service;
        private readonly ITransRepository _transRepository;

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
        public SummaryAccountViewModel(
            ISummaryAccount summaryAccount, 
            IAccountRepository accountRepository, 
            Lazy<IAccountViewModelService> service,
            ITransRepository transRepository,
            IParentTransactionViewModelService parentTransactionViewModelService,
            Func<ITransactionViewModel> transactionViewModelFactory,
            Func<ITransferViewModel> transferViewModelFactory,
            Func<IParentTransactionViewModel> parentTransactionFactory,
            Func<ITransaction, ITransactionViewModel> dependingTransactionViewModelFactory,
            Func<ITransfer, ITransferViewModel> dependingTransferViewModelFactory) 
            : base(
                summaryAccount,
                service, 
                parentTransactionViewModelService,
                dependingTransactionViewModelFactory,
                dependingTransferViewModelFactory)
        {
            _summaryAccount = summaryAccount;
            _accountRepository = accountRepository;
            _service = service;
            _transRepository = transRepository;
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

            StartingBalance = new ReactiveProperty<long>().AddTo(CompositeDisposable);
            //RefreshStartingBalance();

            NewTransactionCommand.Subscribe(_ => NewTits.Add(transactionViewModelFactory())).AddTo(CompositeDisposable);

            NewTransferCommand.Subscribe(_ => NewTits.Add(transferViewModelFactory())).AddTo(CompositeDisposable);

            NewParentTransactionCommand.Subscribe(_ => NewTits.Add(parentTransactionFactory())).AddTo(CompositeDisposable);

            ApplyCommand = NewTits.ToReadOnlyReactivePropertyAsSynchronized(collection => collection.Count)
                .Select(count => count > 0)
                .ToReactiveCommand().AddTo(CompositeDisposable);

            ApplyCommand.Subscribe(_ => ApplyTits()).AddTo(CompositeDisposable);

            Disposable.Create(() => { Messenger.Default.Unregister<SummaryAccountMessage>(this); }).AddTo(CompositeDisposable);
        }

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

        #region ViewModel_Part

        protected override IBasicAsyncDataAccess<ITransLikeViewModel> BasicAccess
            => new RelayBasicAsyncDataAccess<ITransLikeViewModel>(
                (offset, pageSize) => CreatePacket(_transRepository.GetPage(offset, pageSize, null)),
                () => _transRepository.GetCount(null),
                () => new TransLikeViewModelPlaceholder());

        /// <summary>
        /// Lazy loaded collection of TITs belonging to this Account.
        /// </summary>
        public override IDataVirtualizingCollection<ITransLikeViewModel> Tits => _tits ?? (_tits = CreateDataVirtualizingCollection());
        
        /// <summary>
        /// Collection of TITs, which are about to be inserted to this Account.
        /// </summary>
        public sealed override ObservableCollection<ITransLikeViewModel> NewTits { get; } = new ObservableCollection<ITransLikeViewModel>();
        
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
        /// The sum of all accounts balances.
        /// </summary>
        public override long? Balance => _accountRepository.GetBalance(_summaryAccount);

        public override long? BalanceUntilNow => _accountRepository.GetBalanceUntilNow(_summaryAccount);

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

        /// <summary>
        /// Refreshes the starting balance.
        /// This is needed for the summary account, because on run-time the user may add a new account.
        /// </summary>
        public void RefreshStartingBalance()
        {
            StartingBalance.Value = _service.Value.All.Sum(account => account.StartingBalance.Value);
        }
    }
}
