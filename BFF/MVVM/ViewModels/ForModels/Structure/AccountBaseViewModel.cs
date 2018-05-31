using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.DataVirtualizingCollections;
using BFF.DB;
using BFF.DB.Dapper.ModelRepositories;
using BFF.Helper;
using BFF.Helper.Extensions;
using BFF.MVVM.Managers;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
using BFF.MVVM.Services;
using BFF.Properties;
using MuVaViMo;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface IAccountBaseViewModel : ICommonPropertyViewModel
    {
        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        IReactiveProperty<long> StartingBalance { get; }

        /// <summary>
        /// Lazy loaded collection of TITs belonging to this Account.
        /// </summary>
        IDataVirtualizingCollection<ITransLikeViewModel> Tits { get; }

        /// <summary>
        /// Collection of TITs, which are about to be inserted to this Account.
        /// </summary>
        ObservableCollection<ITransLikeViewModel> NewTransList { get; }

        /// <summary>
        /// The current Balance of this Account.
        /// </summary>
        long? Balance { get; }

        
        long? BalanceUntilNow { get; }

        IReactiveProperty<bool> IsOpen { get; }

        /// <summary>
        /// Creates a new Transaction.
        /// </summary>
        ReactiveCommand NewTransactionCommand { get; }

        /// <summary>
        /// Creates a new Transfer.
        /// </summary>
        ReactiveCommand NewTransferCommand { get; }

        /// <summary>
        /// Creates a new ParentTransaction.
        /// </summary>
        ReactiveCommand NewParentTransactionCommand { get; }

        /// <summary>
        /// Flushes all valid and not yet inserted TITs to the database.
        /// </summary>
        ReactiveCommand ApplyCommand { get; }

        ReactiveCommand ImportCsvBankStatement { get; }

        bool ShowLongDate { get; }

        IAccountModuleColumnManager AccountModuleColumnManager { get; }
        /// <summary>
        /// Refreshes the Balance.
        /// </summary>
        void RefreshBalance();

        /// <summary>
        /// Refreshes the TITs of this Account.
        /// </summary>
        void RefreshTits();
    }

    public abstract class AccountBaseViewModel : CommonPropertyViewModel, IVirtualizedRefresh, IAccountBaseViewModel
    {
        private readonly Lazy<IAccountViewModelService> _accountViewModelService;
        private readonly IRxSchedulerProvider _schedulerProvider;
        private readonly Func<ITransaction, IAccountBaseViewModel, ITransactionViewModel> _transactionViewModelFactory;
        private readonly Func<IParentTransaction, IAccountBaseViewModel, IParentTransactionViewModel> _parentTransactionViewModelFactory;
        private readonly Func<ITransfer, IAccountBaseViewModel, ITransferViewModel> _transferViewModelFactory;
        private readonly SerialDisposable _removeRequestSubscriptions = new SerialDisposable();
        private CompositeDisposable _currentRemoveRequestSubscriptions = new CompositeDisposable();
        private long? _balance = 0;
        private long? _balanceUntilNow = 0;

        private readonly Subject<Unit> _refreshBalance = new Subject<Unit>();
        private readonly Subject<Unit> _refreshBalanceUntilNow = new Subject<Unit>();

        private IDataVirtualizingCollection<ITransLikeViewModel> _tits;

        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public abstract IReactiveProperty<long> StartingBalance { get; }

        /// <summary>
        /// Lazy loaded collection of TITs belonging to this Account.
        /// </summary>
        public IDataVirtualizingCollection<ITransLikeViewModel> Tits => _tits ?? (_tits = CreateDataVirtualizingCollection());

        /// <summary>
        /// Collection of TITs, which are about to be inserted to this Account.
        /// </summary>
        public ObservableCollection<ITransLikeViewModel> NewTransList { get; } = new ObservableCollection<ITransLikeViewModel>();
        
        public long? Balance => _balance;
        
        public long? BalanceUntilNow => _balanceUntilNow;

        public IReactiveProperty<bool> IsOpen { get; }

        /// <summary>
        /// All available Accounts.
        /// </summary>
        public IObservableReadOnlyList<IAccountViewModel> AllAccounts => _accountViewModelService.Value.All;

        /// <summary>
        /// Creates a new Transaction.
        /// </summary>
        public abstract ReactiveCommand NewTransactionCommand { get; }

        /// <summary>
        /// Creates a new Transfer.
        /// </summary>
        public abstract ReactiveCommand NewTransferCommand { get; }

        /// <summary>
        /// Creates a new ParentTransaction.
        /// </summary>
        public abstract ReactiveCommand NewParentTransactionCommand { get; }

        /// <summary>
        /// Flushes all valid and not yet inserted TITs to the database.
        /// </summary>
        public abstract ReactiveCommand ApplyCommand { get; }

        public abstract ReactiveCommand ImportCsvBankStatement { get; }

        /// <summary>
        /// Indicates if the date format should be display in short or long fashion.
        /// </summary>
        public bool ShowLongDate => Settings.Default.Culture_DefaultDateLong;

        public IAccountModuleColumnManager AccountModuleColumnManager { get; }

        protected AccountBaseViewModel(
            IAccount account,
            Lazy<IAccountViewModelService> accountViewModelService,
            IAccountRepository accountRepository,
            IRxSchedulerProvider schedulerProvider,
            IBackendCultureManager cultureManager,
            Func<ITransaction, IAccountBaseViewModel, ITransactionViewModel> transactionViewModelFactory,
            Func<IParentTransaction, IAccountBaseViewModel, IParentTransactionViewModel> parentTransactionViewModelFactory,
            Func<ITransfer, IAccountBaseViewModel, ITransferViewModel> transferViewModelFactory,
            IAccountModuleColumnManager accountModuleColumnManager) : base(account, schedulerProvider)
        {
            _accountViewModelService = accountViewModelService;
            _schedulerProvider = schedulerProvider;
            _transactionViewModelFactory = transactionViewModelFactory;
            _parentTransactionViewModelFactory = parentTransactionViewModelFactory;
            _transferViewModelFactory = transferViewModelFactory;
            AccountModuleColumnManager = accountModuleColumnManager;

            _refreshBalance.AddTo(CompositeDisposable);
            _refreshBalanceUntilNow.AddTo(CompositeDisposable);

            cultureManager.RefreshSignal.Subscribe(message =>
            {
                switch (message)
                {
                    case CultureMessage.Refresh:
                        OnPropertyChanged(nameof(StartingBalance));
                        OnPropertyChanged(nameof(Balance));
                        RefreshTits();
                        break;
                    case CultureMessage.RefreshCurrency:
                        OnPropertyChanged(nameof(StartingBalance));
                        OnPropertyChanged(nameof(Balance));
                        RefreshTits();
                        break;
                    case CultureMessage.RefreshDate:
                        OnPropertyChanged(nameof(ShowLongDate));
                        RefreshTits();
                        break;
                    default:
                        throw new InvalidEnumArgumentException();

                }
            }).AddTo(CompositeDisposable);

            IsOpen = new ReactiveProperty<bool>(false).AddTo(CompositeDisposable);

            IsOpen.Where(isOpen => isOpen).Subscribe(_ =>
            {
                RefreshTits();
                RefreshBalance();
            }).AddTo(CompositeDisposable);

            _removeRequestSubscriptions.AddTo(CompositeDisposable);
            _removeRequestSubscriptions.Disposable = _currentRemoveRequestSubscriptions;
            NewTransList.ObserveAddChanged().Subscribe(t =>
            {
                t.RemoveRequests
                    .Take(1)
                    .Subscribe(_ => NewTransList.Remove(t))
                    .AddTo(_currentRemoveRequestSubscriptions);
            }).AddTo(CompositeDisposable);

            _refreshBalance
                .ObserveOn(schedulerProvider.Task)
                .Where(_ => IsOpen.Value)
                .SelectMany(_ => accountRepository.GetBalanceAsync(account))
                .ObserveOn(schedulerProvider.UI)
                .Subscribe(b =>
                {
                    _balance = b;
                    OnPropertyChanged(nameof(Balance));
                }).AddTo(CompositeDisposable);

            _refreshBalanceUntilNow
                .ObserveOn(schedulerProvider.Task)
                .Where(_ => IsOpen.Value)
                .SelectMany(_ => accountRepository.GetBalanceUntilNowAsync(account))
                .ObserveOn(schedulerProvider.UI)
                .Subscribe(bun =>
                {
                    _balanceUntilNow = bun;
                    OnPropertyChanged(nameof(BalanceUntilNow));
                }).AddTo(CompositeDisposable);

            Disposable.Create(() =>
            {
                _tits?.Dispose();
            }).AddTo(CompositeDisposable);
        }

        /// <summary>
        /// Refreshes the Balance.
        /// </summary>
        public void RefreshBalance()
        {
            _refreshBalance.OnNext(Unit.Default);
            _refreshBalanceUntilNow.OnNext(Unit.Default);
        }

        public void RefreshTits()
        {
            if (IsOpen.Value)
            {
                Task.Run(() => CreateDataVirtualizingCollection())
                    .ContinueWith(async t =>
                    {
                        var temp = _tits;
                        _tits = await t;
                        _schedulerProvider.UI.MinimalSchedule(() =>
                        {
                            OnPreVirtualizedRefresh();
                            OnPropertyChanged(nameof(Tits));
                            OnPostVirtualizedRefresh();
                            Task.Run(() => temp?.Dispose());
                        });

                    });
            }
        }

        /// <summary>
        /// Common logic for the Apply-Command.
        /// </summary>
        protected async Task ApplyTits()
        {
            if (NewTransList.All(t => t.IsInsertable()))
            {
                _currentRemoveRequestSubscriptions = new CompositeDisposable();
                _removeRequestSubscriptions.Disposable = _currentRemoveRequestSubscriptions;
                List<ITransLikeViewModel> insertTits = NewTransList.ToList();
                foreach (ITransLikeViewModel tit in insertTits)
                {
                    await tit.InsertAsync();
                    NewTransList.Remove(tit);
                }
                
                RefreshBalance();
                RefreshTits();
            }
        }

        /// <summary>
        /// Invoked right before the TITs are refreshed.
        /// </summary>
        public virtual event EventHandler PreVirtualizedRefresh;

        /// <summary>
        /// Invoked right before the TITs are refreshed.
        /// </summary>
        public void OnPreVirtualizedRefresh()
        {
            PreVirtualizedRefresh?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Invoked right after the TITs are refreshed.
        /// </summary>
        public virtual event EventHandler PostVirtualizedRefresh;

        /// <summary>
        /// Invoked right after the TITs are refreshed.
        /// </summary>
        public void OnPostVirtualizedRefresh()
        {
            PostVirtualizedRefresh?.Invoke(this, new EventArgs());
        }

        protected ITransLikeViewModel[] CreatePacket(IEnumerable<ITransBase> items)
        {
            IList<ITransLikeViewModel> vmItems = new List<ITransLikeViewModel>();
            foreach (ITransBase item in items)
            {
                switch (item)
                {
                    case ITransfer transfer:
                        vmItems.Add(_transferViewModelFactory(transfer, this));
                        break;
                    case IParentTransaction parentTransaction:
                        vmItems.Add(_parentTransactionViewModelFactory(parentTransaction, this));
                        break;
                    case ITransaction transaction:
                        vmItems.Add(_transactionViewModelFactory(transaction, this));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            return vmItems.ToArray();
        }


        protected IDataVirtualizingCollection<ITransLikeViewModel> CreateDataVirtualizingCollection()
            => CollectionBuilder<ITransLikeViewModel>
                .CreateBuilder()
                .BuildAHoardingTaskBasedAsyncCollection(
                    BasicAccess,
                    _schedulerProvider.Task,
                    _schedulerProvider.UI,
                    PageSize);

        protected int PageSize = 100;

        protected abstract IBasicTaskBasedAsyncDataAccess<ITransLikeViewModel> BasicAccess { get; }
    }
}