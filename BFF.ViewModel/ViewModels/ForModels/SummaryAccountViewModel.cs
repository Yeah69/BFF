using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Core.IoC;
using BFF.Model.Models;
using BFF.ViewModel.Extensions;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Managers;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.ViewModel.ViewModels.ForModels
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
    internal class SummaryAccountViewModel : AccountBaseViewModel, ISummaryAccountViewModel, IOncePerBackend
    {
        private readonly ISummaryAccount _summaryAccount;
        private readonly Lazy<IAccountViewModelService> _service;
        private readonly IConvertFromTransBaseToTransLikeViewModel _convertFromTransBaseToTransLikeViewModel;

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
            Lazy<IAccountViewModelService> service,
            IBffSettings bffSettings,
            Func<ITransLikeViewModelPlaceholder> placeholderFactory,
            IRxSchedulerProvider rxSchedulerProvider,
            IConvertFromTransBaseToTransLikeViewModel convertFromTransBaseToTransLikeViewModel,
            IBackendCultureManager cultureManager,
            ITransDataGridColumnManager transDataGridColumnManager,
            Func<IAccountBaseViewModel, ITransactionViewModel> transactionViewModelFactory,
            Func<IAccountBaseViewModel, ITransferViewModel> transferViewModelFactory,
            Func<IAccountBaseViewModel, IParentTransactionViewModel> parentTransactionViewModelFactory) 
            : base(
                summaryAccount,
                service,
                rxSchedulerProvider,
                placeholderFactory,
                bffSettings,
                cultureManager,
                transDataGridColumnManager)
        {
            _summaryAccount = summaryAccount;
            _service = service;
            _convertFromTransBaseToTransLikeViewModel = convertFromTransBaseToTransLikeViewModel;

            if(string.IsNullOrWhiteSpace(bffSettings.OpenAccountTab))
                IsOpen = true;



            StartingBalance = new ReactiveProperty<long>().AddTo(CompositeDisposable);
            
            NewTransactionCommand = new RxRelayCommand(() =>
            {
                var transactionViewModel = transactionViewModelFactory(this);
                if (MissingSum != null) transactionViewModel.Sum.Value = (long)MissingSum;
                NewTransList.Add(transactionViewModel);
            }).AddTo(CompositeDisposable);

            NewTransferCommand = new RxRelayCommand(() => NewTransList.Add(transferViewModelFactory(this))).AddTo(CompositeDisposable);

            NewParentTransactionCommand = new RxRelayCommand(() =>
            {
                var parentTransactionViewModel = parentTransactionViewModelFactory(this);
                NewTransList.Add(parentTransactionViewModel);
                if (MissingSum != null)
                {
                    parentTransactionViewModel.NewSubTransactionCommand?.Execute(null);
                    parentTransactionViewModel.NewSubTransactions.First().Sum.Value = (long)MissingSum;
                }
            }).AddTo(CompositeDisposable);

            ApplyCommand = new AsyncRxRelayCommand(async () => await ApplyTrans(),
                NewTransList
                    .ToReadOnlyReactivePropertyAsSynchronized(collection => collection.Count)
                    .Select(count => count > 0),
                NewTransList.Count > 0);

            ImportCsvBankStatement = new RxRelayCommand(() => {}).AddTo(CompositeDisposable);

            RefreshStartingBalance();
        }

        #region ViewModel_Part

        protected override Func<int, int, Task<ITransLikeViewModel[]>> PageFetcher =>
            async (offset, pageSize) => _convertFromTransBaseToTransLikeViewModel
                .Convert(await _summaryAccount.GetTransPageAsync(offset, pageSize), this)
                .ToArray();

        protected override Func<Task<int>> CountFetcher =>
            async () => (int) await _summaryAccount.GetTransCountAsync();

        protected override long? CalculateNewPartOfIntermediateBalance()
        {
            long? sum = 0;
            foreach (var transLikeViewModel in NewTransList)
            {
                switch (transLikeViewModel)
                {
                    case TransferViewModel _:
                        break;
                    case ParentTransactionViewModel parent:
                        sum += parent.TotalSum.Value;
                        break;
                    default:
                        sum += transLikeViewModel.Sum.Value;
                        break;
                }
            }

            return sum;
        }

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
            _service
                .Value
                .AllCollectionInitialized.ContinueWith(
                    _ => StartingBalance.Value = _service.Value.All.Sum(account => account.StartingBalance.Value));
        }
    }
}
