using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BFF.Helper;
using BFF.Helper.Extensions;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;
using MuVaViMo;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface ITransferViewModel : ITransBaseViewModel
    {
        /// <summary>
        /// The account from where the money is transfered.
        /// </summary>
        IAccountViewModel FromAccount { get; set; }

        /// <summary>
        /// The account to where the money is transfered.
        /// </summary>
        IAccountViewModel ToAccount { get; set; }
    }

    /// <summary>
    /// The ViewModel of the Model Transfer.
    /// </summary>
    public sealed class TransferViewModel : TransBaseViewModel, ITransferViewModel
    {
        private readonly ITransfer _transfer;
        private readonly IAccountViewModelService _accountViewModelService;
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

        /// <summary>
        /// The amount of money, which is transfered.
        /// </summary>
        public override IReactiveProperty<long> Sum { get; }

        public TransferViewModel(
            ITransfer transfer, 
            IAccountViewModelService accountViewModelService,
            INewFlagViewModel newFlagViewModel,
            Func<IReactiveProperty<long>, ISumEditViewModel> createSumEdit,
            ILastSetDate lastSetDate,
            IRxSchedulerProvider rxSchedulerProvider,
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
            _summaryAccountViewModel = summaryAccountViewModel;

            _fromAccount = _accountViewModelService.GetViewModel(transfer.FromAccount);
            transfer
                .ObservePropertyChanges(nameof(transfer.FromAccount))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ =>
                {
                    _fromAccount = _accountViewModelService.GetViewModel(transfer.FromAccount);
                    OnPropertyChanged(nameof(FromAccount));
                })
                .AddTo(CompositeDisposable);

            _toAccount = _accountViewModelService.GetViewModel(transfer.ToAccount);
            transfer
                .ObservePropertyChanges(nameof(transfer.ToAccount))
                .ObserveOn(rxSchedulerProvider.UI)
                .Subscribe(_ =>
                {
                    _toAccount = _accountViewModelService.GetViewModel(transfer.ToAccount);
                    OnPropertyChanged(nameof(ToAccount));
                })
                .AddTo(CompositeDisposable);

            if (FromAccount is null && owner is IAccountViewModel specificAccount)
                FromAccount = specificAccount;

            transfer
                .ObservePropertyChanges(t => t.FromAccount)
                .SkipLast(1)
                .Where(_ => transfer.Id != -1L)
                .Subscribe(fa => RefreshAnAccountViewModel(accountViewModelService.GetViewModel(fa)))
                .AddTo(CompositeDisposable);

            transfer
                .ObservePropertyChanges(t => t.FromAccount)
                .Where(_ => transfer.Id != -1L)
                .Subscribe(fa => RefreshAnAccountViewModel(accountViewModelService.GetViewModel(fa)))
                .AddTo(CompositeDisposable);

            transfer
                .ObservePropertyChanges(t => t.ToAccount)
                .SkipLast(1)
                .Where(_ => transfer.Id != -1L)
                .Subscribe(fa => RefreshAnAccountViewModel(accountViewModelService.GetViewModel(fa)))
                .AddTo(CompositeDisposable);

            transfer
                .ObservePropertyChanges(t => t.ToAccount)
                .Where(_ => transfer.Id != -1L)
                .Subscribe(fa => RefreshAnAccountViewModel(accountViewModelService.GetViewModel(fa)))
                .AddTo(CompositeDisposable);

            Sum = transfer.ToReactivePropertyAsSynchronized(
                nameof(transfer.Sum),
                () => transfer.Sum,
                s => transfer.Sum = s,
                rxSchedulerProvider.UI,
                ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);

            transfer
                .ObservePropertyChanges(t => t.Sum)
                .Where(_ => transfer.Id != -1L)
                .Subscribe(sum =>
                {
                    FromAccount?.RefreshBalance();
                    ToAccount?.RefreshBalance();
                }).AddTo(CompositeDisposable);

            SumEdit = createSumEdit(Sum);
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

        protected override void NotifyRelevantAccountsToRefreshTits()
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
