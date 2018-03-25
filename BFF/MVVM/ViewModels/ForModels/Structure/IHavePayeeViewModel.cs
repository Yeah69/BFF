using Reactive.Bindings;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface IHavePayeeViewModel
    {
        IReactiveProperty<IPayeeViewModel> Payee { get; }

        INewPayeeViewModel NewPayeeViewModel { get; }
    }
}
