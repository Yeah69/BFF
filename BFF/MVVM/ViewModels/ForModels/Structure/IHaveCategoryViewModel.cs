using Reactive.Bindings;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface IHaveCategoryViewModel
    {
        IReactiveProperty<ICategoryBaseViewModel> Category { get; }

        INewCategoryViewModel NewCategoryViewModel { get; }
    }
}
