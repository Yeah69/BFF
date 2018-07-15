using System;
using System.Linq;
using System.Reactive.Linq;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DB;
using BFF.DB.Dapper.ModelRepositories;
using BFF.Helper;
using BFF.Helper.Extensions;
using BFF.MVVM.Managers;
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
    /// Trans can be added to an Account
    /// </summary>
    public class SummaryAccountViewModel : AccountBaseViewModel, ISummaryAccountViewModel, IOncePerBackend
    {
        private readonly Lazy<IAccountViewModelService> _service;
        private readonly ITransRepository _transRepository;
        private readonly Func<ITransLikeViewModelPlaceholder> _placeholderFactory;

        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public sealed override IReactiveProperty<long> StartingBalance { get; }

        /// <summary>
        /// Name of the Account Model
        /// </summary>
        public override string Name //todo Localization
            => "All Accounts";
        
        public SummaryAccountViewModel(
            ISummaryAccount summaryAccount, 
            IAccountRepository accountRepository, 
            Lazy<IAccountViewModelService> service,
            ITransRepository transRepository,
            Func<ITransLikeViewModelPlaceholder> placeholderFactory,
            IRxSchedulerProvider rxSchedulerProvider,
            IBackendCultureManager cultureManager,
            IAccountModuleColumnManager accountModuleColumnManager,
            Func<IAccountBaseViewModel, ITransactionViewModel> transactionViewModelFactory,
            Func<IAccountBaseViewModel, ITransferViewModel> transferViewModelFactory,
            Func<IAccountBaseViewModel, IParentTransactionViewModel> parentTransactionViewModelFactory,
            Func<ITransaction, IAccountBaseViewModel, ITransactionViewModel> dependingTransactionViewModelFactory,
            Func<IParentTransaction, IAccountBaseViewModel, IParentTransactionViewModel> dependingParentTransactionViewModelFactory,
            Func<ITransfer, IAccountBaseViewModel, ITransferViewModel> dependingTransferViewModelFactory) 
            : base(
                summaryAccount,
                service,
                accountRepository,
                rxSchedulerProvider,
                cultureManager,
                dependingTransactionViewModelFactory, 
                dependingParentTransactionViewModelFactory,
                dependingTransferViewModelFactory,
                accountModuleColumnManager)
        {
            _service = service;
            _transRepository = transRepository;
            _placeholderFactory = placeholderFactory;
            IsOpen = true;

            StartingBalance = new ReactiveProperty<long>().AddTo(CompositeDisposable);
            
            NewTransactionCommand = new RxRelayCommand(() => NewTransList.Add(transactionViewModelFactory(this))).AddTo(CompositeDisposable);

            NewTransferCommand = new RxRelayCommand(() => NewTransList.Add(transferViewModelFactory(this))).AddTo(CompositeDisposable);

            NewParentTransactionCommand = new RxRelayCommand(() => NewTransList.Add(parentTransactionViewModelFactory(this))).AddTo(CompositeDisposable);

            ApplyCommand = new AsyncRxRelayCommand(async () => await ApplyTrans(),
                NewTransList
                    .ToReadOnlyReactivePropertyAsSynchronized(collection => collection.Count)
                    .Select(count => count > 0),
                NewTransList.Count > 0);

            ImportCsvBankStatement = new RxRelayCommand(() => {}).AddTo(CompositeDisposable);
        }

        #region ViewModel_Part

        protected override IBasicTaskBasedAsyncDataAccess<ITransLikeViewModel> BasicAccess
            => new RelayBasicTaskBasedAsyncDataAccess<ITransLikeViewModel>(
                async (offset, pageSize) => CreatePacket(await _transRepository.GetPageAsync(offset, pageSize, null)),
                async () => (int) await _transRepository.GetCountAsync(null),
                () => _placeholderFactory());
        
        /// <summary>
        /// Creates a new Transaction.
        /// </summary>
        public sealed override IRxRelayCommand NewTransactionCommand { get; }

        /// <summary>
        /// Creates a new Transfer.
        /// </summary>
        public sealed override IRxRelayCommand NewTransferCommand { get; }

        /// <summary>
        /// Creates a new ParentTransaction.
        /// </summary>
        public sealed override IRxRelayCommand NewParentTransactionCommand { get; }

        /// <summary>
        /// Flushes all valid and not yet inserted Trans' to the database.
        /// </summary>
        public sealed override IRxRelayCommand ApplyCommand { get; }

        public override IRxRelayCommand ImportCsvBankStatement { get; }

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
