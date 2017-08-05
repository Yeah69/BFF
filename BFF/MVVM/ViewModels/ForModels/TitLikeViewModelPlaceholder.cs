using System.Windows.Input;
using BFF.MVVM.ViewModels.ForModels.Structure;
using Reactive.Bindings;

namespace BFF.MVVM.ViewModels.ForModels
{
    /// <summary>
    /// A TIT ViewModel Placeholder used for asyncron lazy loaded TIT's.
    /// </summary>
    sealed class TitLikeViewModelPlaceholder : ITitLikeViewModel
    {
        /// <summary>
        /// Needed to mimic a TIT.
        /// </summary>
        public long Id => -2L;

        /// <summary>
        /// Needed to mimic a TIT.
        /// </summary>
        public IReactiveProperty<string> Memo { get;}

        /// <summary>
        /// Needed to mimic a TIT.
        /// </summary>
        public IReactiveProperty<long> Sum { get; }

        public ICommand DeleteCommand { get; }

        /// <summary>
        /// Needed to mimic a TIT.
        /// </summary>
        public bool Cleared { get; set; }

        /// <summary>
        /// Initializes the TitBase-parts of the object
        /// </summary>
        public TitLikeViewModelPlaceholder()
        {
            Memo = new ReactiveProperty<string>("Content is loading…");
            Sum = new ReactiveProperty<long>(0L);
        }

        #region Overrides of TitLikeViewModel

        /// <summary>
        /// Does only return False, because a Placeholder may not be inserted to the database. Needed to mimic a TIT.
        /// </summary>
        /// <returns>Only False.</returns>
        public bool ValidToInsert() => false;

        public void Insert()
        {
            throw new System.NotImplementedException();
        }

        public void Delete()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
