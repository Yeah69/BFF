using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using BFF.DataVirtualizingObservableCollection;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
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
        DataVirtualizingCollection<ITitLikeViewModel> Tits { get; }

        /// <summary>
        /// Collection of TITs, which are about to be inserted to this Account.
        /// </summary>
        ObservableCollection<ITitLikeViewModel> NewTits { get; }

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
        /// Creates a new Income.
        /// </summary>
        ReactiveCommand NewIncomeCommand { get; }

        /// <summary>
        /// Creates a new Transfer.
        /// </summary>
        ReactiveCommand NewTransferCommand { get; }

        /// <summary>
        /// Creates a new ParentTransaction.
        /// </summary>
        ReactiveCommand NewParentTransactionCommand { get; }

        /// <summary>
        /// Creates a new ParentIncome.
        /// </summary>
        ReactiveCommand NewParentIncomeCommand { get; }

        /// <summary>
        /// Flushes all valid and not yet inserted TITs to the database.
        /// </summary>
        ReactiveCommand ApplyCommand { get; }

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
        protected DataVirtualizingCollection<ITitLikeViewModel> _tits;

        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public abstract IReactiveProperty<long> StartingBalance { get; }

        /// <summary>
        /// Lazy loaded collection of TITs belonging to this Account.
        /// </summary>
        public abstract DataVirtualizingCollection<ITitLikeViewModel> Tits { get; }

        /// <summary>
        /// Collection of TITs, which are about to be inserted to this Account.
        /// </summary>
        public abstract ObservableCollection<ITitLikeViewModel> NewTits { get; }

        /// <summary>
        /// The current Balance of this Account.
        /// </summary>
        public abstract long? Balance { get; }

        public IReactiveProperty<bool> IsOpen { get; }

        /// <summary>
        /// All available Accounts.
        /// </summary>
        public IObservableReadOnlyList<IAccountViewModel> AllAccounts => CommonPropertyProvider.AllAccountViewModels;

        /// <summary>
        /// Creates a new Transaction.
        /// </summary>
        public abstract ReactiveCommand NewTransactionCommand { get; }

        /// <summary>
        /// Creates a new Income.
        /// </summary>
        public abstract ReactiveCommand NewIncomeCommand { get; }

        /// <summary>
        /// Creates a new Transfer.
        /// </summary>
        public abstract ReactiveCommand NewTransferCommand { get; }

        /// <summary>
        /// Creates a new ParentTransaction.
        /// </summary>
        public abstract ReactiveCommand NewParentTransactionCommand { get; }

        /// <summary>
        /// Creates a new ParentIncome.
        /// </summary>
        public abstract ReactiveCommand NewParentIncomeCommand { get; }

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
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="account">The model.</param>
        protected AccountBaseViewModel(IBffOrm orm, IAccount account) : base(orm, account)
        {
            Orm = orm;
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

            IsOpen = new ReactiveProperty<bool>(false);
            IsOpen.AddTo(CompositeDisposable);

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
            List<ITitLikeViewModel> insertTits = NewTits.Where(tit => tit.ValidToInsert()).ToList();
            foreach (ITitLikeViewModel tit in insertTits)
            {
                tit.Insert();
                NewTits.Remove(tit);
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

        protected ITitLikeViewModel[] CreatePacket(IEnumerable<ITitBase> items)
        {
            IList<ITitLikeViewModel> vmItems = new List<ITitLikeViewModel>();
            foreach (ITitBase item in items)
            {
                switch (item)
                {
                    case ITransfer transfer:
                        vmItems.Add(new TransferViewModel(transfer, Orm, Orm.CommonPropertyProvider.AccountViewModelService));
                        break;
                    case IParentTransaction parentTransaction:
                        vmItems.Add(Orm.ParentTransactionViewModelService.GetViewModel(parentTransaction));
                        break;
                    case IParentIncome parentIncome:
                        vmItems.Add(Orm.ParentIncomeViewModelService.GetViewModel(parentIncome));
                        break;
                    case ITransaction transaction:
                        vmItems.Add(new TransactionViewModel(
                            transaction,
                            hcvm => new NewCategoryViewModel(hcvm, Orm.BffRepository.CategoryRepository, Orm.CommonPropertyProvider.CategoryViewModelService),
                            hpvm => new NewPayeeViewModel(hpvm, Orm.BffRepository.PayeeRepository, Orm.CommonPropertyProvider.PayeeViewModelService),
                            Orm,
                            Orm.CommonPropertyProvider.AccountViewModelService,
                            Orm.CommonPropertyProvider.PayeeViewModelService,
                            Orm.CommonPropertyProvider.CategoryViewModelService));
                        break;
                    case IIncome income:
                        vmItems.Add(new IncomeViewModel(
                            income,
                            hcvm => new NewCategoryViewModel(hcvm, Orm.BffRepository.CategoryRepository, Orm.CommonPropertyProvider.CategoryViewModelService),
                            hpvm => new NewPayeeViewModel(hpvm, Orm.BffRepository.PayeeRepository, Orm.CommonPropertyProvider.PayeeViewModelService),
                            Orm,
                            Orm.CommonPropertyProvider.AccountViewModelService,
                            Orm.CommonPropertyProvider.PayeeViewModelService,
                            Orm.CommonPropertyProvider.CategoryViewModelService));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            return vmItems.ToArray();
        }

        protected IScheduler SubscriptionScheduler = ThreadPoolScheduler.Instance;

        protected IScheduler ObserveScheduler = new DispatcherScheduler(Application.Current.Dispatcher);
    }
}