using Reactive.Bindings;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface IHaveCategoryViewModel
    {
        /// <summary>
        /// Each SubTransaction can be budgeted to a category.
        /// </summary>
        IReactiveProperty<ICategoryBaseViewModel> Category { get; }
    }
}
