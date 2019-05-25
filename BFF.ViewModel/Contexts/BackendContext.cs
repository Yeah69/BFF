using BFF.Core.Helper;
using BFF.ViewModel.Managers;
using BFF.ViewModel.ViewModels;

namespace BFF.ViewModel.Contexts
{
    public interface IProjectContext
    {
        ICultureManager CultureManager { get; }
        TopLevelViewModelCompositionBase LoadProject();
    }

    internal abstract class ProjectContext : ObservableObject, IProjectContext
    {
        public abstract ICultureManager CultureManager { get; }
        public abstract TopLevelViewModelCompositionBase LoadProject();
    }
}
