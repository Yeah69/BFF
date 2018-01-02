using System;
using BFF.DB.SQLite;
using BFF.MVVM.ViewModels;

namespace BFF
{
    public interface ISqLiteBackendContext : IBackendContext
    {

    }

    class SqLiteBackendContext : BackendContext, ISqLiteBackendContext
    {
        private readonly Lazy<IBudgetOverviewViewModel> _lazyBudgetOverviewViewModel;
        private readonly Lazy<IAccountTabsViewModel> _lazyAccountTabsViewModel;

        public SqLiteBackendContext(
            string dbPath, 
            Func<string, IProvideSqLiteConnetion> provideSqLiteConnectionFactory,
            Lazy<IBudgetOverviewViewModel> lazyBudgetOverviewViewModel,
            Lazy<IAccountTabsViewModel> lazyAccountTabsViewModel)
        {
            provideSqLiteConnectionFactory(dbPath);
            _lazyBudgetOverviewViewModel = lazyBudgetOverviewViewModel;
            _lazyAccountTabsViewModel = lazyAccountTabsViewModel;
        }

        public override IBudgetOverviewViewModel BudgetOverviewViewModel => _lazyBudgetOverviewViewModel.Value;
        public override IAccountTabsViewModel AccountTabsViewModel => _lazyAccountTabsViewModel.Value;
    }
}
