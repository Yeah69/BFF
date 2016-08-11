using BFF.DB;

namespace BFF.MVVM.ViewModels.ForModels
{
    sealed class TitViewModelBasePlaceholder : TitViewModelBase
    {
        private string _memo;
        private long _sum;

        /// <summary>
        /// Initializes the TitBase-parts of the object
        /// </summary>
        public TitViewModelBasePlaceholder(IBffOrm orm)
            : base(orm)
        {
            Memo = "Content is loading…";
        }

        #region Overrides of TitViewModelBase

        public override string Memo
        {
            get { return _memo; }
            set { _memo = value; }
        }

        public override long Sum
        {
            get { return _sum; }
            set { _sum = value; }
        }

        public override bool ValidToInsert()
        {
            return false;
        }

        public override void Insert() {}

        #endregion
    }
}
