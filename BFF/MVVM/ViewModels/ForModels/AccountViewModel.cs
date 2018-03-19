using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Threading;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.DataVirtualizingCollections;
using BFF.DB.Dapper.ModelRepositories;
using BFF.Helper.Extensions;
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

        ISumEditViewModel StartingBalanceEdit { get; }
    }

    /// <summary>
    /// Tits can be added to an Account
    /// </summary>
    public class AccountViewModel : AccountBaseViewModel, IAccountViewModel
    {
        private readonly IAccount _account;
        private readonly ITransRepository _transRepository;
        private readonly IMainBffDialogCoordinator _mainBffDialogCoordinator;

        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public sealed override IReactiveProperty<long> StartingBalance { get; }


        public IReactiveProperty<DateTime> StartingDate { get; }
        public ISumEditViewModel StartingBalanceEdit { get; }

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
        
        public AccountViewModel(
            IAccount account, 
            ISummaryAccountViewModel summaryAccountViewModel,
            ITransRepository transRepository,
            IAccountRepository accountRepository,
            Lazy<IAccountViewModelService> accountViewModelService,
            IParentTransactionViewModelService parentTransactionViewModelService,
            IMainBffDialogCoordinator mainBffDialogCoordinator,
            Func<IReactiveProperty<long>, ISumEditViewModel> createSumEdit,
            Func<IAccount, ITransactionViewModel> transactionViewModelFactory,
            Func<ITransferViewModel> transferViewModelFactory,
            Func<IAccount, IParentTransactionViewModel> parentTransactionViewModelFactory,
            Func<ITransaction, ITransactionViewModel> dependingTransactionViewModelFactory,
            Func<ITransfer, ITransferViewModel> dependingTransferViewModelFactory) 
            : base(
                account,
                accountViewModelService,
                accountRepository,
                parentTransactionViewModelService, 
                dependingTransactionViewModelFactory, 
                dependingTransferViewModelFactory)
        {
            _account = account;
            _transRepository = transRepository;
            _mainBffDialogCoordinator = mainBffDialogCoordinator;

            StartingBalance = account
                .ToReactivePropertyAsSynchronized(a => a.StartingBalance, ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);
            StartingBalanceEdit = createSumEdit(StartingBalance);
            StartingBalance.Subscribe(_ => summaryAccountViewModel.RefreshStartingBalance())
                           .AddTo(CompositeDisposable);

            StartingDate = account
                .ToReactivePropertyAsSynchronized(a => a.StartingDate, ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            NewTransactionCommand.Subscribe(_ => NewTransList.Add(transactionViewModelFactory(_account))).AddTo(CompositeDisposable);

            NewTransferCommand.Subscribe(_ => NewTransList.Add(transferViewModelFactory())).AddTo(CompositeDisposable);

            NewParentTransactionCommand.Subscribe(_ => NewTransList.Add(parentTransactionViewModelFactory(_account))).AddTo(CompositeDisposable);

            ApplyCommand = NewTransList.ToReadOnlyReactivePropertyAsSynchronized(collection => collection.Count)
                .Select(count => count > 0)
                .ToReactiveCommand();

            ApplyCommand.Subscribe(_ => ApplyTits()).AddTo(CompositeDisposable);

        }

        protected override IBasicAsyncDataAccess<ITransLikeViewModel> BasicAccess
            => new RelayBasicAsyncDataAccess<ITransLikeViewModel>(
                (offset, pageSize) => CreatePacket(_transRepository.GetPageAsync(offset, pageSize, _account).Result),
                () => (int) _transRepository.GetCountAsync(_account).Result,
                () => new TransLikeViewModelPlaceholder());

        /// <summary>
        /// Lazy loaded collection of TITs belonging to this Account.
        /// </summary>
        public override IDataVirtualizingCollection<ITransLikeViewModel> Tits => _tits ?? CreateDataVirtualizingCollection();

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

        public override void Delete()
        {
            _mainBffDialogCoordinator
                .ShowMessageAsync(
                    "ConfirmationDialog_Title".Localize(),
                    "Account_Delete_ConfirmationMessage".Localize(),
                    BffMessageDialogStyle.AffirmativeAndNegative)
                .ToObservable()
                .ObserveOn(Dispatcher.CurrentDispatcher)
                .Subscribe(r =>
                {
                    if (r == BffMessageDialogResult.Affirmative)
                    {
                        base.Delete();
                        foreach (var accountViewModel in AllAccounts)
                        {
                            accountViewModel.RefreshTits();
                        }
                        Messenger.Default.Send(SummaryAccountMessage.Refresh);
                    }
                });
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
    }
}
