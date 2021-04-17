using BFF.Model.Contexts;
using System;
using BFF.Model.Models;
using BFF.ViewModel.Managers;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels;
using BFF.ViewModel.ViewModels.ForModels;

namespace BFF.ViewModel.Contexts
{
    internal class LoadContextViewModel : ContextViewModel, ILoadContextViewModel
    {
        private readonly Lazy<LoadedProjectTopLevelViewModelComposition> _lazyLoadedProjectTopLevelViewModelComposition;

        public LoadContextViewModel(
            // parameters
            IContext context, 

            // dependencies
            Func<IAccountViewModelService> accountViewModelService,
            Func<ICategoryViewModelService> categoryViewModelService,
            Func<IIncomeCategoryViewModelService> incomeCategoryViewModelService,
            Func<IPayeeViewModelService> payeeViewModelService,
            Func<IFlagViewModelService> flagViewModelService,
            Func<ISummaryAccount> summaryAccount,
            Func<ISummaryAccountViewModel> summaryAccountViewModel,
            IBackendCultureManager cultureManager,
            Lazy<LoadedProjectTopLevelViewModelComposition> lazyLoadedProjectTopLevelViewModelComposition)
        {
            CultureManager = cultureManager;
            _lazyLoadedProjectTopLevelViewModelComposition = lazyLoadedProjectTopLevelViewModelComposition;
            accountViewModelService();
            categoryViewModelService();
            incomeCategoryViewModelService();
            payeeViewModelService();
            flagViewModelService();
            summaryAccount();
            summaryAccountViewModel();

            Title = $"{context.Title} - {base.Title}";
        }

        public override TopLevelViewModelCompositionBase LoadProject()
        {
            return _lazyLoadedProjectTopLevelViewModelComposition.Value;
        }
        
        public override ICultureManager CultureManager { get; }

        public override string Title { get; }
    }
}
