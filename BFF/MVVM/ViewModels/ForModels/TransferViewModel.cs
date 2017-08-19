using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels.ForModels.Structure;
using MuVaViMo;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels
{
    internal interface ITransferViewModel : ITitBaseViewModel
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
    public class TransferViewModel : TitBaseViewModel, ITransferViewModel
    {

        public IObservableReadOnlyList<IAccountViewModel> AllAccounts => CommonPropertyProvider.AllAccountViewModels;

        #region Transfer Properties

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

        #endregion

        /// <summary>
        /// Initializes a TransferViewModel.
        /// </summary>
        /// <param name="transfer">A Transfer Model.</param>
        /// <param name="orm">Used for the database accesses.</param>
        /// <param name="accountViewModelService"></param>
        public TransferViewModel(ITransfer transfer, IBffOrm orm, AccountViewModelService accountViewModelService) : base(orm, transfer)
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
                .Subscribe(avm =>
                {
                    RefreshAnAccountViewModel(avm);
                    Messenger.Default.Send(SummaryAccountMessage.RefreshTits);
                })
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
                .Subscribe(avm =>
                {
                    RefreshAnAccountViewModel(avm);
                    Messenger.Default.Send(SummaryAccountMessage.RefreshTits);
                })
                .AddTo(CompositeDisposable);

            Sum = transfer.ToReactivePropertyAsSynchronized(t => t.Sum, ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);
            Sum.Subscribe(sum =>
            {
                RefreshAnAccountViewModel(FromAccount.Value);
                RefreshAnAccountViewModel(ToAccount.Value);
                Messenger.Default.Send(SummaryAccountMessage.RefreshTits);
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

        /// <summary>
        /// Uses the OR mapper to update the model in the database. Inner function for the Update method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void OnUpdate()
        {
            Messenger.Default.Send(AccountMessage.RefreshTits, FromAccount.Value);
            Messenger.Default.Send(AccountMessage.RefreshTits, ToAccount.Value);
            Messenger.Default.Send(SummaryAccountMessage.RefreshTits);
        }

        /// <summary>
        /// Deletes the model from the database and refreshes the accounts, which it belonged to, and the summary account.
        /// </summary>
        public override ICommand DeleteCommand => new RelayCommand(obj =>
        {
            Delete();
            RefreshAnAccountViewModel(FromAccount.Value);
            RefreshAnAccountViewModel(ToAccount.Value);
            Messenger.Default.Send(SummaryAccountMessage.RefreshTits);
        });


        private void RefreshAnAccountViewModel(IAccountBaseViewModel accountViewModel)
        {
            accountViewModel?.RefreshTits();
            accountViewModel?.RefreshBalance();
        }
    }
}
