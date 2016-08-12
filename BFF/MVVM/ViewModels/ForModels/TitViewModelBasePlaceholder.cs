using BFF.DB;

namespace BFF.MVVM.ViewModels.ForModels
{
    sealed class TitViewModelBasePlaceholder : TitViewModelBase
    {
        private string _memo;
        private long _sum;

        public override long Id => -2L;

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

        internal override bool ValidToInsert() => false;

        protected override void InsertToDb() { }

        protected override void UpdateToDb() { }

        protected override void DeleteFromDb() { }

        #endregion
    }
}
