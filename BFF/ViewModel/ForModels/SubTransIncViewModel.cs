using BFF.DB;
using BFF.Model.Native;

namespace BFF.ViewModel.ForModels
{
    class SubTransIncViewModel : TitViewModelBase
    {
        protected readonly ISubTransInc SubTransInc;

        #region SubTransaction/SubIncome Properties
        
        public Category Category
        {
            get { return SubTransInc.Category; }
            set
            {
                SubTransInc.Category = value;
                OnPropertyChanged();
            }
        }
        public override string Memo
        {
            get { return SubTransInc.Memo; }
            set
            {
                SubTransInc.Memo = value;
                OnPropertyChanged();
            }
        }
        public override long Sum
        {
            get { return SubTransInc.Sum; }
            set
            {
                SubTransInc.Sum = value;
                OnPropertyChanged();
            }
        }

        #endregion
        public SubTransIncViewModel(ISubTransInc subTransInc, IBffOrm orm) : base(orm)
        {
            SubTransInc = subTransInc;
        }

    }
}
