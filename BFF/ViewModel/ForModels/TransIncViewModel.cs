using System;
using BFF.DB;
using BFF.Model.Native;

namespace BFF.ViewModel.ForModels
{
    class TransIncViewModel : TitViewModelBase
    {
        protected readonly ITransInc TransInc;

        #region Transaction/Income Properties
        
        public Account Account
        {
            get { return TransInc.Account; }
            set
            {
                TransInc.Account = value;
                OnPropertyChanged();
            }
        }
        public DateTime Date
        {
            get { return TransInc.Date; }
            set
            {
                TransInc.Date = value;
                OnPropertyChanged();
            }
        }
        public Payee Payee
        {
            get { return TransInc.Payee; }
            set
            {
                TransInc.Payee = value;
                OnPropertyChanged();
            }
        }
        public Category Category
        {
            get { return TransInc.Category; }
            set
            {
                TransInc.Category = value;
                OnPropertyChanged();
            }
        }
        public override string Memo
        {
            get { return TransInc.Memo; }
            set
            {
                TransInc.Memo = value;
                OnPropertyChanged();
            }
        }
        public override long Sum
        {
            get { return TransInc.Sum; }
            set
            {
                TransInc.Sum = value;
                OnPropertyChanged();
            }
        }
        public bool Cleared
        {
            get { return TransInc.Cleared; }
            set
            {
                TransInc.Cleared = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public TransIncViewModel(ITransInc transInc, IBffOrm orm) : base(orm)
        {
            TransInc = transInc;
        }
    }
}
