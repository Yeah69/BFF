using System;
using BFF.MVVM.Managers;
using BFF.MVVM.ViewModels;

namespace BFF
{

    public interface IEmptyContext : IBackendContext
    {

    }

    class EmptyContext : BackendContext, IEmptyContext
    {
        public EmptyContext(Lazy<IEmptyCultureManager> lazyCultureManger)
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
