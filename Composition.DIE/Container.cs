using BFF.Model.Contexts;
using BFF.Model.ImportExport;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.Persistence.Import;
using BFF.Persistence.Realm;
using BFF.View.Wpf;
using BFF.ViewModel.Contexts;
using BFF.ViewModel.ViewModels;
using BFF.ViewModel.ViewModels.ForModels;
using BFF.ViewModel.ViewModels.ForModels.Utility;
using MahApps.Metro.Controls.Dialogs;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.ResXToViewModelGenerator;
using System;
using System.Reactive.Disposables;

namespace BFF.Composition.DIE
{
    [ConstructorChoice(typeof(CompositeDisposable))]
    [ContainerInstanceImplementationAggregation(
        typeof(DialogCoordinator), 
        typeof(CurrentTextsViewModel))]
    [ImplementationChoice(typeof(IBudgetMonthViewModel), typeof(BudgetMonthViewModel))]
    [ImplementationChoice(typeof(IBudgetEntryViewModel), typeof(BudgetEntryViewModel))]
    [ImplementationChoice(typeof(ICsvBankStatementImportNonProfileViewModel), typeof(CsvBankStatementImportNonProfileViewModel))]
    [CreateFunction(typeof(App), "Create")]
    internal sealed partial class Container
    {
        private Container() {}

        private Func<IExportConfiguration, IExportContext> DIE_Factory_IExportContextFactory(
            Func<IRealmProjectFileAccessConfiguration, IRealmContext> realmFactory) =>
            ec => ec switch
            {
                IRealmProjectFileAccessConfiguration realmProjectFileAccessConfiguration => realmFactory(
                    realmProjectFileAccessConfiguration),
                _ => throw new Exception()
            };

        private Func<IImportConfiguration, IImportContext> DIE_Factory_IImportContextFactory(
            Func<IYnab4CsvImportConfiguration, IYnab4CsvImporter> ynab4Factory) =>
            ec => ec switch
            {
                IYnab4CsvImportConfiguration ynab4CsvImportConfiguration => ynab4Factory(
                    ynab4CsvImportConfiguration),
                _ => throw new Exception()
            };

        private Func<IContext, ILoadContextViewModel> DIE_Factory_ILoadContextViewModelFactory() =>
            c => c switch
            {
                ILoadContext loadContext when loadContext.ViewModelContext(c) is ILoadContextViewModel loadContextViewModel => loadContextViewModel,
                _ => throw new Exception()
            };

        private sealed partial class DIE_DefaultTransientScope
        {
            private IPayee DIE_Factory_IPayee(ICreateNewModels createNewModels) =>
                createNewModels.CreatePayee();
        } 
    }
}