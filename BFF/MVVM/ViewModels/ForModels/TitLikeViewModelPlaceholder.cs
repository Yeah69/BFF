using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using BFF.Helper;
using BFF.MVVM.ViewModels.ForModels.Structure;
using Reactive.Bindings;

namespace BFF.MVVM.ViewModels.ForModels
{
    public interface ITransLikeViewModelPlaceholder : ITransLikeViewModel
    {
    }

    /// <summary>
    /// A TIT ViewModel Placeholder used for async lazy loaded TIT's.
    /// </summary>
    public sealed class TransLikeViewModelPlaceholder : ITransLikeViewModelPlaceholder
    {
        /// <summary>
        /// Needed to mimic a TIT.
        /// </summary>
        public IReactiveProperty<string> Memo { get;}

        /// <summary>
        /// Needed to mimic a TIT.
        /// </summary>
        public IReactiveProperty<long> Sum { get; }

        public Sign SumSign => Sign.Plus;
        public ISumEditViewModel SumEdit { get; }
        public long SumAbsolute => 0L;

        public ReactiveCommand DeleteCommand { get; }
        public IReadOnlyReactiveProperty<bool> IsInserted { get; }
        public ReactiveCommand ToggleSign { get; } = new ReactiveCommand();
        public IObservable<Unit> RemoveRequests => Observable.Never<Unit>();
        public ReactiveCommand RemoveCommand { get; } = new ReactiveCommand();
        public IAccountBaseViewModel Owner => null;

        /// <summary>
        /// Needed to mimic a TIT.
        /// </summary>
        public IReactiveProperty<bool> Cleared { get; }

        /// <summary>
        /// Initializes the TransBase-parts of the object
        /// </summary>
        public TransLikeViewModelPlaceholder(
            Func<IReactiveProperty<long>, 
            ISumEditViewModel> sumEditFactory)
        {
            Memo = new ReactiveProperty<string>("Content is loading…", ReactivePropertyMode.DistinctUntilChanged); // ToDo Localize
            Sum = new ReactiveProperty<long>(0L, ReactivePropertyMode.DistinctUntilChanged);
            SumEdit = sumEditFactory(new ReactiveProperty<long>());
            Cleared = new ReactiveProperty<bool>(false, ReactivePropertyMode.DistinctUntilChanged);
            DeleteCommand = new ReactiveCommand();
            IsInserted = new ReadOnlyReactivePropertySlim<bool>(Observable.Never<bool>(), false, ReactivePropertyMode.DistinctUntilChanged);
        }

        #region Overrides of TransLikeViewModel

        public Task InsertAsync()
        {
            throw new NotImplementedException();
        }

        public bool IsInsertable() => false;

        public Task DeleteAsync()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
