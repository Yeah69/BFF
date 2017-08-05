using System.Windows.Input;
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
        //{
        //    get => _transfer.FromAccountId == -1 ? null : 
        //               CommonPropertyProvider.GetAccountViewModel(_transfer.FromAccountId);
        //    set
        //    {
        //        if(value?.Id == _transfer.FromAccountId) return;
        //        IAccountViewModel temp = FromAccount;
        //        bool accountSwitch = false;
        //        if (value != null && _transfer.ToAccountId == value.Id) // If value equals ToAccount, then the FromAccount and ToAccount switch values
        //        {
        //            _transfer.ToAccountId = _transfer.FromAccountId;
        //            OnPropertyChanged(nameof(ToAccount));
        //            accountSwitch = true;
        //        }
        //        _transfer.FromAccountId = value?.Id ?? -1;
        //        OnUpdate();
        //        if(accountSwitch && ToAccount != null) //Refresh ToAccount if switch occurred
        //        {
        //            Messenger.Default.Send(AccountMessage.RefreshTits, ToAccount);
        //            Messenger.Default.Send(AccountMessage.RefreshBalance, ToAccount);
        //        }
        //        if(FromAccount != null) //Refresh FromAccount if it exists
        //        {
        //            Messenger.Default.Send(AccountMessage.RefreshTits, FromAccount);
        //            Messenger.Default.Send(AccountMessage.RefreshBalance, FromAccount);
        //        }
        //        if(!accountSwitch && temp != null && temp != FromAccount) //if switch happened then with temp is now ToAccount and was refreshed already, if not then refresh if it exists and is not FromAccount
        //        { 
        //            Messenger.Default.Send(AccountMessage.RefreshTits, temp);
        //            Messenger.Default.Send(AccountMessage.RefreshBalance, temp);
        //        }
        //        OnPropertyChanged();
        //    }
        //}

        /// <summary>
        /// The account to where the money is transferred.
        /// </summary>
        public IReactiveProperty<IAccountViewModel> ToAccount { get; }
        //{
        //    get => _transfer.ToAccountId == -1 ? null :
        //               CommonPropertyProvider.GetAccountViewModel(_transfer.ToAccountId);
        //    set
        //    {
        //        if (value?.Id == _transfer.ToAccountId) return;
        //        IAccountViewModel temp = ToAccount;
        //        bool accountSwitch = false;
        //        if (value != null && _transfer.FromAccountId == value.Id) // If value equals FromAccount, then the ToAccount and FromAccount switch values
        //        {
        //            _transfer.FromAccountId = _transfer.ToAccountId;
        //            OnPropertyChanged(nameof(FromAccount));
        //            accountSwitch = true;
        //        }
        //        _transfer.ToAccountId = value?.Id ?? -1;
        //        OnUpdate();
        //        if (accountSwitch && FromAccount != null) //Refresh ToAccount if switch occurred
        //        {
        //            Messenger.Default.Send(AccountMessage.RefreshTits, FromAccount);
        //            Messenger.Default.Send(AccountMessage.RefreshBalance, FromAccount);
        //        }
        //        if (ToAccount != null) //Refresh FromAccount if it exists
        //        {
        //            Messenger.Default.Send(AccountMessage.RefreshTits, ToAccount);
        //            Messenger.Default.Send(AccountMessage.RefreshBalance, ToAccount);
        //        }
        //        if (!accountSwitch && temp != null && temp != ToAccount) //if switch happened then with temp is now FromAccount and was refreshed already, if not then refresh if it exists and is not ToAccount
        //        {
        //            Messenger.Default.Send(AccountMessage.RefreshTits, temp);
        //            Messenger.Default.Send(AccountMessage.RefreshBalance, temp);
        //        }
        //        OnPropertyChanged();
        //    }
        //}

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
        public TransferViewModel(ITransfer transfer, IBffOrm orm, AccountViewModelService accountAccountViewModelService) : base(orm, transfer)
        {
            FromAccount = transfer.ToReactivePropertyAsSynchronized(t => t.FromAccount, accountAccountViewModelService.GetViewModel, accountAccountViewModelService.GetModel).AddTo(CompositeDisposable);
            ToAccount = transfer.ToReactivePropertyAsSynchronized(t => t.ToAccount, accountAccountViewModelService.GetViewModel, accountAccountViewModelService.GetModel).AddTo(CompositeDisposable);
            Sum = transfer.ToReactivePropertyAsSynchronized(t => t.Sum).AddTo(CompositeDisposable);
            //Sum.Subscribe(sum =>
            //{
            //    OnUpdate();
            //    Messenger.Default.Send(AccountMessage.RefreshBalance, FromAccount);
            //    Messenger.Default.Send(AccountMessage.RefreshBalance, ToAccount);
            //}).AddTo(CompositeDisposable);
        }

        /// <summary>
        /// Before a model object is inserted into the database, it has to be valid.
        /// This function should guarantee that the object is valid to be inserted.
        /// </summary>
        /// <returns>True if valid, else false</returns>
        public override bool ValidToInsert()
        {
            return FromAccount != null && ToAccount != null;
        }

        /// <summary>
        /// Uses the OR mapper to update the model in the database. Inner function for the Update method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void OnUpdate()
        {
            Messenger.Default.Send(AccountMessage.RefreshTits, FromAccount);
            Messenger.Default.Send(AccountMessage.RefreshTits, ToAccount);
            Messenger.Default.Send(SummaryAccountMessage.RefreshTits);
        }

        /// <summary>
        /// Deletes the model from the database and refreshes the accounts, which it belonged to, and the summary account.
        /// </summary>
        public override ICommand DeleteCommand => new RelayCommand(obj =>
        {
            Delete();
            Messenger.Default.Send(AccountMessage.Refresh, FromAccount);
            Messenger.Default.Send(AccountMessage.Refresh, ToAccount);
            Messenger.Default.Send(SummaryAccountMessage.RefreshTits);
        });
    }
}
