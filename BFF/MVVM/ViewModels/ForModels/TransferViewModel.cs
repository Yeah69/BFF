using System;
using System.Windows.Input;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    internal interface ITransferViewModel : ITitBaseViewModel
    {
        /// <summary>
        /// The account from where the money is transfered.
        /// </summary>
        IAccount FromAccount { get; set; }

        /// <summary>
        /// The account to where the money is transfered.
        /// </summary>
        IAccount ToAccount { get; set; }
    }

    /// <summary>
    /// The ViewModel of the Model Transfer.
    /// </summary>
    class TransferViewModel : TitBaseViewModel, ITransferViewModel
    {
        /// <summary>
        /// The Transfer Model.
        /// </summary>
        protected readonly ITransfer Transfer;

        #region Transfer Properties

        /// <summary>
        /// The object's Id in the table of the database.
        /// </summary>
        public override long Id => Transfer.Id;

        /// <summary>
        /// This timestamp marks the time point, when the Transfer happened.
        /// </summary>
        public override DateTime Date
        {
            get { return Transfer.Date; }
            set
            {
                Transfer.Date = value;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The account from where the money is transfered.
        /// </summary>
        public IAccount FromAccount
        {
            get {
                return Transfer.FromAccountId == -1 ? null : 
                    Orm?.CommonPropertyProvider.GetAccount(Transfer.FromAccountId);
            }
            set
            {
                if(value?.Id == Transfer.FromAccountId) return;
                IAccount temp = FromAccount;
                bool accountSwitch = false;
                if (ToAccount == value) // If value equals ToAccount, then the FromAccount and ToAccount switch values
                {
                    Transfer.ToAccountId = Transfer.FromAccountId;
                    OnPropertyChanged(nameof(ToAccount));
                    accountSwitch = true;
                }
                Transfer.FromAccountId = value?.Id ?? -1;
                Update();
                if(accountSwitch && ToAccount != null) //Refresh ToAccount if switch occured
                {
                    Messenger.Default.Send(AccountMessage.RefreshTits, ToAccount);
                    Messenger.Default.Send(AccountMessage.RefreshBalance, ToAccount);
                }
                if(FromAccount != null) //Refresh FromAccount if it exists
                {
                    Messenger.Default.Send(AccountMessage.RefreshTits, FromAccount);
                    Messenger.Default.Send(AccountMessage.RefreshBalance, FromAccount);
                }
                if(!accountSwitch && temp != null && temp != FromAccount) //if switch happened then with temp is now ToAccount and was refreshed already, if not then refresh if it exists and is not FromAccount
                { 
                    Messenger.Default.Send(AccountMessage.RefreshTits, temp);
                    Messenger.Default.Send(AccountMessage.RefreshBalance, temp);
                }
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The account to where the money is transfered.
        /// </summary>
        public IAccount ToAccount
        {
            get
            {
                return Transfer.ToAccountId == -1 ? null :
                    Orm?.CommonPropertyProvider.GetAccount(Transfer.ToAccountId);
            }
            set
            {
                if (value?.Id == Transfer.ToAccountId) return;
                IAccount temp = ToAccount;
                bool accountSwitch = false;
                if (FromAccount == value) // If value equals FromAccount, then the ToAccount and FromAccount switch values
                {
                    Transfer.FromAccountId = Transfer.ToAccountId;
                    OnPropertyChanged(nameof(FromAccount));
                    accountSwitch = true;
                }
                Transfer.ToAccountId = value?.Id ?? -1;
                Update();
                if (accountSwitch && FromAccount != null) //Refresh ToAccount if switch occured
                {
                    Messenger.Default.Send(AccountMessage.RefreshTits, FromAccount);
                    Messenger.Default.Send(AccountMessage.RefreshBalance, FromAccount);
                }
                if (ToAccount != null) //Refresh FromAccount if it exists
                {
                    Messenger.Default.Send(AccountMessage.RefreshTits, ToAccount);
                    Messenger.Default.Send(AccountMessage.RefreshBalance, ToAccount);
                }
                if (!accountSwitch && temp != null && temp != ToAccount) //if switch happened then with temp is now FromAccount and was refreshed already, if not then refresh if it exists and is not ToAccount
                {
                    Messenger.Default.Send(AccountMessage.RefreshTits, temp);
                    Messenger.Default.Send(AccountMessage.RefreshBalance, temp);
                }
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// A note, which a user can attach to each TIT as a reminder for himself.
        /// </summary>
        public override string Memo
        {
            get { return Transfer.Memo; }
            set
            {
                Transfer.Memo = value;
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The amount of money, which is transfered.
        /// </summary>
        public override long Sum
        {
            get { return Transfer.Sum; }
            set
            {
                Transfer.Sum = value;
                Update();
                Messenger.Default.Send(AccountMessage.RefreshBalance, FromAccount);
                Messenger.Default.Send(AccountMessage.RefreshBalance, ToAccount);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Like the Memo the Cleared flag is an aid for the user.
        /// It can be used to mark TITs, which the user thinks is processed enough (True) or needs to be changed later (False).
        /// This maybe needed, if the user does not remember everything clearly and wants to finish the Tit later.
        /// </summary>
        public override bool Cleared
        {
            get { return Transfer.Cleared; }
            set
            {
                Transfer.Cleared = value;
                Update();
                OnPropertyChanged();
            }
        }

        #endregion

        /// <summary>
        /// Initializes a TransferViewModel.
        /// </summary>
        /// <param name="transfer">A Transfer Model.</param>
        /// <param name="orm">Used for the database accesses.</param>
        public TransferViewModel(ITransfer transfer, IBffOrm orm) : base(orm)
        {
            Transfer = transfer;
        }

        /// <summary>
        /// Before a model object is inserted into the database, it has to be valid.
        /// This function should guarantee that the object is valid to be inserted.
        /// </summary>
        /// <returns>True if valid, else false</returns>
        public override bool ValidToInsert()
        {
            return FromAccount != null && (Orm?.CommonPropertyProvider.Accounts.Contains(FromAccount) ?? false) &&
                   ToAccount != null   &&  Orm .CommonPropertyProvider.Accounts.Contains(ToAccount);
        }

        /// <summary>
        /// Uses the OR mapper to insert the model into the database. Inner function for the Insert method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void InsertToDb()
        {
            Transfer.Insert(Orm);
        }

        /// <summary>
        /// Uses the OR mapper to update the model in the database. Inner function for the Update method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void UpdateToDb()
        {
            Transfer.Update(Orm);
            Messenger.Default.Send(AccountMessage.RefreshTits, FromAccount);
            Messenger.Default.Send(AccountMessage.RefreshTits, ToAccount);
            Messenger.Default.Send(SummaryAccountMessage.RefreshTits);
        }

        /// <summary>
        /// Uses the OR mapper to delete the model from the database. Inner function for the Delete method.
        /// The Orm works in a generic way and determines the right table by the given type.
        /// In order to avoid the need to update a huge if-else construct to select the right type, each concrete class calls the ORM itself.
        /// </summary>
        protected override void DeleteFromDb()
        {
            Transfer.Delete(Orm);
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
