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
using BFF.DataVirtualizingCollection.DataVirtualizingCollection;
using BFF.Model.Models;
using BFF.ViewModel.Extensions;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Managers;
using BFF.ViewModel.Services;
using MoreLinq;
using MrMeeseeks.Reactive.Extensions;
using MrMeeseeks.Windows;
using MuVaViMo;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Threading;
using System.Windows.Input;

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
        ICommand NewTransactionCommand { get; }

        /// <summary>
        /// Creates a new Transfer.
        /// </summary>
        ICommand NewTransferCommand { get; }

        /// <summary>
        /// Creates a new ParentTransaction.
        /// </summary>
        ICommand NewParentTransactionCommand { get; }

        /// <summary>
        /// Flushes all valid and not yet inserted Trans' to the database.
        /// </summary>
        ICommand ApplyCommand { get; }

        ICommand ImportCsvBankStatement { get; }

        ICommand StartTargetingBalance { get; }

        ICommand AbortTargetingBalance { get; }

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

    internal abstract class AccountBaseViewModel : CommonPropertyViewModel, IAccountBaseViewModel, IAsyncDisposable
    {
        private readonly IAccount _account;
        private readonly Lazy<IAccountViewModelService> _accountViewModelService;
        private readonly IRxSchedulerProvider _rxSchedulerProvider;
        private readonly Func<ITransLikeViewModelPlaceholder> _placeholderFactory;
        private readonly IConvertFromTransBaseToTransLikeViewModel _convertFromTransBaseToTransLikeViewModel;
        protected readonly IBffSettings BffSettings;
        private readonly SerialDisposable _removeRequestSubscriptions = new();
        private CompositeDisposable _currentRemoveRequestSubscriptions = new();
        private readonly Subject<Unit> _refreshBalance = new();
        private readonly Subject<Unit> _refreshBalanceUntilNow = new();


        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public abstract IReactiveProperty<long> StartingBalance { get; }

        /// <summary>
        /// Lazy loaded collection of Trans' belonging to this Account.
        /// </summary>
        public IDataVirtualizingCollection<ITransLikeViewModel> Trans { get; }

        public bool TransIsEmpty => ((ICollection)Trans).Count == 0;

        /// <summary>
        /// Collection of Trans', which are about to be inserted to this Account.
        /// </summary>
        public ObservableCollection<ITransLikeViewModel> NewTransList { get; } = new();

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
                if (_isOpen && this is ISummaryAccountViewModel summaryAccountViewModel)
                {
                    BffSettings.OpenAccountTab =  null;
                    summaryAccountViewModel.RefreshStartingBalance();
                }
                else
                {

                    BffSettings.OpenAccountTab = Name;
                }
            }
        }

        /// <summary>
        /// All available Accounts.
        /// </summary>
        public IObservableReadOnlyList<IAccountViewModel>? AllAccounts => _accountViewModelService.Value.All;

        /// <summary>
        /// Creates a new Transaction.
        /// </summary>
        public abstract ICommand NewTransactionCommand { get; }

        /// <summary>
        /// Creates a new Transfer.
        /// </summary>
        public abstract ICommand NewTransferCommand { get; }

        /// <summary>
        /// Creates a new ParentTransaction.
        /// </summary>
        public abstract ICommand NewParentTransactionCommand { get; }

        /// <summary>
        /// Flushes all valid and not yet inserted Trans' to the database.
        /// </summary>
        public abstract ICommand ApplyCommand { get; }

        public abstract ICommand ImportCsvBankStatement { get; }
        public ICommand StartTargetingBalance { get; }
        public ICommand AbortTargetingBalance { get; }

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
            IRxSchedulerProvider rxSchedulerProvider,
            Func<ITransLikeViewModelPlaceholder> placeholderFactory,
            IConvertFromTransBaseToTransLikeViewModel convertFromTransBaseToTransLikeViewModel,
            IBffSettings bffSettings,
            IBackendCultureManager cultureManager,
            ITransDataGridColumnManager transDataGridColumnManager) : base(account, rxSchedulerProvider)
        {
            _account = account;
            _accountViewModelService = accountViewModelService;
            _rxSchedulerProvider = rxSchedulerProvider;
            _placeholderFactory = placeholderFactory;
            _convertFromTransBaseToTransLikeViewModel = convertFromTransBaseToTransLikeViewModel;
            BffSettings = bffSettings;
            TransDataGridColumnManager = transDataGridColumnManager;

            _refreshBalance.CompositeDisposalWith(CompositeDisposable);
            _refreshBalanceUntilNow.CompositeDisposalWith(CompositeDisposable);

            Trans = CreateDataVirtualizingCollection();
            Trans.ObservePropertyChanged(nameof(ICollection.Count))
                .ObserveOn(_rxSchedulerProvider.UI)
                .Subscribe(_ => OnPropertyChanged(nameof(TransIsEmpty)));

            cultureManager.RefreshSignal.Subscribe(message =>
            {
                switch (message)
                {
                    case CultureMessage.Refresh:
                        OnPropertyChanged(
                            nameof(StartingBalance),
                            nameof(ClearedBalance),
                            nameof(UnclearedBalance),
                            nameof(TotalBalance),
                            nameof(ClearedBalanceUntilNow),
                            nameof(UnclearedBalanceUntilNow),
                            nameof(TotalBalanceUntilNow));
                        RefreshTransCollection();
                        break;
                    case CultureMessage.RefreshCurrency:
                        OnPropertyChanged(
                            nameof(StartingBalance),
                            nameof(ClearedBalance),
                            nameof(UnclearedBalance),
                            nameof(TotalBalance),
                            nameof(ClearedBalanceUntilNow),
                            nameof(UnclearedBalanceUntilNow),
                            nameof(TotalBalanceUntilNow));
                        RefreshTransCollection();
                        break;
                    case CultureMessage.RefreshDate:
                        OnPropertyChanged(nameof(ShowLongDate));
                        RefreshTransCollection();
                        break;
                    default:
                        throw new InvalidEnumArgumentException();

                }
            }).CompositeDisposalWith(CompositeDisposable);

            IsOpen = false;

            _removeRequestSubscriptions.CompositeDisposalWith(CompositeDisposable);
            _removeRequestSubscriptions.Disposable = _currentRemoveRequestSubscriptions;
            NewTransList.ObserveAddChanged().Subscribe(t =>
            {
                t.RemoveRequests
                    .Take(1)
                    .Subscribe(_ => NewTransList.Remove(t))
                    .AddTo(_currentRemoveRequestSubscriptions);
            }).CompositeDisposalWith(CompositeDisposable);

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
                    OnPropertyChanged(
                        nameof(ClearedBalance),
                        nameof(UnclearedBalance),
                        nameof(TotalBalance));
                    if (TargetBalance == TotalBalance && NewTransList.Count == 0)
                    {
                        MissingSum = null;
                        TargetBalance = null;
                        OnPropertyChanged(nameof(MissingSum));
                    }
                }).CompositeDisposalWith(CompositeDisposable);

            _refreshBalanceUntilNow
                .ObserveOn(rxSchedulerProvider.Task)
                .SelectMany(_ => Task.WhenAll(
                    account.GetClearedBalanceUntilNowAsync(),
                    account.GetUnclearedBalanceUntilNowAsync()))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(bun =>
                {
                    ClearedBalanceUntilNow = bun[0];
                    UnclearedBalanceUntilNow = bun[1];
                    OnPropertyChanged(
                        nameof(ClearedBalanceUntilNow),
                        nameof(UnclearedBalanceUntilNow),
                        nameof(TotalBalanceUntilNow));
                }).CompositeDisposalWith(CompositeDisposable);
            
            EmitOnSumRelatedChanges(NewTransList)
                .Merge(this.ObservePropertyChanged(nameof(TargetBalance), nameof(TotalBalance)).SelectUnit())
                .Select(_ => CalculateNewPartOfIntermediateBalance())
                .Subscribe(DoTargetBalanceSystem)
                .CompositeDisposalWith(CompositeDisposable);

            var serialDisposable = new SerialDisposable().CompositeDisposalWith(CompositeDisposable);

            NewTransList
                .ObserveCollectionChanges()
                .Select(_ => Unit.Default)
                .Subscribe(_ => serialDisposable.Disposable =
                    NewTransList
                        .OfType<ParentTransactionViewModel>()
                        .Select(ptvm => ptvm.TotalSum.ObservePropertyChanged(nameof(IReactiveProperty<long>.Value)))
                        .Merge()
                        .Select(_ => CalculateNewPartOfIntermediateBalance())
                        .Subscribe(DoTargetBalanceSystem))
                .CompositeDisposalWith(CompositeDisposable);

            NewTransList
                .ObservePropertyChanged(nameof(NewTransList.Count))
                .Merge(TransDataGridColumnManager.ObservePropertyChanged(nameof(TransDataGridColumnManager.NeverShowEditHeaders)))
                .Subscribe(_ => ShowEditHeaders = NewTransList.Count > 0 && !TransDataGridColumnManager.NeverShowEditHeaders)
                .CompositeDisposalWith(CompositeDisposable);
            
            StartTargetingBalance = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    CompositeDisposable,
                    () => TargetBalance = TotalBalance);
            
            AbortTargetingBalance = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    CompositeDisposable,
                    () => TargetBalance = null);

            static IObservable<Unit> EmitOnSumRelatedChanges(ObservableCollection<ITransLikeViewModel> collection) =>
                collection
                    .ObserveCollectionChanges().Select(_ => Unit.Default)
                    .Merge(collection
                        .ObserveElementObservableProperty(st => st.Sum)
                        .Select(_ => Unit.Default));

            void DoTargetBalanceSystem(long? ib)
            {
                IntermediateBalance = ib + TotalBalance;
                if (TargetBalance is not null)
                {
                    MissingSum = TargetBalance - IntermediateBalance;
                }
                else
                {
                    MissingSum = null;
                }
                OnPropertyChanged(
                    nameof(IntermediateBalance),
                    nameof(MissingSum));
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
            if (IsOpen) Task.Run(() => Trans.Reset());
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


        private IDataVirtualizingCollection<ITransLikeViewModel> CreateDataVirtualizingCollection()
            => DataVirtualizingCollectionBuilder
                .Build<ITransLikeViewModel>(
                    PageSize, 
                    _rxSchedulerProvider.UI, 
                    _rxSchedulerProvider.Task)
                .NonPreloading()
                .LeastRecentlyUsed(10, 5)
                .TaskBasedFetchers(PageFetcher, CountFetcher)
                .AsyncIndexAccess((_, _) => _placeholderFactory());

        private readonly int PageSize = 100;
        private bool _isOpen;
        private long? _targetBalance;
        private bool _showEditHeaders;

        private async Task<ITransLikeViewModel[]> PageFetcher (int offset, int pageSize, CancellationToken _)
        {
            var transLikeViewModels = _convertFromTransBaseToTransLikeViewModel
                .Convert(await _account.GetTransPageAsync(offset, pageSize), this)
                .ToArray();
            return transLikeViewModels;
        }

        private async Task<int> CountFetcher(CancellationToken _) =>
            (int) await _account.GetTransCountAsync();

        protected abstract long? CalculateNewPartOfIntermediateBalance();
        public ValueTask DisposeAsync()
        {
            this.Dispose();
            return this.Trans.DisposeAsync();
        }
    }
}