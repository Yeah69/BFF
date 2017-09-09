using Reactive.Bindings;

namespace BFF.MVVM.ViewModels.ForModels.Structure
{
    public interface IHaveCategoryViewModel
    {
        /// <summary>
        /// Each SubTransaction or SubIncome can be budgeted to a category.
        /// </summary>
        IReactiveProperty<ICategoryViewModel> Category { get; }
    }
}
