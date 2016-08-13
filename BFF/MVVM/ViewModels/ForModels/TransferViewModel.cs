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
            get { return Transfer.FromAccount; }
            set
            {
                Transfer.FromAccount = value;
                Update();
                OnPropertyChanged();
            }
        }
        public Account ToAccount
        {
            get { return Transfer.ToAccount; }
            set
            {
                Transfer.ToAccount = value;
                Update();
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
