using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using BFF.Core.Extensions;
using BFF.Core.Helper;
using BFF.DataVirtualizingCollection;
using BFF.DataVirtualizingCollection.DataVirtualizingCollections;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.ViewModel.Extensions;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Managers;
using BFF.ViewModel.Services;
using MoreLinq;
using MuVaViMo;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.ViewModel.ViewModels.ForModels.Structure
{
    public interface IAccountBaseViewModel : ICommonPropertyViewModel
    {
        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        IReactiveProperty<long> StartingBalance { get; }

        /// <summary>
        /// Lazy loaded collection of Trans' belonging to this Account.
        /// </summary>
        IDataVirtualizingCollection<ITransLikeViewModel> Trans { get; }

        bool TransIsEmpty { get; }

        /// <summary>
        /// Collection of Trans', which are about to be inserted to this Account.
        /// </summary>
        ObservableCollection<ITransLikeViewModel> NewTransList { get; }

        long? ClearedBalance { get; }

        long? ClearedBalanceUntilNow { get; }

        long? UnclearedBalance { get; }

        long? UnclearedBalanceUntilNow { get; }

        long? TotalBalance { get; }

        long? TotalBalanceUntilNow { get; }

        long? IntermediateBalance { get; }

        long? MissingSum { get; }

        long? TargetBalance { get; set; }

        bool IsOpen { get; set; }

        /// <summary>
        /// Creates a new Transaction.
        /// </summary>
        IRxRelayCommand NewTransactionCommand { get; }

        /// <summary>
        /// Creates a new Transfer.
        /// </summary>
        IRxRelayCommand NewTransferCommand { get; }

        /// <summary>
        /// Creates a new ParentTransaction.
        /// </summary>
        IRxRelayCommand NewParentTransactionCommand { get; }

        /// <summary>
        /// Flushes all valid and not yet inserted Trans' to the database.
        /// </summary>
        IRxRelayCommand ApplyCommand { get; }

        IRxRelayCommand ImportCsvBankStatement { get; }

        IRxRelayCommand StartTargetingBalance { get; }

        IRxRelayCommand AbortTargetingBalance { get; }

        bool ShowLongDate { get; }

        ITransDataGridColumnManager TransDataGridColumnManager { get; }

        bool ShowEditHeaders { get; }
        /// <summary>
        /// Refreshes the Balance.
        /// </summary>
        void RefreshBalance();

        /// <summary>
        /// Refreshes the Trans' of this Account.
        /// </summary>
        void RefreshTransCollection();

        void ReplaceNewTrans(ITransLikeViewModel replaced, ITransLikeViewModel replacement);
    }

    public abstract class AccountBaseViewModel : CommonPropertyViewModel, IVirtualizedRefresh, IAccountBaseViewModel
    {
        private readonly Lazy<IAccountViewModelService> _accountViewModelService;
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly Func<ITransLikeViewModelPlaceholder> _placeholderFactory;
        protected readonly IBffSettings BffSettings;
        private readonly SerialDisposable _removeRequestSubscriptions = new SerialDisposable();
        private CompositeDisposable _currentRemoveRequestSubscriptions = new CompositeDisposable();
        private readonly Subject<Unit> _refreshBalance = new Subject<Unit>();
        private readonly Subject<Unit> _refreshBalanceUntilNow = new Subject<Unit>();

        private IDataVirtualizingCollection<ITransLikeViewModel> _trans;

        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public abstract IReactiveProperty<long> StartingBalance { get; }

        /// <summary>
        /// Lazy loaded collection of Trans' belonging to this Account.
        /// </summary>
        public IDataVirtualizingCollection<ITransLikeViewModel> Trans => _trans ?? (_trans = CreateDataVirtualizingCollection());

        public bool TransIsEmpty => ((ICollection)Trans).Count == 0;

        /// <summary>
        /// Collection of Trans', which are about to be inserted to this Account.
        /// </summary>
        public ObservableCollection<ITransLikeViewModel> NewTransList { get; } = new ObservableCollection<ITransLikeViewModel>();

        public long? ClearedBalance { get; private set; } = 0;

        public long? ClearedBalanceUntilNow { get; private set; } = 0;
        public long? UnclearedBalance { get; private set; } = 0;
        public long? UnclearedBalanceUntilNow { get; private set; } = 0;
        public long? TotalBalance => ClearedBalance + UnclearedBalance;
        public long? TotalBalanceUntilNow => ClearedBalanceUntilNow + UnclearedBalanceUntilNow;
        public long? IntermediateBalance { get; private set; }
        public long? MissingSum { get; private set; }

        public long? TargetBalance
        {
            get => _targetBalance;
            set
            {
                if (_targetBalance == value) return;
                _targetBalance = value;
                OnPropertyChanged();
            }
        }

        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                if (value == _isOpen) return;
                _isOpen = value;
                OnPropertyChanged();
                _rxSchedulerProvider.Task.MinimalSchedule(() =>
                {
                    RefreshTransCollection();
                    RefreshBalance();
                });
                if (_isOpen)
                {
                    BffSettings.OpenAccountTab = this is ISummaryAccountViewModel 
                        ? null 
                        : Name;
                }
            }
        }

        /// <summary>
        /// All available Accounts.
        /// </summary>
        public IObservableReadOnlyList<IAccountViewModel> AllAccounts => _accountViewModelService.Value.All;

        /// <summary>
        /// Creates a new Transaction.
        /// </summary>
        public abstract IRxRelayCommand NewTransactionCommand { get; }

        /// <summary>
        /// Creates a new Transfer.
        /// </summary>
        public abstract IRxRelayCommand NewTransferCommand { get; }

        /// <summary>
        /// Creates a new ParentTransaction.
        /// </summary>
        public abstract IRxRelayCommand NewParentTransactionCommand { get; }

        /// <summary>
        /// Flushes all valid and not yet inserted Trans' to the database.
        /// </summary>
        public abstract IRxRelayCommand ApplyCommand { get; }

        public abstract IRxRelayCommand ImportCsvBankStatement { get; }
        public IRxRelayCommand StartTargetingBalance { get; }
        public IRxRelayCommand AbortTargetingBalance { get; }

        /// <summary>
        /// Indicates if the date format should be display in short or long fashion.
        /// </summary>
        public bool ShowLongDate => BffSettings.Culture_DefaultDateLong;

        public ITransDataGridColumnManager TransDataGridColumnManager { get; }

        public bool ShowEditHeaders
        {
            get => _showEditHeaders;
            private set
            {
                if (_showEditHeaders == value) return;
                _showEditHeaders = value;
                OnPropertyChanged();
            }
        }

        protected AccountBaseViewModel(
            IAccount account,
            Lazy<IAccountViewModelService> accountViewModelService,
            IAccountRepository accountRepository,
            IRxSchedulerProvider rxSchedulerProvider,
            Func<ITransLikeViewModelPlaceholder> placeholderFactory,
            IBffSettings bffSettings,
            IBackendCultureManager cultureManager,
            ITransDataGridColumnManager transDataGridColumnManager) : base(account, rxSchedulerProvider)
        {
            _accountViewModelService = accountViewModelService;
            _rxSchedulerProvider = rxSchedulerProvider;
            _placeholderFactory = placeholderFactory;
            BffSettings = bffSettings;
            TransDataGridColumnManager = transDataGridColumnManager;

            _refreshBalance.AddTo(CompositeDisposable);
            _refreshBalanceUntilNow.AddTo(CompositeDisposable);

            cultureManager.RefreshSignal.Subscribe(message =>
            {
                switch (message)
                {
                    case CultureMessage.Refresh:
                        OnPropertyChanged(nameof(StartingBalance));
                        OnPropertyChanged(nameof(ClearedBalance));
                        OnPropertyChanged(nameof(UnclearedBalance));
                        OnPropertyChanged(nameof(TotalBalance));
                        OnPropertyChanged(nameof(ClearedBalanceUntilNow));
                        OnPropertyChanged(nameof(UnclearedBalanceUntilNow));
                        OnPropertyChanged(nameof(TotalBalanceUntilNow));
                        RefreshTransCollection();
                        break;
                    case CultureMessage.RefreshCurrency:
                        OnPropertyChanged(nameof(StartingBalance));
                        OnPropertyChanged(nameof(ClearedBalance));
                        OnPropertyChanged(nameof(UnclearedBalance));
                        OnPropertyChanged(nameof(TotalBalance));
                        OnPropertyChanged(nameof(ClearedBalanceUntilNow));
                        OnPropertyChanged(nameof(UnclearedBalanceUntilNow));
                        OnPropertyChanged(nameof(TotalBalanceUntilNow));
                        RefreshTransCollection();
                        break;
                    case CultureMessage.RefreshDate:
                        OnPropertyChanged(nameof(ShowLongDate));
                        RefreshTransCollection();
                        break;
                    default:
                        throw new InvalidEnumArgumentException();

                }
            }).AddTo(CompositeDisposable);

            IsOpen = false;

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
                .ObserveOn(rxSchedulerProvider.Task)
                .Where(_ => IsOpen)
                .SelectMany(_ => Task.WhenAll(
                    account.GetClearedBalanceAsync(), 
                    account.GetUnclearedBalanceAsync()))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(b =>
                {
                    ClearedBalance = b[0];
                    UnclearedBalance = b[1];
                    OnPropertyChanged(nameof(ClearedBalance));
                    OnPropertyChanged(nameof(UnclearedBalance));
                    OnPropertyChanged(nameof(TotalBalance));
                    if (TargetBalance == TotalBalance && NewTransList.Count == 0)
                    {
                        MissingSum = null;
                        TargetBalance = null;
                        OnPropertyChanged(nameof(MissingSum));
                    }
                }).AddTo(CompositeDisposable);

            _refreshBalanceUntilNow
                .ObserveOn(rxSchedulerProvider.Task)
                .Where(_ => IsOpen)
                .SelectMany(_ => Task.WhenAll(
                    account.GetClearedBalanceUntilNowAsync(),
                    account.GetUnclearedBalanceUntilNowAsync()))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(bun =>
                {
                    ClearedBalanceUntilNow = bun[0];
                    UnclearedBalanceUntilNow = bun[1];
                    OnPropertyChanged(nameof(ClearedBalanceUntilNow));
                    OnPropertyChanged(nameof(UnclearedBalanceUntilNow));
                    OnPropertyChanged(nameof(TotalBalanceUntilNow));
                }).AddTo(CompositeDisposable);
            
            EmitOnSumRelatedChanges(NewTransList)
                .Merge(this.ObservePropertyChanges(nameof(TargetBalance)))
                .Merge(this.ObservePropertyChanges(nameof(TotalBalance)))
                .Select(_ => CalculateNewPartOfIntermediateBalance())
                .Subscribe(DoTargetBalanceSystem)
                .AddTo(CompositeDisposable);

            var serialDisposable = new SerialDisposable().AddHere(CompositeDisposable);

            NewTransList
                .ObserveCollectionChanges()
                .Select(_ => Unit.Default)
                .Subscribe(_ => serialDisposable.Disposable =
                    NewTransList
                        .OfType<ParentTransactionViewModel>()
                        .Select(ptvm => ptvm.TotalSum.ObservePropertyChanges(nameof(IReactiveProperty<long>.Value)))
                        .Merge()
                        .Select(__ => CalculateNewPartOfIntermediateBalance())
                        .Subscribe(DoTargetBalanceSystem))
                .AddTo(CompositeDisposable);

            NewTransList
                .ObservePropertyChanges(nameof(NewTransList.Count))
                .Merge(TransDataGridColumnManager.ObservePropertyChanges(nameof(TransDataGridColumnManager.NeverShowEditHeaders)))
                .Subscribe(_ => ShowEditHeaders = NewTransList.Count > 0 && !TransDataGridColumnManager.NeverShowEditHeaders)
                .AddTo(CompositeDisposable);

            StartTargetingBalance = new RxRelayCommand(() =>
            {
                TargetBalance = TotalBalance;
            });

            AbortTargetingBalance = new RxRelayCommand(() =>
            {
                TargetBalance = null;
            });

            Disposable.Create(() =>
            {
                _trans?.Dispose();
            }).AddTo(CompositeDisposable);

            IObservable<Unit> EmitOnSumRelatedChanges(ObservableCollection<ITransLikeViewModel> collection)
            {
                return collection
                    .ObserveCollectionChanges().Select(_ => Unit.Default)
                    .Merge(collection
                    .ObserveElementObservableProperty(st => st.Sum)
                    .Select(_ => Unit.Default));
            }

            void DoTargetBalanceSystem(long? ib)
            {
                IntermediateBalance = ib + TotalBalance;
                if (TargetBalance != null)
                {
                    MissingSum = TargetBalance - IntermediateBalance;
                }
                else
                {
                    MissingSum = null;
                }
                OnPropertyChanged(nameof(IntermediateBalance));
                OnPropertyChanged(nameof(MissingSum));
            }

        }

        /// <summary>
        /// Refreshes the Balance.
        /// </summary>
        public void RefreshBalance()
        {
            _refreshBalance.OnNext(Unit.Default);
            _refreshBalanceUntilNow.OnNext(Unit.Default);
        }

        public void RefreshTransCollection()
        {
            if (IsOpen)
            {
                Task.Run(() => CreateDataVirtualizingCollection())
                    .ContinueWith(async t =>
                    {
                        var temp = _trans;
                        _trans = await t;
                        _rxSchedulerProvider.UI.MinimalSchedule(() =>
                        {
                            OnPreVirtualizedRefresh();
                            OnPropertyChanged(nameof(Trans));
                            OnPropertyChanged(nameof(TransIsEmpty));
                            OnPostVirtualizedRefresh();
                            Task.Run(() => temp?.Dispose());
                        });

                    });
            }
        }

        public void ReplaceNewTrans(ITransLikeViewModel replaced, ITransLikeViewModel replacement)
        {
            int index = NewTransList.IndexOf(replaced);
            NewTransList.Insert(index, replacement);
            NewTransList.Remove(replaced);
        }

        /// <summary>
        /// Common logic for the Apply-Command.
        /// </summary>
        protected async Task ApplyTrans()
        {
            if (NewTransList.Any(t => !t.IsInsertable()))
            {
                NewTransList.ForEach(t => t.NotifyErrorsIfAny());
            }
            else
            {
                _currentRemoveRequestSubscriptions = new CompositeDisposable();
                _removeRequestSubscriptions.Disposable = _currentRemoveRequestSubscriptions;
                List<ITransLikeViewModel> insertTrans = NewTransList.ToList();
                foreach (ITransLikeViewModel trans in insertTrans)
                {
                    await trans.InsertAsync();
                    NewTransList.Remove(trans);
                }
                
                RefreshBalance();
                RefreshTransCollection();
            }
        }

        /// <summary>
        /// Invoked right before the Trans' are refreshed.
        /// </summary>
        public virtual event EventHandler PreVirtualizedRefresh;

        /// <summary>
        /// Invoked right before the Trans' are refreshed.
        /// </summary>
        public void OnPreVirtualizedRefresh()
        {
            PreVirtualizedRefresh?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Invoked right after the Trans' are refreshed.
        /// </summary>
        public virtual event EventHandler PostVirtualizedRefresh;

        /// <summary>
        /// Invoked right after the Trans' are refreshed.
        /// </summary>
        public void OnPostVirtualizedRefresh()
        {
            PostVirtualizedRefresh?.Invoke(this, new EventArgs());
        }


        private IDataVirtualizingCollection<ITransLikeViewModel> CreateDataVirtualizingCollection()
            => DataVirtualizingCollectionBuilder<ITransLikeViewModel>
                .Build(pageSize: PageSize)
                .Hoarding()
                .NonPreloading()
                .TaskBasedFetchers(PageFetcher, CountFetcher)
                .AsyncIndexAccess(_placeholderFactory, _rxSchedulerProvider.Task, _rxSchedulerProvider.UI);

        private readonly int PageSize = 100;
        private bool _isOpen;
        private long? _targetBalance;
        private bool _showEditHeaders;

        protected abstract Func<int, int, Task<ITransLikeViewModel[]>> PageFetcher { get; }

        protected abstract Func<Task<int>> CountFetcher { get; }

        protected abstract long? CalculateNewPartOfIntermediateBalance();
    }
}