using System;
using BFF.DB;
using BFF.MVVM.Models.Native;

namespace BFF.MVVM.ViewModels.ForModels
{
    class TransferViewModel : TitViewModelBase
    {
        protected readonly Transfer Transfer;

        #region Transfer Properties

        public override long Id => Transfer.Id;

        public DateTime Date
        {
            get { return Transfer.Date; }
            set
            {
                Transfer.Date = value;
                OnPropertyChanged();
            }
        }
        public Account FromAccount
        {
            get { return Transfer.FromAccount; }
            set
            {
                Transfer.FromAccount = value;
                OnPropertyChanged();
            }
        }
        public Account ToAccount
        {
            get { return Transfer.ToAccount; }
            set
            {
                Transfer.ToAccount = value;
                OnPropertyChanged();
            }
        }
        public override string Memo
        {
            get { return Transfer.Memo; }
            set
            {
                Transfer.Memo = value;
                OnPropertyChanged();
            }
        }
        public override long Sum
        {
            get { return Transfer.Sum; }
            set
            {
                Transfer.Sum = value;
                OnPropertyChanged();
            }
        }

        public bool Cleared
        {
            get { return Transfer.Cleared; }
            set
            {
                Transfer.Cleared = value;
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
        }

        protected override void DeleteFromDb()
        {
            Orm?.Delete(Transfer);
        }
    }
}
