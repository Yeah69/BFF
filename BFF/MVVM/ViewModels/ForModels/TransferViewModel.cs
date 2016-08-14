using System;
using System.Windows.Input;
using BFF.DB;
using BFF.MVVM.Models.Native;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    class TransferViewModel : TitBaseViewModel
    {
        protected readonly Transfer Transfer;

        #region Transfer Properties

        public override long Id => Transfer.Id;

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
        public Account FromAccount
        {
            get {
                return Transfer.FromAccountId == -1 ? null : 
                    Orm?.CommonPropertyProvider.GetAccount(Transfer.FromAccountId);
            }
            set
            {
                if(value?.Id == Transfer.FromAccountId) return;
                Account temp = FromAccount;
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
        public Account ToAccount
        {
            get
            {
                return Transfer.ToAccountId == -1 ? null :
                    Orm?.CommonPropertyProvider.GetAccount(Transfer.ToAccountId);
            }
            set
            {
                if (value?.Id == Transfer.ToAccountId) return;
                Account temp = ToAccount;
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

        public TransferViewModel(Transfer transfer, IBffOrm orm) : base(orm)
        {
            Transfer = transfer;
        }

        internal override bool ValidToInsert()
        {
            return FromAccount != null && (Orm?.CommonPropertyProvider.Accounts.Contains(FromAccount) ?? false) &&
                   ToAccount != null   &&  Orm .CommonPropertyProvider.Accounts.Contains(ToAccount);
        }

        protected override void InsertToDb()
        {
            Orm?.Insert(Transfer);
        }

        protected override void UpdateToDb()
        {
            Orm?.Update(Transfer);
            Messenger.Default.Send(AccountMessage.RefreshTits, FromAccount);
            Messenger.Default.Send(AccountMessage.RefreshTits, ToAccount);
            Messenger.Default.Send(AllAccountMessage.RefreshTits);
        }

        protected override void DeleteFromDb()
        {
            Orm?.Delete(Transfer);
        }
        
        public override ICommand DeleteCommand => new RelayCommand(obj =>
        {
            Delete();
            Messenger.Default.Send(AccountMessage.Refresh, FromAccount);
            Messenger.Default.Send(AccountMessage.Refresh, ToAccount);
            Messenger.Default.Send(AllAccountMessage.RefreshTits);
        });
    }
}
