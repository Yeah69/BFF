using BFF.ViewModel.Managers;
using BFF.ViewModel.ViewModels;
using MrMeeseeks.Windows;

namespace BFF.ViewModel.Contexts
{
    public interface IContextViewModel
    {
        ICultureManager CultureManager { get; }
        string Title { get; }
        TopLevelViewModelCompositionBase LoadProject();
    }

    internal abstract class ContextViewModel : ObservableObject, IContextViewModel
    {
        public abstract ICultureManager CultureManager { get; }
        public virtual string Title { get; } = "BFF";
        public abstract TopLevelViewModelCompositionBase LoadProject();
    }
}
