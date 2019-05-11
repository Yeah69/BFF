using System;
using BFF.Core.IoC;
using BFF.ViewModel.Managers;
using BFF.ViewModel.ViewModels;

namespace BFF.ViewModel.Contexts
{
    internal class EmptyProjectContext : ProjectContext, IEmptyProjectContext
    {
        private readonly IDisposable _disposeContext;

        public EmptyProjectContext(
            // parameters
            IDisposable disposeContext,

            // dependencies
            Lazy<IEmptyCultureManager> lazyCultureManger)
        {
            _disposeContext = disposeContext;
            CultureManager = lazyCultureManger.Value;
        }

        public override IBudgetOverviewViewModel BudgetOverviewViewModel => null;
        public override IAccountTabsViewModel AccountTabsViewModel => null;
        public override IEditAccountsViewModel EditAccountsViewModel => null;
        public override IEditCategoriesViewModel EditCategoriesViewModel => null;
        public override IEditPayeesViewModel EditPayeesViewModel => null;
        public override IEditFlagsViewModel EditFlagsViewModel => null;
        public override ICultureManager CultureManager { get; }

        public void Dispose()
        {
            _disposeContext?.Dispose();
        }
    }
}
