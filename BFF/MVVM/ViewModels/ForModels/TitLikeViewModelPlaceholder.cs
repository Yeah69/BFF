using BFF.DB;
using BFF.MVVM.ViewModels.ForModels.Structure;

namespace BFF.MVVM.ViewModels.ForModels
{
    /// <summary>
    /// A TIT ViewModel Placeholder used for asyncron lazy loaded TIT's.
    /// </summary>
    sealed class TitLikeViewModelPlaceholder : TitLikeViewModel
    {
        /// <summary>
        /// Needed to mimic a TIT.
        /// </summary>
        public override long Id => -2L;

        /// <summary>
        /// Needed to mimic a TIT.
        /// </summary>
        public override string Memo { get; set; }

        /// <summary>
        /// Needed to mimic a TIT.
        /// </summary>
        public override long Sum { get; set; }

        /// <summary>
        /// Needed to mimic a TIT.
        /// </summary>
        public bool Cleared { get; set; }

        /// <summary>
        /// Initializes the TitBase-parts of the object
        /// </summary>
        /// <param name="orm">Needed to mimic a TIT.</param>
        public TitLikeViewModelPlaceholder(IBffOrm orm)
            : base(orm)
        {
            Memo = "Content is loading…";
        }

        #region Overrides of TitLikeViewModel

        /// <summary>
        /// Does only return False, because a Placeholder may not be inserted to the database. Needed to mimic a TIT.
        /// </summary>
        /// <returns>Only False.</returns>
        internal override bool ValidToInsert() => false;

        /// <summary>
        /// Does nothing, because this is a Placeholder. Needed to mimic a TIT.
        /// </summary>
        protected override void InsertToDb() { }

        /// <summary>
        /// Does nothing, because this is a Placeholder. Needed to mimic a TIT.
        /// </summary>
        protected override void UpdateToDb() { }

        /// <summary>
        /// Does nothing, because this is a Placeholder. Needed to mimic a TIT.
        /// </summary>
        protected override void DeleteFromDb() { }

        #endregion
    }
}
