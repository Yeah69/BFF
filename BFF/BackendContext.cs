using BFF.MVVM;
using BFF.MVVM.ViewModels;

namespace BFF
{
    public interface IBackendContext
    {
        IBudgetOverviewViewModel BudgetOverviewViewModel { get; }

        IAccountTabsViewModel AccountTabsViewModel { get; }

        IEditAccountsViewModel EditAccountsViewModel { get; }

        IEditCategoriesViewModel EditCategoriesViewModel { get; }
    }

    public abstract class BackendContext : ObservableObject, IBackendContext
    {
        public abstract IBudgetOverviewViewModel BudgetOverviewViewModel { get; }

        public abstract IAccountTabsViewModel AccountTabsViewModel { get; }

        public abstract IEditAccountsViewModel EditAccountsViewModel { get; }

        public abstract IEditCategoriesViewModel EditCategoriesViewModel { get; }
    }
}
