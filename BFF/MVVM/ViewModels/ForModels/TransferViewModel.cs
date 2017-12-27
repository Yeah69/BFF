using System;
using System.Reactive.Linq;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;
using MuVaViMo;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels
{
    internal interface ITransferViewModel : ITransBaseViewModel
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
    public class TransferViewModel : TransBaseViewModel, ITransferViewModel
    {

        public IObservableReadOnlyList<IAccountViewModel> AllAccounts => CommonPropertyProvider.AllAccountViewModels;

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
        public sealed override IReactiveProperty<long> Sum { get; }

        /// <summary>
        /// Initializes a TransferViewModel.
        /// </summary>
        /// <param name="transfer">A Transfer Model.</param>
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="accountViewModelService"></param>
        public TransferViewModel(
            ITransfer transfer, 
            IBffOrm orm, 
            IAccountViewModelService accountViewModelService,
            IFlagViewModelService flagViewModelService) : base(orm, transfer, flagViewModelService)
        {

            FromAccount = transfer
                .ToReactivePropertyAsSynchronized(t => t.FromAccount, accountViewModelService.GetViewModel, accountViewModelService.GetModel)
                .AddTo(CompositeDisposable);

            FromAccount
                .SkipLast(1)
                .Subscribe(RefreshAnAccountViewModel)
                .AddTo(CompositeDisposable);

            FromAccount
                .Skip(1)
                .Subscribe(RefreshAnAccountViewModel)
                .AddTo(CompositeDisposable);

            ToAccount = transfer
                .ToReactivePropertyAsSynchronized(t => t.ToAccount, accountViewModelService.GetViewModel, accountViewModelService.GetModel)
                .AddTo(CompositeDisposable);

            ToAccount
                .SkipLast(1)
                .Subscribe(RefreshAnAccountViewModel)
                .AddTo(CompositeDisposable);

            ToAccount
                .Skip(1)
                .Subscribe(RefreshAnAccountViewModel)
                .AddTo(CompositeDisposable);

            Sum = transfer.ToReactivePropertyAsSynchronized(t => t.Sum, ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);
            Sum.Subscribe(sum =>
            {
                FromAccount.Value?.RefreshBalance();
                ToAccount.Value?.RefreshBalance();
            }).AddTo(CompositeDisposable);
        }

        /// <summary>
        /// Before a model object is inserted into the database, it has to be valid.
        /// This function should guarantee that the object is valid to be inserted.
        /// </summary>
        /// <returns>True if valid, else false</returns>
        public override bool ValidToInsert()
        {
            return FromAccount.Value != null && ToAccount.Value != null;
        }

        protected override void InitializeDeleteCommand()
        {
            DeleteCommand.Subscribe(_ =>
            {
                Delete();
                RefreshAnAccountViewModel(FromAccount.Value);
                RefreshAnAccountViewModel(ToAccount.Value);
                Messenger.Default.Send(SummaryAccountMessage.RefreshTits);
                Messenger.Default.Send(SummaryAccountMessage.RefreshBalance);
            });
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
