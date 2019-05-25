using System;
using BFF.Model.Models;
using BFF.ViewModel.Managers;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels;
using BFF.ViewModel.ViewModels.ForModels;

namespace BFF.ViewModel.Contexts
{
    internal class LoadProjectContext : ProjectContext, ILoadProjectContext
    {
        private readonly IDisposable _disposeContext;
        private readonly Lazy<LoadedProjectTopLevelViewModelComposition> _lazyLoadedProjectTopLevelViewModelComposition;

        public LoadProjectContext(
            // parameters
            IDisposable disposeContext,

            // dependencies
            Func<IAccountViewModelService> accountViewModelService,
            Func<ICategoryViewModelService> categoryViewModelService,
            Func<ICategoryViewModelInitializer> categoryViewModelInitializer,
            Func<IIncomeCategoryViewModelService> incomeCategoryViewModelService,
            Func<IPayeeViewModelService> payeeViewModelService,
            Func<IFlagViewModelService> flagViewModelService,
            Func<ISummaryAccount> summaryAccount,
            Func<ISummaryAccountViewModel> summaryAccountViewModel,
            IBackendCultureManager cultureManager,
            Lazy<LoadedProjectTopLevelViewModelComposition> lazyLoadedProjectTopLevelViewModelComposition)
        {
            CultureManager = cultureManager;
            _disposeContext = disposeContext;
            _lazyLoadedProjectTopLevelViewModelComposition = lazyLoadedProjectTopLevelViewModelComposition;
            accountViewModelService();
            var viewModelService = categoryViewModelService();
            viewModelService.AllCollectionInitialized.ContinueWith(_ => categoryViewModelInitializer().Initialize(viewModelService.All));
            incomeCategoryViewModelService();
            payeeViewModelService();
            flagViewModelService();
            summaryAccount();
            summaryAccountViewModel();
        }

        public override TopLevelViewModelCompositionBase LoadProject()
        {
            return _lazyLoadedProjectTopLevelViewModelComposition.Value;
        }

        public void Dispose()
        {
            _disposeContext?.Dispose();
        }

        public override ICultureManager CultureManager { get; }
    }
}
