using System;
using BFF.MVVM.Managers;
using BFF.MVVM.ViewModels;

namespace BFF.Contexts
{

    public interface IEmptyProjectContext : IProjectContext
    {

    }

    class EmptyProjectContext : ProjectContext, IEmptyProjectContext
    {
        public EmptyProjectContext(Lazy<IEmptyCultureManager> lazyCultureManger)
        {
            CultureManager = lazyCultureManger.Value;
        }

        public override IBudgetOverviewViewModel BudgetOverviewViewModel => null;
        public override IAccountTabsViewModel AccountTabsViewModel => null;
        public override IEditAccountsViewModel EditAccountsViewModel => null;
        public override IEditCategoriesViewModel EditCategoriesViewModel => null;
        public override IEditPayeesViewModel EditPayeesViewModel => null;
        public override IEditFlagsViewModel EditFlagsViewModel => null;
        public override ICultureManager CultureManager { get; }
    }
}
