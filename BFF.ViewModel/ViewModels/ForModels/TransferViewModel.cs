using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.ViewModel.Extensions;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Managers;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels.ForModels.Structure;
using MrMeeseeks.Extensions;
using MrMeeseeks.Reactive.Extensions;
using MuVaViMo;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.ViewModel.ViewModels.ForModels
{
    public interface ITransferViewModel : ITransBaseViewModel
    {
        IObservableReadOnlyList<IAccountViewModel> AllAccounts { get; }

        /// <summary>
        /// The account from where the money is transfered.
        /// </summary>
        IAccountViewModel FromAccount { get; set; }

        /// <summary>
        /// The account to where the money is transfered.
        /// </summary>
        IAccountViewModel ToAccount { get; set; }

        IRxRelayCommand NonInsertedConvertToTransaction { get; }
        IRxRelayCommand NonInsertedConvertToParentTransaction { get; }
    }

    /// <summary>
    /// The ViewModel of the Model Transfer.
    /// </summary>
    internal sealed class TransferViewModel : TransBaseViewModel, ITransferViewModel
    {
        private readonly ITransfer _transfer;
        private readonly IAccountViewModelService _accountViewModelService;
        private readonly ILocalizer _localizer;
        private readonly ISummaryAccountViewModel _summaryAccountViewModel;
        private IAccountViewModel _fromAccount;
        private IAccountViewModel _toAccount;

        public IObservableReadOnlyList<IAccountViewModel> AllAccounts => _accountViewModelService.All;

        /// <summary>
        /// The account from where the money is transfered.
        /// </summary>
        public IAccountViewModel FromAccount
        {
            get => _fromAccount;
            set => _transfer.FromAccount = _accountViewModelService.GetModel(value);
        }

        /// <summary>
        /// The account to where the money is transferred.
        /// </summary>
        public IAccountViewModel ToAccount
        {
            get => _toAccount;
            set => _transfer.ToAccount = _accountViewModelService.GetModel(value);
        }

        public IRxRelayCommand NonInsertedConvertToTransaction { get; }
        public IRxRelayCommand NonInsertedConvertToParentTransaction { get; }

        /// <summary>
        /// The amount of money, which is transfered.
        /// </summary>
        public override IReactiveProperty<long> Sum { get; }

        public TransferViewModel(
            ITransfer transfer, 
            IAccountViewModelService accountViewModelService,
            INewFlagViewModel newFlagViewModel,
            Func<IReactiveProperty<long>, ISumEditViewModel> createSumEdit,
            ILocalizer localizer,
            ILastSetDate lastSetDate,
            IRxSchedulerProvider rxSchedulerProvider,
            ITransTransformingManager transTransformingManager,
            ISummaryAccountViewModel summaryAccountViewModel,
            IFlagViewModelService flagViewModelService,
            IAccountBaseViewModel owner) 
            : base(
                transfer, 
                newFlagViewModel, 
                lastSetDate, 
                rxSchedulerProvider, 
                flagViewModelService, 
                owner)
        {
            _transfer = transfer;
            _accountViewModelService = accountViewModelService;
            _localizer = localizer;
            _summaryAccountViewModel = summaryAccountViewModel;

            _fromAccount = _accountViewModelService.GetViewModel(transfer.FromAccount);
            transfer
                .ObservePropertyChanged(nameof(transfer.FromAccount))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ =>
                {
                    _fromAccount = _accountViewModelService.GetViewModel(transfer.FromAccount);
                    OnPropertyChanged(nameof(FromAccount));
                    if (_fromAccount != null)
                    {
                        ClearErrors(nameof(FromAccount));
                        OnErrorChanged(nameof(FromAccount));
                    }
                })
                .AddTo(CompositeDisposable);

            _toAccount = _accountViewModelService.GetViewModel(transfer.ToAccount);
            transfer
                .ObservePropertyChanged(nameof(transfer.ToAccount))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ =>
                {
                    _toAccount = _accountViewModelService.GetViewModel(transfer.ToAccount);
                    OnPropertyChanged(nameof(ToAccount));
                    if (_toAccount != null)
                    {
                        ClearErrors(nameof(ToAccount));
                        OnErrorChanged(nameof(ToAccount));
                    }
                })
                .AddTo(CompositeDisposable);

            if (FromAccount is null && owner is IAccountViewModel specificAccount)
                FromAccount = specificAccount;

            transfer
                .ObservePropertyChanged(nameof(transfer.FromAccount))
                .SkipLast(1)
                .Where(_ => transfer.IsInserted)
                .Subscribe(_ => RefreshAnAccountViewModel(accountViewModelService.GetViewModel(transfer.FromAccount)))
                .AddTo(CompositeDisposable);

            transfer
                .ObservePropertyChanged(nameof(transfer.FromAccount))
                .Where(_ => transfer.IsInserted)
                .Subscribe(_ => RefreshAnAccountViewModel(accountViewModelService.GetViewModel(transfer.FromAccount)))
                .AddTo(CompositeDisposable);

            transfer
                .ObservePropertyChanged(nameof(transfer.ToAccount))
                .SkipLast(1)
                .Where(_ => transfer.IsInserted)
                .Subscribe(_ => RefreshAnAccountViewModel(accountViewModelService.GetViewModel(transfer.ToAccount)))
                .AddTo(CompositeDisposable);

            transfer
                .ObservePropertyChanged(nameof(transfer.ToAccount))
                .Where(_ => transfer.IsInserted)
                .Subscribe(_ => RefreshAnAccountViewModel(accountViewModelService.GetViewModel(transfer.ToAccount)))
                .AddTo(CompositeDisposable);

            Sum = transfer.ToReactivePropertyAsSynchronized(
                nameof(transfer.Sum),
                () => transfer.Sum,
                s => transfer.Sum = s,
                rxSchedulerProvider.UI,
                ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);

            transfer
                .ObservePropertyChanged(nameof(transfer.Sum))
                .Where(_ => transfer.IsInserted)
                .Subscribe(sum =>
                {
                    FromAccount?.RefreshBalance();
                    ToAccount?.RefreshBalance();
                }).AddTo(CompositeDisposable);

            SumEdit = createSumEdit(Sum);


            NonInsertedConvertToTransaction = new RxRelayCommand(
                    () => Owner.ReplaceNewTrans(
                        this,
                        transTransformingManager.NotInsertedToTransactionViewModel(this)))
                .AddTo(CompositeDisposable);
            NonInsertedConvertToParentTransaction = new RxRelayCommand(
                    () => Owner.ReplaceNewTrans(
                        this,
                        transTransformingManager.NotInsertedToParentTransactionViewModel(this)))
                .AddTo(CompositeDisposable);
        }

        public override ISumEditViewModel SumEdit { get; }

        public override bool IsInsertable() => base.IsInsertable() && FromAccount.IsNotNull() && ToAccount.IsNotNull();

        public override async Task DeleteAsync()
        {
            await base.DeleteAsync();
            RefreshAnAccountViewModel(FromAccount);
            RefreshAnAccountViewModel(ToAccount);
            _summaryAccountViewModel.RefreshTransCollection();
            _summaryAccountViewModel.RefreshBalance();
        }

        public override void NotifyErrorsIfAny()
        {
            if (FromAccount is null)
            {
                SetErrors(_localizer.Localize("ErrorMessageEmptyFromAccount").ToEnumerable(), nameof(FromAccount));
                OnErrorChanged(nameof(FromAccount));
            }

            if (!(ToAccount is null)) return;

            SetErrors(_localizer.Localize("ErrorMessageEmptyToAccount").ToEnumerable(), nameof(ToAccount));
            OnErrorChanged(nameof(ToAccount));
        }

        protected override void NotifyRelevantAccountsToRefreshTrans()
        {
            FromAccount?.RefreshTransCollection();
            ToAccount?.RefreshTransCollection();
            _summaryAccountViewModel.RefreshTransCollection();
        }

        protected override void NotifyRelevantAccountsToRefreshBalance()
        {
            FromAccount?.RefreshBalance();
            ToAccount?.RefreshBalance();
            _summaryAccountViewModel.RefreshBalance();
        }


        private void RefreshAnAccountViewModel(IAccountBaseViewModel accountViewModel)
        {
            accountViewModel?.RefreshTransCollection();
            accountViewModel?.RefreshBalance();
        }
    }
}
