using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
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
            IParentTransactionViewModelService parentTransactionViewModelService,
            IRxSchedulerProvider schedulerProvider,
            IBackendCultureManager cultureManager,
            Func<ITransactionViewModel> transactionViewModelFactory,
            Func<ITransferViewModel> transferViewModelFactory,
            Func<IParentTransactionViewModel> parentTransactionFactory,
            Func<ITransaction, ITransactionViewModel> dependingTransactionViewModelFactory,
            Func<ITransfer, ITransferViewModel> dependingTransferViewModelFactory) 
            : base(
                summaryAccount,
                service,
                accountRepository,
                parentTransactionViewModelService,
                schedulerProvider,
                cultureManager,
                dependingTransactionViewModelFactory,
                dependingTransferViewModelFactory)
        {
            _service = service;
            _transRepository = transRepository;
            _schedulerProvider = schedulerProvider;
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
            //RefreshStartingBalance();

            NewTransactionCommand.Subscribe(_ => NewTransList.Add(transactionViewModelFactory())).AddTo(CompositeDisposable);

            NewTransferCommand.Subscribe(_ => NewTransList.Add(transferViewModelFactory())).AddTo(CompositeDisposable);

            NewParentTransactionCommand.Subscribe(_ => NewTransList.Add(parentTransactionFactory())).AddTo(CompositeDisposable);

            ApplyCommand = NewTransList.ToReadOnlyReactivePropertyAsSynchronized(collection => collection.Count)
                .Select(count => count > 0)
                .ToReactiveCommand().AddTo(CompositeDisposable);

            ApplyCommand.Subscribe(_ => ApplyTits()).AddTo(CompositeDisposable);

            ImportCsvBankStatement = new ReactiveCommand().AddTo(CompositeDisposable);

            Disposable.Create(() => { Messenger.Default.Unregister<SummaryAccountMessage>(this); }).AddTo(CompositeDisposable);
        }

        #region ViewModel_Part

        protected override IBasicTaskBasedAsyncDataAccess<ITransLikeViewModel> BasicAccess
            => new RelayBasicTaskBasedAsyncDataAccess<ITransLikeViewModel>(
                async (offset, pageSize) => CreatePacket(await _transRepository.GetPageAsync(offset, pageSize, null)),
                async () => (int) await _transRepository.GetCountAsync(null),
                () => new TransLikeViewModelPlaceholder());

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
                        _schedulerProvider.UI.Schedule(Unit.Default, (sc, st) =>
                        {
                            OnPreVirtualizedRefresh();
                            OnPropertyChanged(nameof(Tits));
                            OnPostVirtualizedRefresh();
                            Task.Run(() => temp?.Dispose());
                            return Disposable.Empty;
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
