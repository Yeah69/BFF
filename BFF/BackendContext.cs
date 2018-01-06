using BFF.MVVM;
using BFF.MVVM.ViewModels;

namespace BFF
{
    public interface IBackendContext
    {
        string Text { get; }

        IBudgetOverviewViewModel BudgetOverviewViewModel { get; }
        IAccountTabsViewModel AccountTabsViewModel { get; }
    }

    public abstract class BackendContext : ObservableObject, IBackendContext
    {
        public string Text => "Hello, World!";

        public abstract IBudgetOverviewViewModel BudgetOverviewViewModel { get; }
        public abstract IAccountTabsViewModel AccountTabsViewModel { get; }
    }
}
