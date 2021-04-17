namespace BFF.ViewModel.ViewModels
{
    public abstract class TopLevelViewModelCompositionBase
    {
        public abstract IBudgetOverviewViewModel? BudgetOverviewViewModel { get; }
        public abstract IAccountTabsViewModel? AccountTabsViewModel { get; }
        public abstract IEditAccountsViewModel? EditAccountsViewModel { get; }
        public abstract IEditCategoriesViewModel? EditCategoriesViewModel { get; }
        public abstract IEditPayeesViewModel? EditPayeesViewModel { get; }
        public abstract IEditFlagsViewModel? EditFlagsViewModel { get; }
    }

    public class LoadedProjectTopLevelViewModelComposition : TopLevelViewModelCompositionBase
    {
        public LoadedProjectTopLevelViewModelComposition(
            IBudgetOverviewViewModel budgetOverviewViewModel, 
            IAccountTabsViewModel accountTabsViewModel,
            IEditAccountsViewModel editAccountsViewModel, 
            IEditCategoriesViewModel editCategoriesViewModel, 
            IEditPayeesViewModel editPayeesViewModel, 
            IEditFlagsViewModel editFlagsViewModel)
        {
            BudgetOverviewViewModel = budgetOverviewViewModel;
            AccountTabsViewModel = accountTabsViewModel;
            EditAccountsViewModel = editAccountsViewModel;
            EditCategoriesViewModel = editCategoriesViewModel;
            EditPayeesViewModel = editPayeesViewModel;
            EditFlagsViewModel = editFlagsViewModel;
        }

        public override IBudgetOverviewViewModel? BudgetOverviewViewModel { get; }
        public override IAccountTabsViewModel? AccountTabsViewModel { get; }
        public override IEditAccountsViewModel? EditAccountsViewModel { get; }
        public override IEditCategoriesViewModel? EditCategoriesViewModel { get; }
        public override IEditPayeesViewModel? EditPayeesViewModel { get; }
        public override IEditFlagsViewModel? EditFlagsViewModel { get; }
    }

    public class EmptyTopLevelViewModelComposition : TopLevelViewModelCompositionBase
    {
        public override IBudgetOverviewViewModel? BudgetOverviewViewModel { get; } = null;
        public override IAccountTabsViewModel? AccountTabsViewModel { get; } = null;
        public override IEditAccountsViewModel? EditAccountsViewModel { get; } = null;
        public override IEditCategoriesViewModel? EditCategoriesViewModel { get; } = null;
        public override IEditPayeesViewModel? EditPayeesViewModel { get; } = null;
        public override IEditFlagsViewModel? EditFlagsViewModel { get; } = null;
    }
}
