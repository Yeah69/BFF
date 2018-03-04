using System;
using System.Reactive.Linq;
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

        /// <summary>
        /// Initializes a TransferViewModel.
        /// </summary>
        /// <param name="transfer">A Transfer Model.</param>
        /// <param name="accountViewModelService">Fetches the accounts.</param>
        /// <param name="createSumEdit">Creates a sum editing viewmodel.</param>
        /// <param name="flagViewModelService">Fetches the flags.</param>
        public TransferViewModel(
            ITransfer transfer, 
            IAccountViewModelService accountViewModelService,
            Func<IHaveFlagViewModel, INewFlagViewModel> newFlagViewModelFactory,
            Func<IReactiveProperty<long>, ISumEditViewModel> createSumEdit,
            IFlagViewModelService flagViewModelService) : base(transfer, newFlagViewModelFactory, flagViewModelService)
        {
            _accountViewModelService = accountViewModelService;

            FromAccount = transfer
                .ToReactivePropertyAsSynchronized(
                    t => t.FromAccount,
                    accountViewModelService.GetViewModel, 
                    accountViewModelService.GetModel, 
                    ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            FromAccount
                .SkipLast(1)
                .Where(_ => transfer.Id != -1L)
                .Subscribe(RefreshAnAccountViewModel)
                .AddTo(CompositeDisposable);

            FromAccount
                .Where(_ => transfer.Id != -1L)
                .Subscribe(RefreshAnAccountViewModel)
                .AddTo(CompositeDisposable);

            ToAccount = transfer
                .ToReactivePropertyAsSynchronized(
                    t => t.ToAccount, 
                    accountViewModelService.GetViewModel,
                    accountViewModelService.GetModel,
                    ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            ToAccount
                .SkipLast(1)
                .Where(_ => transfer.Id != -1L)
                .Subscribe(RefreshAnAccountViewModel)
                .AddTo(CompositeDisposable);

            ToAccount
                .Where(_ => transfer.Id != -1L)
                .Subscribe(RefreshAnAccountViewModel)
                .AddTo(CompositeDisposable);

            Sum = transfer.ToReactivePropertyAsSynchronized(t => t.Sum, ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);
            Sum
                .Where(_ => transfer.Id != -1L)
                .Subscribe(sum =>
                {
                    FromAccount.Value?.RefreshBalance();
                    ToAccount.Value?.RefreshBalance();
                }).AddTo(CompositeDisposable);

            SumEdit = createSumEdit(Sum);
        }

        public override ISumEditViewModel SumEdit { get; }

        public override void Delete()
        {
            base.Delete();
            RefreshAnAccountViewModel(FromAccount.Value);
            RefreshAnAccountViewModel(ToAccount.Value);
            Messenger.Default.Send(SummaryAccountMessage.RefreshTits);
            Messenger.Default.Send(SummaryAccountMessage.RefreshBalance);
        }

        protected override void NotifyRelevantAccountsToRefreshTits()
        {
            FromAccount.Value?.RefreshTits();
            ToAccount.Value?.RefreshTits();
            Messenger.Default.Send(SummaryAccountMessage.RefreshTits);
        }

        protected override void NotifyRelevantAccountsToRefreshBalance()
        {
            FromAccount.Value?.RefreshBalance();
            ToAccount.Value?.RefreshBalance();
            Messenger.Default.Send(SummaryAccountMessage.RefreshBalance);
        }


        private void RefreshAnAccountViewModel(IAccountBaseViewModel accountViewModel)
        {
            accountViewModel?.RefreshTits();
            accountViewModel?.RefreshBalance();
        }
    }
}
