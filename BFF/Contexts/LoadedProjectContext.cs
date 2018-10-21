using System;
using BFF.Core.Persistence;
using BFF.Model.Contexts;
using BFF.Model.Models;
using BFF.Model.Repositories.ModelRepositories;
using BFF.MVVM.Managers;
using BFF.MVVM.Services;
using BFF.MVVM.ViewModels;
using BFF.MVVM.ViewModels.ForModels;

namespace BFF.Contexts
{
    public interface ILoadedProjectContext : IProjectContext
    {

    }

    class LoadedProjectContext : ProjectContext, ILoadedProjectContext
    {
        public LoadedProjectContext(
            IPersistenceConfiguration persistenceConfiguration,
            Func<IPersistenceConfiguration, IModelContext> modelContextFactory, 
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
            Lazy<IEditCategoriesViewModel> lazyEditCategoriesViewModel,
            Lazy<IEditPayeesViewModel> lazyEditPayeesViewModel,
            Lazy<IEditFlagsViewModel> lazyEditFlagsViewModel,
            Lazy<IBackendCultureManager> lazyCultureManger)
        {
            modelContextFactory(persistenceConfiguration);
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
            EditPayeesViewModel = lazyEditPayeesViewModel.Value;
            EditFlagsViewModel = lazyEditFlagsViewModel.Value;
            CultureManager = lazyCultureManger.Value;
        }

        public override IBudgetOverviewViewModel BudgetOverviewViewModel { get; } 
        public override IAccountTabsViewModel AccountTabsViewModel { get; }
        public override IEditAccountsViewModel EditAccountsViewModel { get; }
        public override IEditCategoriesViewModel EditCategoriesViewModel { get; }
        public override IEditPayeesViewModel EditPayeesViewModel { get; }
        public override IEditFlagsViewModel EditFlagsViewModel { get; }
        public override ICultureManager CultureManager { get; }
    }
}
