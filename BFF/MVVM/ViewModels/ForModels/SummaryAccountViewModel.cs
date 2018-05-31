using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BFF.DataVirtualizingCollection.DataAccesses;
using BFF.DataVirtualizingCollection.DataVirtualizingCollections;
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
    /// Tits can be added to an Account
    /// </summary>
    public class SummaryAccountViewModel : AccountBaseViewModel, ISummaryAccountViewModel, IOncePerBackend
    {
        private readonly Lazy<IAccountViewModelService> _service;
        private readonly ITransRepository _transRepository;
        private readonly Func<ITransLikeViewModelPlaceholder> _placeholderFactory;
        private readonly IRxSchedulerProvider _schedulerProvider;

        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public sealed override IReactiveProperty<long> StartingBalance { get; }

        /// <summary>
        /// Name of the Account Model
        /// </summary>
        public override IReactiveProperty<string> Name //todo Localization
            => new ReactiveProperty<string>("All Accounts");
        
        public SummaryAccountViewModel(
            ISummaryAccount summaryAccount, 
            IAccountRepository accountRepository, 
            Lazy<IAccountViewModelService> service,
            ITransRepository transRepository,
            Func<ITransLikeViewModelPlaceholder> placeholderFactory,
            IRxSchedulerProvider schedulerProvider,
            IBackendCultureManager cultureManager,
            IAccountModuleColumnManager accountModuleColumnManager,
            Func<IAccountBaseViewModel, ITransactionViewModel> transactionViewModelFactory,
            Func<IAccountBaseViewModel, ITransferViewModel> transferViewModelFactory,
            Func<IAccountBaseViewModel, IParentTransactionViewModel> parentTransactionFactory,
            Func<ITransaction, IAccountBaseViewModel, ITransactionViewModel> dependingTransactionViewModelFactory,
            Func<IParentTransaction, IAccountBaseViewModel, IParentTransactionViewModel> dependingParentTransactionViewModelFactory,
            Func<ITransfer, IAccountBaseViewModel, ITransferViewModel> dependingTransferViewModelFactory) 
            : base(
                summaryAccount,
                service,
                accountRepository,
                schedulerProvider,
                cultureManager,
                dependingTransactionViewModelFactory, 
                dependingParentTransactionViewModelFactory,
                dependingTransferViewModelFactory,
                accountModuleColumnManager)
        {
            _service = service;
            _transRepository = transRepository;
            _placeholderFactory = placeholderFactory;
            _schedulerProvider = schedulerProvider;
            IsOpen.Value = true;

            StartingBalance = new ReactiveProperty<long>().AddTo(CompositeDisposable);

            NewTransactionCommand.Subscribe(_ => NewTransList.Add(transactionViewModelFactory(this))).AddTo(CompositeDisposable);

            NewTransferCommand.Subscribe(_ => NewTransList.Add(transferViewModelFactory(this))).AddTo(CompositeDisposable);

            NewParentTransactionCommand.Subscribe(_ => NewTransList.Add(parentTransactionFactory(this))).AddTo(CompositeDisposable);

            ApplyCommand = NewTransList.ToReadOnlyReactivePropertyAsSynchronized(collection => collection.Count)
                .Select(count => count > 0)
                .ToReactiveCommand().AddTo(CompositeDisposable);

            ApplyCommand.Subscribe(async _ => await ApplyTits()).AddTo(CompositeDisposable);

            ImportCsvBankStatement = new ReactiveCommand().AddTo(CompositeDisposable);
        }

        #region ViewModel_Part

        protected override IBasicTaskBasedAsyncDataAccess<ITransLikeViewModel> BasicAccess
            => new RelayBasicTaskBasedAsyncDataAccess<ITransLikeViewModel>(
                async (offset, pageSize) => CreatePacket(await _transRepository.GetPageAsync(offset, pageSize, null)),
                async () => (int) await _transRepository.GetCountAsync(null),
                () => _placeholderFactory());

        /// <summary>
        /// Lazy loaded collection of TITs belonging to this Account.
        /// </summary>
        public override IDataVirtualizingCollection<ITransLikeViewModel> Tits => _tits ?? (_tits = CreateDataVirtualizingCollection());
        
        /// <summary>
        /// Refreshes the TITs of this Account.
        /// </summary>
        public override void RefreshTits()
        {
            if(IsOpen.Value)
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

        public override ReactiveCommand ImportCsvBankStatement { get; }

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
