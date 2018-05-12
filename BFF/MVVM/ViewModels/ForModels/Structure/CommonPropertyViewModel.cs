using BFF.Helper;
using BFF.MVVM.Models.Native.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface ICommonPropertyViewModel : IDataModelViewModel
    {
        IReactiveProperty<string> Name { get; }
    }

    public abstract class CommonPropertyViewModel : DataModelViewModel, ICommonPropertyViewModel
    {
        protected CommonPropertyViewModel(
            ICommonProperty commonProperty,
            IRxSchedulerProvider schedulerProvider) : base(commonProperty, schedulerProvider)
        {
            Name = commonProperty.ToReactivePropertyAsSynchronized(cp => cp.Name, ReactivePropertyMode.DistinctUntilChanged).AddTo(CompositeDisposable);
        }
        public virtual IReactiveProperty<string> Name { get; }
        public override string ToString() => Name.Value;
    }
}