using BFF.Core.Helper;
using BFF.Model.Contexts;
using BFF.Model.Helper;
using BFF.Model.ImportExport;
using BFF.Model.IoC;
using BFF.Model.Repositories;
using BFF.Persistence.Contexts;
using BFF.Persistence.Realm;
using BFF.Persistence.Realm.ORM;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;
using BFF.View.Wpf;
using BFF.View.Wpf.Helper;
using BFF.View.Wpf.Managers;
using BFF.View.Wpf.Views;
using BFF.View.Wpf.Views.Dialogs;
using BFF.ViewModel.Contexts;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Managers;
using BFF.ViewModel.Services;
using BFF.ViewModel.ViewModels;
using BFF.ViewModel.ViewModels.Dialogs;
using BFF.ViewModel.ViewModels.ForModels;
using BFF.ViewModel.ViewModels.Import;
using MrMeeseeks.ResXToViewModelGenerator;
using StrongInject;
using StrongInject.Modules;
using System;
using System.Reactive.Disposables;

namespace BFF.Composition.Wpf.Program
{
    public interface IRoot
    {
        (App, IDisposable) WpfComposition();
    }

    public static class Root
    {
        public static IRoot Create() => new RootInner();
    }

    internal class RootInner : IRoot
    {
        public (App, IDisposable) WpfComposition() => AutofacModule.ResolveWpfApp();

    }

    ///*
    [RegisterModule(typeof(LazyModule))]
    [Register(typeof(BFF.Properties.Settings))]
    [Register(typeof(BFF.View.Wpf.App))]
    [Register(typeof(BFF.View.Wpf.Views.AccountTabsView))]
    [Register(typeof(BFF.View.Wpf.Views.AccountView))]
    [Register(typeof(BFF.View.Wpf.Views.AlertSymbol))]
    [Register(typeof(BFF.View.Wpf.Views.BudgetOverviewView))]
    [Register(typeof(BFF.View.Wpf.Views.BudgetMonthItemDataTemplateSelector))]
    [Register(typeof(BFF.View.Wpf.Views.ColorPickerView))]
    [Register(typeof(BFF.View.Wpf.Views.EditAccountsView))]
    [Register(typeof(BFF.View.Wpf.Views.EditCategoriesView))]
    [Register(typeof(BFF.View.Wpf.Views.EditFlagsView))]
    [Register(typeof(BFF.View.Wpf.Views.EditPayeesView))]
    [Register(typeof(BFF.View.Wpf.Views.MainWindow))]
    [Register(typeof(BFF.View.Wpf.Views.ThemeWrap))]
    [Register(typeof(BFF.View.Wpf.Views.NewAccountView))]
    [Register(typeof(BFF.View.Wpf.Views.NewCategoryView))]
    [Register(typeof(BFF.View.Wpf.Views.NewFlagView))]
    [Register(typeof(BFF.View.Wpf.Views.NewPayeeView))]
    [Register(typeof(BFF.View.Wpf.Views.ParentTransactionView))]
    [Register(typeof(BFF.View.Wpf.Views.StatusCheckMark))]
    [Register(typeof(BFF.View.Wpf.Views.TransDataGrid))]
    [Register(typeof(BFF.View.Wpf.Views.Dialogs.ImportCsvBankStatementView))]
    [Register(typeof(BFF.View.Wpf.Views.Dialogs.ImportDialog))]
    [Register(typeof(BFF.View.Wpf.Views.Dialogs.MainWindowDialogView))]
    [Register(typeof(BFF.View.Wpf.Views.Dialogs.NewFileAccessDialog))]
    [Register(typeof(BFF.View.Wpf.Views.Dialogs.OpenFileAccessDialog))]
    [Register(typeof(BFF.View.Wpf.Resources.TransCellTemplates))]
    [Register(typeof(BFF.View.Wpf.Managers.MainWindowDialogManager))]
    [Register(typeof(BFF.View.Wpf.Helper.MainDialogCoordinator))]
    [Register(typeof(BFF.View.Wpf.Helper.BffOpenFileDialog))]
    [Register(typeof(BFF.View.Wpf.Helper.BffSaveFileDialog))]
    [Register(typeof(BFF.View.Wpf.Helper.BffSettings))]
    [Register(typeof(BFF.View.Wpf.Helper.BindingProxy))]
    [Register(typeof(BFF.View.Wpf.Helper.EnumerationExtension))]
    [Register(typeof(BFF.View.Wpf.Helper.WpfRxSchedulerProvider))]
    [Register(typeof(BFF.View.Wpf.DropHandler.MergingCategoryViewModelsDropHandler))]
    [Register(typeof(BFF.View.Wpf.DropHandler.MergingFlagViewModelsDropHandler))]
    [Register(typeof(BFF.View.Wpf.DropHandler.MergingIncomeCategoryViewModelsDropHandler))]
    [Register(typeof(BFF.View.Wpf.DropHandler.MergingPayeeViewModelsDropHandler))]
    [Register(typeof(BFF.View.Wpf.AttachedBehaviors.AdditionalTabItemsBehavior))]
    [Register(typeof(BFF.View.Wpf.AttachedBehaviors.DynamicTableDataGridBehavior))]
    [Register(typeof(BFF.View.Wpf.AttachedBehaviors.FrameworkElementClickBehavior))]
    [Register(typeof(BFF.View.Wpf.AttachedBehaviors.PopupCommands))]
    [Register(typeof(BFF.View.Wpf.AttachedBehaviors.ScrollViewerSync))]
    [Register(typeof(BFF.View.Wpf.AttachedBehaviors.ToolTipCommands))]
    [Register(typeof(BFF.ViewModel.ViewModels.AccountTabsViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.BudgetCategoryViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.BudgetMonthViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.BudgetMonthViewModelPlaceholder))]
    [Register(typeof(BFF.ViewModel.ViewModels.BudgetOverviewTableViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.BudgetOverviewTableRowViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.BudgetOverviewViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.EditAccountsViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.EditCategoriesViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.EditFlagsViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.EditPayeesViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.MainWindowViewModel), typeof(IMainWindowViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.MonthViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.NewAccountViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.NewCategoryViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.NewFlagViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.NewPayeeViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.SumEditViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.LoadedProjectTopLevelViewModelComposition))]
    [Register(typeof(BFF.ViewModel.ViewModels.EmptyTopLevelViewModelComposition))]
    [Register(typeof(BFF.ViewModel.ViewModels.Import.ImportDialogViewModel), typeof(IImportDialogViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.Import.RealmFileExportViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.Import.Ynab4CsvImportViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.AccountViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.BudgetEntryViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.CategoryViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.FlagViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.IncomeCategoryViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.ParentTransactionViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.PayeeViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.SubTransactionViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.SummaryAccountViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.TransLikeViewModelPlaceholder))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.TransactionViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.TransferViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.Utility.BudgetMonthMenuTitles))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.Utility.CsvBankStatementImportItemViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.Utility.CsvBankStatementImportProfileViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.Utility.CsvBankStatementImportNonProfileViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.Utility.LazyTransLikeViewModels))]
    [Register(typeof(BFF.ViewModel.ViewModels.Dialogs.NewFileAccessViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.Dialogs.OpenFileAccessViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.Dialogs.ImportCsvBankStatementViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.Dialogs.MainWindowDialogViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.Dialogs.PasswordProtectedFileAccessViewModel))]
    [Register(typeof(BFF.ViewModel.Services.AccountViewModelService))]
    [Register(typeof(BFF.ViewModel.Services.BudgetEntryViewModelService))]
    [Register(typeof(BFF.ViewModel.Services.CategoryBaseViewModelService))]
    [Register(typeof(BFF.ViewModel.Services.CategoryViewModelService))]
    [Register(typeof(BFF.ViewModel.Services.FlagViewModelService))]
    [Register(typeof(BFF.ViewModel.Services.IncomeCategoryViewModelService))]
    [Register(typeof(BFF.ViewModel.Services.PayeeViewModelService))]
    [Register(typeof(BFF.ViewModel.Managers.TransDataGridColumnManager))]
    [Register(typeof(BFF.ViewModel.Managers.BudgetRefreshes))]
    [Register(typeof(BFF.ViewModel.Managers.BackendCultureManager))]
    [Register(typeof(BFF.ViewModel.Managers.EmptyCultureManager))]
    [Register(typeof(BFF.ViewModel.Managers.ParentTransactionFlyoutManager))]
    [Register(typeof(BFF.ViewModel.Managers.TransTransformingManager))]
    [Register(typeof(BFF.ViewModel.Helper.ConvertFromTransBaseToTransLikeViewModel))]
    [Register(typeof(BFF.ViewModel.Helper.BffSettingsProxy))]
    [Register(typeof(BFF.ViewModel.Helper.ManageCsvBankStatementImportProfiles))]
    [Register(typeof(BFF.ViewModel.Contexts.EmptyContextViewModel))]
    [Register(typeof(BFF.ViewModel.Contexts.LoadContextViewModel))]
    [Register(typeof(CurrentTextsViewModel), typeof(ICurrentTextsViewModel))]
    [Register(typeof(BFF.Model.UpdateBudgetCategory))]
    [Register(typeof(BFF.Model.Models.CategoryComparer))]
    [Register(typeof(BFF.Model.Models.Utility.CsvBankStatementImportProfile))]
    [Register(typeof(BFF.Model.Models.Utility.CsvBankStatementProfileManager))]
    [Register(typeof(BFF.Model.IoC.ContextManager))]
    [Register(typeof(BFF.Model.IoC.CurrentProject))]
    [Register(typeof(BFF.Model.Import.DtoImportContainer))]
    [Register(typeof(BFF.Model.Import.DtoImportContainerBuilder))]
    [Register(typeof(BFF.Model.Import.Models.BudgetEntryDto))]
    [Register(typeof(BFF.Model.Import.Models.CategoryDto))]
    [Register(typeof(BFF.Model.Import.Models.ParentTransactionDto))]
    [Register(typeof(BFF.Model.Import.Models.SubTransactionDto))]
    [Register(typeof(BFF.Model.Import.Models.TransactionDto))]
    [Register(typeof(BFF.Model.Import.Models.TransferDto))]
    [Register(typeof(BFF.Model.ImportExport.Ynab4CsvImportConfiguration))]
    [Register(typeof(BFF.Model.Helper.LastSetDate))]
    [Register(typeof(BFF.Model.Contexts.EmptyContext))]
    [Register(typeof(BFF.Persistence.Realm.RealmContext))]
    [Register(typeof(BFF.Persistence.Realm.RealmContextFactory))]
    [Register(typeof(BFF.Persistence.Realm.RealmExporter))]
    [Register(typeof(BFF.Persistence.Realm.RealmOperations))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.RealmBudgetCategoryRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.RealmBudgetMonthRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.AccountComparer))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmAccountRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmBudgetEntryRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmCategoryBaseRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmCategoryRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmDbSettingRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.FlagComparer))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmFlagRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.IncomeCategoryComparer))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmIncomeCategoryRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmParentTransactionRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.PayeeComparer))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmPayeeRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmSubTransactionRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmTransactionRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmTransferRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmTransRepository))]
    [Register(typeof(BFF.Persistence.Realm.ORM.ProvideConnection))]
    [Register(typeof(BFF.Persistence.Realm.ORM.RealmAccountOrm))]
    [Register(typeof(BFF.Persistence.Realm.ORM.RealmBudgetOrm))]
    [Register(typeof(BFF.Persistence.Realm.ORM.RealmCategoryOrm))]
    [Register(typeof(BFF.Persistence.Realm.ORM.RealmCreateBackendOrm))]
    [Register(typeof(BFF.Persistence.Realm.ORM.RealmExportingOrm))]
    [Register(typeof(BFF.Persistence.Realm.ORM.RealmImporter))]
    [Register(typeof(BFF.Persistence.Realm.ORM.RealmMergeOrm))]
    [Register(typeof(BFF.Persistence.Realm.ORM.RealmParentalOrm))]
    [Register(typeof(BFF.Persistence.Realm.ORM.RealmTransOrm))]
    [Register(typeof(BFF.Persistence.Realm.Models.RealmCreateNewModels))]
    [Register(typeof(BFF.Persistence.Realm.Models.Persistence.Account))]
    [Register(typeof(BFF.Persistence.Realm.Models.Persistence.BudgetEntry))]
    [Register(typeof(BFF.Persistence.Realm.Models.Persistence.Category))]
    [Register(typeof(BFF.Persistence.Realm.Models.Persistence.DbSetting))]
    [Register(typeof(BFF.Persistence.Realm.Models.Persistence.Flag))]
    [Register(typeof(BFF.Persistence.Realm.Models.Persistence.Payee))]
    [Register(typeof(BFF.Persistence.Realm.Models.Persistence.SubTransaction))]
    [Register(typeof(BFF.Persistence.Realm.Models.Persistence.Trans))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.Account))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.BudgetCategory))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.BudgetEntry))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.BudgetMonth))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.Category))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.DbSetting))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.Flag))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.IncomeCategory))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.ParentTransaction))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.Payee))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.SubTransaction))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.SummaryAccount))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.Transaction))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.Transfer))]
    [Register(typeof(BFF.Persistence.Import.Ynab4CsvImporter))]
    [Register(typeof(BFF.Persistence.Import.Ynab4CsvImportContextFactory))]
    [Register(typeof(BFF.Persistence.Import.Models.YNAB.Helper))]
    [Register(typeof(BFF.Persistence.Import.Models.YNAB.Ynab4BudgetEntry))]
    [Register(typeof(BFF.Persistence.Import.Models.YNAB.Ynab4Transaction))]
    [Register(typeof(BFF.Persistence.Contexts.CreateFileAccessConfiguration))]
    [Register(typeof(BFF.Persistence.Contexts.RealmProjectFileAccessConfiguration))]
    [Register(typeof(BFF.Persistence.Common.BudgetEntryData))]
    [Register(typeof(BFF.Persistence.Common.ClearBudgetCache))]
    public partial class MyContainer : IContainer<App>
    {
        [Factory] private CompositeDisposable CreateCompositeDisposable() => new ();
    }//*/
}