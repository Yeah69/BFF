using System;
using BFF.DB;
using BFF.Model.Native;

namespace BFF.ViewModel.ForModels
{
    class TransferViewModel : TitViewModelBase
    {
        protected readonly Transfer _transferModel;

        #region Transfer Properties

        public DateTime Date
        {
            get { return _transferModel.Date; }
            set
            {
                _transferModel.Date = value;
                OnPropertyChanged();
            }
        }
        public Account FromAccount
        {
            get { return _transferModel.FromAccount; }
            set
            {
                _transferModel.FromAccount = value;
                OnPropertyChanged();
            }
        }
        public Account ToAccount
        {
            get { return _transferModel.ToAccount; }
            set
            {
                _transferModel.ToAccount = value;
                OnPropertyChanged();
            }
        }
        public override string Memo
        {
            get { return _transferModel.Memo; }
            set
            {
                _transferModel.Memo = value;
                OnPropertyChanged();
            }
        }
        public override long Sum
        {
            get { return _transferModel.Sum; }
            set
            {
                _transferModel.Sum = value;
                OnPropertyChanged();
            }
        }
        public bool Cleared
        {
            get { return _transferModel.Cleared; }
            set
            {
                _transferModel.Cleared = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public TransferViewModel(Transfer transferModel, IBffOrm orm) : base(orm)
        {
            _transferModel = transferModel;
        }
    }
}
