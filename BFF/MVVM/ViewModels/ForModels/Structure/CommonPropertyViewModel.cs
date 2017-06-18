using BFF.DB;
using BFF.MVVM.Models.Native.Structure;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface ICommonPropertyViewModel : IDataModelViewModel
    {
        ReactiveProperty<string> Name { get; set; }
    }

    public abstract class CommonPropertyViewModel : DataModelViewModel, ICommonPropertyViewModel
    {
        protected CommonPropertyViewModel(IBffOrm orm, ICommonProperty commonProperty) : base(orm, commonProperty)
        {
            Name = commonProperty.ToReactivePropertyAsSynchronized(cp => cp.Name).AddTo(CompositeDisposable);
        }
        public virtual ReactiveProperty<string> Name { get; set; }
    }
}