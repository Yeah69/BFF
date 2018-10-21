using BFF.Core.Helper;
using BFF.MVVM.Managers;
using BFF.MVVM.ViewModels;

namespace BFF.Contexts
{
    public interface IProjectContext
    {
        IBudgetOverviewViewModel BudgetOverviewViewModel { get; }

        IAccountTabsViewModel AccountTabsViewModel { get; }

        IEditAccountsViewModel EditAccountsViewModel { get; }

        IEditCategoriesViewModel EditCategoriesViewModel { get; }

        IEditPayeesViewModel EditPayeesViewModel { get; }

        IEditFlagsViewModel EditFlagsViewModel { get; }

        ICultureManager CultureManager { get; }
    }

    public abstract class ProjectContext : ObservableObject, IProjectContext
    {
        public abstract IBudgetOverviewViewModel BudgetOverviewViewModel { get; }

        public abstract IAccountTabsViewModel AccountTabsViewModel { get; }

        public abstract IEditAccountsViewModel EditAccountsViewModel { get; }

        public abstract IEditCategoriesViewModel EditCategoriesViewModel { get; }

        public abstract IEditPayeesViewModel EditPayeesViewModel { get; }

        public abstract IEditFlagsViewModel EditFlagsViewModel { get; }

        public abstract ICultureManager CultureManager { get; }
    }
}
