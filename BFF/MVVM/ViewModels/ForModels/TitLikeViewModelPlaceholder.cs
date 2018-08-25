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
    /// A Trans ViewModel Placeholder used for async lazy loaded Trans'.
    /// </summary>
    public sealed class TransLikeViewModelPlaceholder : NotifyingErrorViewModelBase, ITransLikeViewModelPlaceholder
    {
        public string Memo { get; set; }
        
        public IReactiveProperty<long> Sum { get; }

        public Sign SumSign => Sign.Plus;
        public ISumEditViewModel SumEdit { get; }
        public long SumAbsolute => 0L;

        public IRxRelayCommand DeleteCommand { get; }
        public bool IsInserted { get; }
        public IRxRelayCommand ToggleSign { get; }
        public IObservable<Unit> RemoveRequests => Observable.Never<Unit>();
        public IRxRelayCommand RemoveCommand { get; }
        public IAccountBaseViewModel Owner => null;
        public void NotifyErrorsIfAny()
        {
        }

        public bool Cleared { get; set; }

        /// <summary>
        /// Initializes the TransBase-parts of the object
        /// </summary>
        public TransLikeViewModelPlaceholder(
            Func<IReactiveProperty<long>, 
            ISumEditViewModel> sumEditFactory)
        {
            Memo = "Content is loading…"; // ToDo Localize
            Sum = new ReactiveProperty<long>(0L, ReactivePropertyMode.DistinctUntilChanged);
            SumEdit = sumEditFactory(new ReactiveProperty<long>());
            Cleared = false;
            ToggleSign = new RxRelayCommand(() => { });
            DeleteCommand = new RxRelayCommand(() => { });
            RemoveCommand = new RxRelayCommand(() => { });
            IsInserted = false;
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
