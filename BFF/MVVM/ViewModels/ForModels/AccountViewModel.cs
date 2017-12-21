﻿using System;
using System.Collections.ObjectModel;
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

        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public sealed override IReactiveProperty<long> StartingBalance { get; }


        public IReactiveProperty<DateTime> StartingDate { get; }

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
            _account = account;
            StartingBalance = account
                .ToReactivePropertyAsSynchronized(a => a.StartingBalance)
                .AddTo(CompositeDisposable);
            StartingBalance.Subscribe(_ => summaryAccountViewModel.RefreshStartingBalance())
                           .AddTo(CompositeDisposable);

            StartingDate = account
                .ToReactivePropertyAsSynchronized(a => a.StartingDate)
                .AddTo(CompositeDisposable);

            summaryAccountViewModel.RefreshStartingBalance();

            NewTransactionCommand.Subscribe(_ =>
            {
                ITransaction transaction = Orm.BffRepository.TransactionRepository.Create();
                transaction.Date = DateTime.Today;
                transaction.Account = _account;
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
                        Orm.CommonPropertyProvider.IncomeCategoryViewModelService),
                    hpvm => new NewPayeeViewModel(hpvm, Orm.BffRepository.PayeeRepository, Orm.CommonPropertyProvider.PayeeViewModelService),
                    Orm,
                    Orm.CommonPropertyProvider.AccountViewModelService,
                    Orm.CommonPropertyProvider.PayeeViewModelService,
                    Orm.CommonPropertyProvider.CategoryBaseViewModelService));
            }).AddTo(CompositeDisposable);

            NewIncomeCommand.Subscribe(_ =>
            {
                IIncome income = Orm.BffRepository.IncomeRepository.Create();
                income.Date = DateTime.Today;
                income.Account = _account;
                income.Memo = "";
                income.Sum = 0L;
                income.Cleared = false;
                NewTits.Add(new IncomeViewModel(
                    income,
                    hcvm => new NewCategoryViewModel(
                        hcvm,
                        Orm.BffRepository.CategoryRepository, 
                        Orm.BffRepository.IncomeCategoryRepository, 
                        Orm.CommonPropertyProvider.CategoryViewModelService,
                        Orm.CommonPropertyProvider.IncomeCategoryViewModelService),
                    hpvm => new NewPayeeViewModel(hpvm, Orm.BffRepository.PayeeRepository, Orm.CommonPropertyProvider.PayeeViewModelService),
                    Orm,
                    Orm.CommonPropertyProvider.AccountViewModelService,
                    Orm.CommonPropertyProvider.PayeeViewModelService,
                    Orm.CommonPropertyProvider.CategoryBaseViewModelService));
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
                parentTransaction.Account = _account;
                parentTransaction.Memo = "";
                parentTransaction.Cleared = false;
                NewTits.Add(Orm.ParentTransactionViewModelService.GetViewModel(parentTransaction));
            }).AddTo(CompositeDisposable);

            NewParentIncomeCommand.Subscribe(_ =>
            {
                IParentIncome parentIncome = Orm.BffRepository.ParentIncomeRepository.Create();
                parentIncome.Date = DateTime.Today;
                parentIncome.Account = _account;
                parentIncome.Memo = "";
                parentIncome.Cleared = false;
                NewTits.Add(Orm.ParentIncomeViewModelService.GetViewModel(parentIncome));
            }).AddTo(CompositeDisposable);

            ApplyCommand = NewTits.ToReadOnlyReactivePropertyAsSynchronized(collection => collection.Count)
                .Select(count => count > 0)
                .ToReactiveCommand();

            ApplyCommand.Subscribe(_ => ApplyTits()).AddTo(CompositeDisposable);

        }

        #region ViewModel_Part

        protected override IBasicAsyncDataAccess<ITitLikeViewModel> BasicAccess
            => new RelayBasicAsyncDataAccess<ITitLikeViewModel>(
                (offset, pageSize) => CreatePacket(Orm.GetPage<ITitBase>(offset, pageSize, _account)),
                () => Orm.GetCount<ITitBase>(_account),
                () => new TitLikeViewModelPlaceholder());

        /// <summary>
        /// Lazy loaded collection of TITs belonging to this Account.
        /// </summary>
        public override IDataVirtualizingCollection<ITitLikeViewModel> Tits => _tits ?? CreateDataVirtualizingCollection();
        
        /// <summary>
        /// Collection of TITs, which are about to be inserted to this Account.
        /// </summary>
        public sealed override ObservableCollection<ITitLikeViewModel> NewTits { get; } = new ObservableCollection<ITitLikeViewModel>();

        /// <summary>
        /// The current Balance of this Account.
        /// </summary>
        public override long? Balance => Orm?.GetAccountBalance(_account);

        /// <summary>
        /// The current Balance of this Account.
        /// </summary>
        public override long? BalanceUntilNow => Orm?.GetAccountBalanceUntilNow(_account);

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
    }
}
