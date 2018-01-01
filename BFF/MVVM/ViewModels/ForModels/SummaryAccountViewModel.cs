﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.DataVirtualizingCollections;
using BFF.DB;
using BFF.Helper;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;
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
        public SummaryAccountViewModel(IBffOrm orm,
                                       ISummaryAccount summaryAccount) : base(orm, summaryAccount)
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

            StartingBalance = new ReactiveProperty<long>().AddTo(CompositeDisposable);
            RefreshStartingBalance();

            NewTransactionCommand.Subscribe(_ =>
            {
                ITransaction transaction = Orm.BffRepository.TransactionRepository.Create();
                transaction.Date = DateTime.Today;
                transaction.Memo = "";
                transaction.Sum = 0L;
                transaction.Cleared = false;
                NewTits.Add(new TransactionViewModel(
                    transaction,
                    hcvm => new NewCategoryViewModel(
                        hcvm, 
                        Orm.BffRepository.CategoryRepository, 
                        Orm.BffRepository.IncomeCategoryRepository,
                        Orm.CommonPropertyProvider.CategoryViewModelService,
                        Orm.CommonPropertyProvider.IncomeCategoryViewModelService,
                        Orm.CommonPropertyProvider.CategoryBaseViewModelService),
                    hpvm => new NewPayeeViewModel(hpvm, Orm.BffRepository.PayeeRepository, Orm.CommonPropertyProvider.PayeeViewModelService),
                    Orm,
                    Orm.CommonPropertyProvider.AccountViewModelService,
                    Orm.CommonPropertyProvider.PayeeViewModelService,
                    Orm.CommonPropertyProvider.CategoryBaseViewModelService,
                    Orm.CommonPropertyProvider.FlagViewModelService));
            }).AddTo(CompositeDisposable);

            NewTransferCommand.Subscribe(_ =>
            {
                ITransfer transfer = Orm.BffRepository.TransferRepository.Create();
                transfer.Date = DateTime.Today;
                transfer.Memo = "";
                transfer.Sum = 0L;
                transfer.Cleared = false;
                NewTits.Add(new TransferViewModel(
                    transfer, 
                    Orm,
                    Orm.CommonPropertyProvider.AccountViewModelService,
                    Orm.CommonPropertyProvider.FlagViewModelService));
            }).AddTo(CompositeDisposable);

            NewParentTransactionCommand.Subscribe(_ =>
            {
                IParentTransaction parentTransaction = Orm.BffRepository.ParentTransactionRepository.Create();
                parentTransaction.Date = DateTime.Today;
                parentTransaction.Memo = "";
                parentTransaction.Cleared = false;
                NewTits.Add(Orm.ParentTransactionViewModelService.GetViewModel(parentTransaction));
            }).AddTo(CompositeDisposable);

            ApplyCommand = NewTits.ToReadOnlyReactivePropertyAsSynchronized(collection => collection.Count)
                .Select(count => count > 0)
                .ToReactiveCommand().AddTo(CompositeDisposable);

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
                OnPropertyChanged(nameof(BalanceUntilNow));
            }
        }

        #region ViewModel_Part

        protected override IBasicAsyncDataAccess<ITransLikeViewModel> BasicAccess
            => new RelayBasicAsyncDataAccess<ITransLikeViewModel>(
                (offset, pageSize) => CreatePacket(Orm.BffRepository.TransRepository.GetPage(offset, pageSize, null)),
                () => Orm.BffRepository.TransRepository.GetCount(null),
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
        public override long? Balance => Orm?.GetSummaryAccountBalance();

        public override long? BalanceUntilNow => Orm?.GetSummaryAccountBalanceUntilNow();

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
            StartingBalance.Value = CommonPropertyProvider?.AccountViewModelService.All.Sum(account => account.StartingBalance.Value) ?? 0L;
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
