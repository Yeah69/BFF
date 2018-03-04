using Reactive.Bindings;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface IHaveFlagViewModel
    {
        IReactiveProperty<IFlagViewModel> Flag { get; }
    }
}
