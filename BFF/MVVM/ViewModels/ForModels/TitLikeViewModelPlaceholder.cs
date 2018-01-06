using BFF.MVVM.ViewModels.ForModels.Structure;
using Reactive.Bindings;

namespace BFF.MVVM.ViewModels.ForModels
{
    /// <summary>
    /// A TIT ViewModel Placeholder used for async lazy loaded TIT's.
    /// </summary>
    sealed class TransLikeViewModelPlaceholder : ITransLikeViewModel
    {
        /// <summary>
        /// Needed to mimic a TIT.
        /// </summary>
        public IReactiveProperty<string> Memo { get;}

        /// <summary>
        /// Needed to mimic a TIT.
        /// </summary>
        public IReactiveProperty<long> Sum { get; }

        public ReactiveCommand DeleteCommand { get; }

        /// <summary>
        /// Needed to mimic a TIT.
        /// </summary>
        public IReactiveProperty<bool> Cleared { get; }

        /// <summary>
        /// Initializes the TransBase-parts of the object
        /// </summary>
        public TransLikeViewModelPlaceholder()
        {
            Memo = new ReactiveProperty<string>("Content is loading…");
            Sum = new ReactiveProperty<long>(0L);
            Cleared = new ReactiveProperty<bool>(false);
            DeleteCommand = new ReactiveCommand();
        }

        #region Overrides of TransLikeViewModel

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
