using System;
using BFF.DB.Dapper.ModelRepositories;
using BFF.DB.SQLite;
using BFF.MVVM.Models.Native;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels;
using BFF.MVVM.ViewModels.ForModels;

namespace BFF
{
    public interface ISqLiteBackendContext : IBackendContext
    {

    }

    class SqLiteBackendContext : BackendContext, ISqLiteBackendContext
    {
        public SqLiteBackendContext(
            string dbPath, 
            Func<string, IProvideSqLiteConnection> provideSqLiteConnectionFactory,
            Func<IAccountRepository> accountRepository,
            Func<ICategoryRepository> categoryRepository,
            Func<IIncomeCategoryRepository> incomeCategoryRepository,
            Func<IPayeeRepository> payeeRepository,
            Func<IFlagRepository> flagRepository,
            Func<IAccountViewModelService> accountViewModelService,
            Func<ICategoryViewModelService> categoryViewModelService,
            Func<ICategoryViewModelInitializer> categoryViewModelInitializer,
            Func<IIncomeCategoryViewModelService> incomeCategoryViewModelService,
            Func<IPayeeViewModelService> payeeViewModelService,
            Func<IFlagViewModelService> flagViewModelService,
            Func<ISummaryAccount> summaryAccount,
            Func<ISummaryAccountViewModel> summaryAccountViewModel,
            Lazy<IBudgetOverviewViewModel> lazyBudgetOverviewViewModel,
            Lazy<IAccountTabsViewModel> lazyAccountTabsViewModel,
            Lazy<IEditAccountsViewModel> lazyEditAccountsViewModel,
            Lazy<IEditCategoriesViewModel> lazyEditCategoriesViewModel)
        {
            provideSqLiteConnectionFactory(dbPath);
            accountRepository();
            categoryRepository();
            incomeCategoryRepository();
            payeeRepository();
            flagRepository();
            accountViewModelService();
            var viewModelService = categoryViewModelService();
            categoryViewModelInitializer().Initialize(viewModelService.All);
            incomeCategoryViewModelService();
            payeeViewModelService();
            flagViewModelService();
            summaryAccount();
            summaryAccountViewModel();
            BudgetOverviewViewModel = lazyBudgetOverviewViewModel.Value;
            AccountTabsViewModel = lazyAccountTabsViewModel.Value;
            EditAccountsViewModel = lazyEditAccountsViewModel.Value;
            EditCategoriesViewModel = lazyEditCategoriesViewModel.Value;
        }

        public override IBudgetOverviewViewModel BudgetOverviewViewModel { get; } 
        public override IAccountTabsViewModel AccountTabsViewModel { get; }
        public override IEditAccountsViewModel EditAccountsViewModel { get; }
        public override IEditCategoriesViewModel EditCategoriesViewModel { get; }
    }
}
