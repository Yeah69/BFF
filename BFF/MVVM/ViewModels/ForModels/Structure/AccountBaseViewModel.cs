using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using BFF.DataVirtualizingCollection;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.DataVirtualizingCollections;
using BFF.DB;
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

        bool IsDateFormatLong { get; }

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
        private readonly IParentTransactionViewModelService _parentTransactionViewModelService;
        private readonly Func<ITransaction, ITransactionViewModel> _transactionViewModelFactory;
        private readonly Func<ITransfer, ITransferViewModel> _transferViewModelFactory;
        protected IDataVirtualizingCollection<ITransLikeViewModel> _tits;

        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public abstract IReactiveProperty<long> StartingBalance { get; }

        /// <summary>
        /// Lazy loaded collection of TITs belonging to this Account.
        /// </summary>
        public abstract IDataVirtualizingCollection<ITransLikeViewModel> Tits { get; }

        /// <summary>
        /// Collection of TITs, which are about to be inserted to this Account.
        /// </summary>
        public abstract ObservableCollection<ITransLikeViewModel> NewTransList { get; }

        /// <summary>
        /// The current Balance of this Account.
        /// </summary>
        public abstract long? Balance { get; }

        /// <summary>
        /// The Balance of this Account considering future out- and inflows.
        /// </summary>
        public abstract long? BalanceUntilNow { get; }

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

        /// <summary>
        /// Indicates if the date format should be display in short or long fashion.
        /// </summary>
        public bool IsDateFormatLong => Settings.Default.Culture_DefaultDateLong;

        /// <summary>
        /// Initializes a AccountBaseViewModel.
        /// </summary>
        /// <param name="account">The model.</param>
        /// <param name="accountViewModelService">Fetches accounts.</param>
        /// <param name="parentTransactionViewModelService">Fetches parent transactions</param>
        /// <param name="transactionViewModelFactory">Creates transactions.</param>
        /// <param name="transferViewModelFactory">Creates transfers.</param>
        protected AccountBaseViewModel(
            IAccount account,
            Lazy<IAccountViewModelService> accountViewModelService,
            IParentTransactionViewModelService parentTransactionViewModelService,
            Func<ITransaction, ITransactionViewModel> transactionViewModelFactory,
            Func<ITransfer, ITransferViewModel> transferViewModelFactory) : base(account)
        {
            _accountViewModelService = accountViewModelService;
            _parentTransactionViewModelService = parentTransactionViewModelService;
            _transactionViewModelFactory = transactionViewModelFactory;
            _transferViewModelFactory = transferViewModelFactory;
            Messenger.Default.Register<CultureMessage>(this, message =>
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
                        OnPropertyChanged(nameof(IsDateFormatLong));
                        break;
                    default:
                        throw new InvalidEnumArgumentException();

                }
            });

            IsOpen = new ReactiveProperty<bool>(false).AddTo(CompositeDisposable);

            IsOpen.Where(isOpen => isOpen).Subscribe(_ =>
            {
                RefreshTits();
                RefreshBalance();
            }).AddTo(CompositeDisposable);
        }

        /// <summary>
        /// Representing String.
        /// </summary>
        /// <returns>Just the Name-property.</returns>
        public override string ToString()
        {
            return Name.Value;
        }

        /// <summary>
        /// Refreshes the Balance.
        /// </summary>
        public abstract void RefreshBalance();

        /// <summary>
        /// Refreshes the TITs of this Account.
        /// </summary>
        public abstract void RefreshTits();

        /// <summary>
        /// Common logic for the Apply-Command.
        /// </summary>
        protected void ApplyTits()
        {
            List<ITransLikeViewModel> insertTits = NewTransList.ToList();//.Where(tit => tit.ValidToInsert()).ToList();
            foreach (ITransLikeViewModel tit in insertTits)
            {
                tit.Insert();
                NewTransList.Remove(tit);
            }

            RefreshBalance();
            RefreshTits();
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
                        vmItems.Add(_transferViewModelFactory(transfer));
                        break;
                    case IParentTransaction parentTransaction:
                        vmItems.Add(_parentTransactionViewModelService.GetViewModel(parentTransaction));
                        break;
                    case ITransaction transaction:
                        vmItems.Add(_transactionViewModelFactory(transaction));
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
                .BuildAHoardingPreloadingSyncCollection(
                    BasicAccess, 
                    PageSize);

        protected IScheduler SubscriptionScheduler = ThreadPoolScheduler.Instance;

        protected IScheduler ObserveScheduler = new DispatcherScheduler(Application.Current.Dispatcher);

        protected int PageSize = 100;

        protected abstract IBasicAsyncDataAccess<ITransLikeViewModel> BasicAccess { get; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tits?.Dispose();
            }
            Messenger.Default.Unregister<CultureMessage>(this);
            base.Dispose(disposing);
        }
    }
}