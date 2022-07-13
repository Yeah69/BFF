using BFF.Model.Contexts;
using BFF.Model.ImportExport;
using BFF.Model.IoC;
using BFF.Model.Models;
using BFF.Model.Repositories;
using BFF.View.Wpf;
using BFF.ViewModel.ViewModels;
using BFF.ViewModel.ViewModels.ForModels;
using BFF.ViewModel.ViewModels.ForModels.Utility;
using MahApps.Metro.Controls.Dialogs;
using MrMeeseeks.DIE.Configuration.Attributes;
using MrMeeseeks.ResXToViewModelGenerator;
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
        [CustomConstructorParameterChoice(typeof(TransactionViewModel))]
        private void DIE_ConstrParam_TransactionViewModel(
            ICreateNewModels createNewModels,
            out ITransaction transaction) => 
            transaction = createNewModels.CreateTransaction();
        [CustomConstructorParameterChoice(typeof(TransferViewModel))]
        private void DIE_ConstrParam_TransferViewModel(
            ICreateNewModels createNewModels,
            out ITransfer transfer) => 
            transfer = createNewModels.CreateTransfer();
        [CustomConstructorParameterChoice(typeof(ParentTransactionViewModel))]
        private void DIE_ConstrParam_ParentTransactionViewModel(
            ICreateNewModels createNewModels,
            out IParentTransaction parentTransaction) => 
            parentTransaction = createNewModels.CreateParentTransaction();
        private IPayee DIE_Factory_IPayee(ICreateNewModels createNewModels) =>
            createNewModels.CreatePayee();
        private IDialogCoordinator DIE_Factory_IDialogCoordinator => DialogCoordinator.Instance;
        private IImportContext DIE_Factory_IImportContext(IImportConfiguration configuration, IContextManager contextManager) =>
            contextManager.CreateImportContext(configuration);
    }
}