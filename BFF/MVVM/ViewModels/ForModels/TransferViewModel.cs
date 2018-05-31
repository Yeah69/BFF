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
        IReactiveProperty<IAccountViewModel> FromAccount { get; }

        /// <summary>
        /// The account to where the money is transfered.
        /// </summary>
        IReactiveProperty<IAccountViewModel> ToAccount { get; }
    }

    /// <summary>
    /// The ViewModel of the Model Transfer.
    /// </summary>
    public sealed class TransferViewModel : TransBaseViewModel, ITransferViewModel
    {
        private readonly IAccountViewModelService _accountViewModelService;
        private readonly ISummaryAccountViewModel _summaryAccountViewModel;

        public IObservableReadOnlyList<IAccountViewModel> AllAccounts => _accountViewModelService.All;

        /// <summary>
        /// The account from where the money is transfered.
        /// </summary>
        public IReactiveProperty<IAccountViewModel> FromAccount { get; }

        /// <summary>
        /// The account to where the money is transferred.
        /// </summary>
        public IReactiveProperty<IAccountViewModel> ToAccount { get; }

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
            IRxSchedulerProvider schedulerProvider,
            ISummaryAccountViewModel summaryAccountViewModel,
            IFlagViewModelService flagViewModelService,
            IAccountBaseViewModel owner) 
            : base(
                transfer, 
                newFlagViewModel, 
                lastSetDate, 
                schedulerProvider, 
                flagViewModelService, 
                owner)
        {
            _accountViewModelService = accountViewModelService;
            _summaryAccountViewModel = summaryAccountViewModel;

            FromAccount = transfer
                .ToReactivePropertyAsSynchronized(
                    nameof(transfer.FromAccount),
                    () => transfer.FromAccount,
                    fa => transfer.FromAccount = fa,
                    accountViewModelService.GetViewModel, 
                    accountViewModelService.GetModel,
                    schedulerProvider.UI,
                    ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            ToAccount = transfer
                .ToReactivePropertyAsSynchronized(
                    nameof(transfer.ToAccount),
                    () => transfer.ToAccount,
                    ta => transfer.ToAccount = ta,
                    accountViewModelService.GetViewModel,
                    accountViewModelService.GetModel,
                    schedulerProvider.UI,
                    ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            if (FromAccount.Value is null && owner is IAccountViewModel specificAccount)
                FromAccount.Value = specificAccount;

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
                schedulerProvider.UI,
                ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);

            transfer
                .ObservePropertyChanges(t => t.Sum)
                .Where(_ => transfer.Id != -1L)
                .Subscribe(sum =>
                {
                    FromAccount.Value?.RefreshBalance();
                    ToAccount.Value?.RefreshBalance();
                }).AddTo(CompositeDisposable);

            SumEdit = createSumEdit(Sum);
        }

        public override ISumEditViewModel SumEdit { get; }

        public override bool IsInsertable() => base.IsInsertable() && FromAccount.Value.IsNotNull() && ToAccount.IsNotNull();

        public override async Task DeleteAsync()
        {
            await base.DeleteAsync();
            RefreshAnAccountViewModel(FromAccount.Value);
            RefreshAnAccountViewModel(ToAccount.Value);
            _summaryAccountViewModel.RefreshTits();
            _summaryAccountViewModel.RefreshBalance();
        }

        protected override void NotifyRelevantAccountsToRefreshTits()
        {
            FromAccount.Value?.RefreshTits();
            ToAccount.Value?.RefreshTits();
            _summaryAccountViewModel.RefreshTits();
        }

        protected override void NotifyRelevantAccountsToRefreshBalance()
        {
            FromAccount.Value?.RefreshBalance();
            ToAccount.Value?.RefreshBalance();
            _summaryAccountViewModel.RefreshBalance();
        }


        private void RefreshAnAccountViewModel(IAccountBaseViewModel accountViewModel)
        {
            accountViewModel?.RefreshTits();
            accountViewModel?.RefreshBalance();
        }
    }
}
