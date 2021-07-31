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
    [Register(typeof(BFF.View.Wpf.Views.Dialogs.ImportCsvBankStatementView), typeof(BFF.View.Wpf.Views.Dialogs.IImportCsvBankStatementView))]
    [Register(typeof(BFF.View.Wpf.Views.Dialogs.ImportDialog))]
    [Register(typeof(BFF.View.Wpf.Views.Dialogs.MainWindowDialogView))]
    [Register(typeof(BFF.View.Wpf.Views.Dialogs.NewFileAccessDialog), typeof(BFF.View.Wpf.Views.Dialogs.INewFileAccessDialog))]
    [Register(typeof(BFF.View.Wpf.Views.Dialogs.OpenFileAccessDialog), typeof(BFF.View.Wpf.Views.Dialogs.IOpenFileAccessDialog))]
    [Register(typeof(BFF.View.Wpf.Resources.TransCellTemplates))]
    [Register(typeof(BFF.View.Wpf.Managers.MainWindowDialogManager), typeof(BFF.ViewModel.Managers.IMainWindowDialogManager))]
    [Register(typeof(BFF.View.Wpf.Helper.MainDialogCoordinator), typeof(BFF.ViewModel.Helper.IMainBffDialogCoordinator), typeof(BFF.ViewModel.Helper.IBffDialogCoordinator))]
    [Register(typeof(BFF.View.Wpf.Helper.BffOpenFileDialog), typeof(BFF.ViewModel.Helper.IBffOpenFileDialog))]
    [Register(typeof(BFF.View.Wpf.Helper.BffSaveFileDialog), typeof(BFF.ViewModel.Helper.IBffSaveFileDialog))]
    [Register(typeof(BFF.View.Wpf.Helper.BffSettings), typeof(BFF.ViewModel.Helper.IBffSettings))]
    [Register(typeof(BFF.View.Wpf.Helper.BindingProxy))]
    [Register(typeof(BFF.View.Wpf.Helper.EnumerationExtension))]
    [Register(typeof(BFF.View.Wpf.Helper.WpfRxSchedulerProvider), typeof(BFF.Core.Helper.IRxSchedulerProvider))]
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
    [Register(typeof(BFF.ViewModel.ViewModels.AccountTabsViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.IAccountTabsViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.BudgetCategoryViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.IBudgetCategoryViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.BudgetMonthViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.IBudgetMonthViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.BudgetMonthViewModelPlaceholder), typeof(BFF.ViewModel.ViewModels.IBudgetMonthViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.BudgetOverviewTableViewModel), typeof(BFF.ViewModel.ViewModels.IBudgetOverviewTableViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.BudgetOverviewTableRowViewModel), typeof(BFF.ViewModel.ViewModels.IBudgetOverviewTableRowViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.BudgetOverviewViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.IBudgetOverviewViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.EditAccountsViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.IEditAccountsViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.EditCategoriesViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.IEditCategoriesViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.EditFlagsViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.IEditFlagsViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.EditPayeesViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.IEditPayeesViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.MainWindowViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.IMainWindowViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.MonthViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.IMonthViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.NewAccountViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.INotifyingErrorViewModel), typeof(BFF.ViewModel.ViewModels.INewAccountViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.NewCategoryViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.INotifyingErrorViewModel), typeof(BFF.ViewModel.ViewModels.INewCategoryViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.NewFlagViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.INotifyingErrorViewModel), typeof(BFF.ViewModel.ViewModels.INewFlagViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.NewPayeeViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.INotifyingErrorViewModel), typeof(BFF.ViewModel.ViewModels.INewPayeeViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.SumEditViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.ISumEditViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.LoadedProjectTopLevelViewModelComposition))]
    [Register(typeof(BFF.ViewModel.ViewModels.EmptyTopLevelViewModelComposition))]
    [Register(typeof(BFF.ViewModel.ViewModels.Import.ImportDialogViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.Import.IImportDialogViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.Import.RealmFileExportViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.Import.IExportViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.Import.Ynab4CsvImportViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.Import.IImportViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.AccountViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.INotifyingErrorViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.IDataModelViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.ICommonPropertyViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.IAccountBaseViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.IAccountViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.IImportCsvBankStatement))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.BudgetEntryViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.INotifyingErrorViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.IDataModelViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.IBudgetEntryViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.CategoryViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.INotifyingErrorViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.IDataModelViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.ICommonPropertyViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.ICategoryBaseViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.ICategoryViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.FlagViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.INotifyingErrorViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.IDataModelViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.ICommonPropertyViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.IFlagViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.IncomeCategoryViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.INotifyingErrorViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.IDataModelViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.ICommonPropertyViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.ICategoryBaseViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.IIncomeCategoryViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.ParentTransactionViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.INotifyingErrorViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.IDataModelViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.ITransLikeViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.ITransBaseViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.IHaveFlagViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.ITransactionBaseViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.IHavePayeeViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.IParentTransactionViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.PayeeViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.INotifyingErrorViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.IDataModelViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.ICommonPropertyViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.IPayeeViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.SubTransactionViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.INotifyingErrorViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.IDataModelViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.ITransLikeViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.ISubTransactionViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.IHaveCategoryViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.SummaryAccountViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.INotifyingErrorViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.IDataModelViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.ICommonPropertyViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.IAccountBaseViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.ISummaryAccountViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.TransLikeViewModelPlaceholder), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.INotifyingErrorViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.ITransLikeViewModelPlaceholder), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.ITransLikeViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.IDataModelViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.TransactionViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.INotifyingErrorViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.IDataModelViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.ITransLikeViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.ITransBaseViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.IHaveFlagViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.ITransactionBaseViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.IHavePayeeViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.ITransactionViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.IHaveCategoryViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.TransferViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.INotifyingErrorViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.IDataModelViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.ITransLikeViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.ITransBaseViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Structure.IHaveFlagViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.ITransferViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.Utility.BudgetMonthMenuTitles), typeof(BFF.ViewModel.ViewModels.ForModels.Utility.IBudgetMonthMenuTitles))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.Utility.CsvBankStatementImportItemViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Utility.ICsvBankStatementImportItemViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.Utility.CsvBankStatementImportProfileViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Utility.ICsvBankStatementImportProfileViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Utility.ICsvBankStatementImportNonProfileViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.Utility.CsvBankStatementImportNonProfileViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Utility.ICsvBankStatementImportNonProfileViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.ForModels.Utility.LazyTransLikeViewModels), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.ForModels.Utility.ILazyTransLikeViewModels))]
    [Register(typeof(BFF.ViewModel.ViewModels.Dialogs.NewFileAccessViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.Dialogs.IMainWindowDialogContentViewModel), typeof(BFF.ViewModel.ViewModels.Dialogs.IFileAccessViewModel), typeof(BFF.ViewModel.ViewModels.Dialogs.INewFileAccessViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.Dialogs.OpenFileAccessViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.Dialogs.IMainWindowDialogContentViewModel), typeof(BFF.ViewModel.ViewModels.Dialogs.IFileAccessViewModel), typeof(BFF.ViewModel.ViewModels.Dialogs.IOpenFileAccessViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.Dialogs.ImportCsvBankStatementViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.Dialogs.IMainWindowDialogContentViewModel), typeof(BFF.ViewModel.ViewModels.Dialogs.IImportCsvBankStatementViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.Dialogs.MainWindowDialogViewModel), typeof(BFF.ViewModel.ViewModels.Dialogs.IMainWindowDialogViewModel))]
    [Register(typeof(BFF.ViewModel.ViewModels.Dialogs.PasswordProtectedFileAccessViewModel), typeof(BFF.ViewModel.ViewModels.IViewModel), typeof(BFF.ViewModel.ViewModels.Dialogs.IPasswordProtectedFileAccessViewModel))]
    [Register(typeof(BFF.ViewModel.Services.AccountViewModelService), typeof(BFF.ViewModel.Services.IAccountViewModelService))]
    [Register(typeof(BFF.ViewModel.Services.BudgetEntryViewModelService), typeof(BFF.ViewModel.Services.IBudgetEntryViewModelService))]
    [Register(typeof(BFF.ViewModel.Services.CategoryBaseViewModelService), typeof(BFF.ViewModel.Services.ICategoryBaseViewModelService))]
    [Register(typeof(BFF.ViewModel.Services.CategoryViewModelService), typeof(BFF.ViewModel.Services.ICategoryViewModelService))]
    [Register(typeof(BFF.ViewModel.Services.FlagViewModelService), typeof(BFF.ViewModel.Services.IFlagViewModelService))]
    [Register(typeof(BFF.ViewModel.Services.IncomeCategoryViewModelService), typeof(BFF.ViewModel.Services.IIncomeCategoryViewModelService))]
    [Register(typeof(BFF.ViewModel.Services.PayeeViewModelService), typeof(BFF.ViewModel.Services.IPayeeViewModelService))]
    [Register(typeof(BFF.ViewModel.Managers.TransDataGridColumnManager), typeof(BFF.ViewModel.Managers.ITransDataGridColumnManager))]
    [Register(typeof(BFF.ViewModel.Managers.BudgetRefreshes), typeof(BFF.ViewModel.Managers.IBudgetRefreshes))]
    [Register(typeof(BFF.ViewModel.Managers.BackendCultureManager), typeof(BFF.ViewModel.Managers.ICultureManager), typeof(BFF.ViewModel.Managers.IBackendCultureManager))]
    [Register(typeof(BFF.ViewModel.Managers.EmptyCultureManager), typeof(BFF.ViewModel.Managers.ICultureManager), typeof(BFF.ViewModel.Managers.IEmptyCultureManager))]
    [Register(typeof(BFF.ViewModel.Managers.ParentTransactionFlyoutManager), typeof(BFF.ViewModel.Managers.IParentTransactionFlyoutManager))]
    [Register(typeof(BFF.ViewModel.Managers.TransTransformingManager), typeof(BFF.ViewModel.Managers.ITransTransformingManager))]
    [Register(typeof(BFF.ViewModel.Helper.ConvertFromTransBaseToTransLikeViewModel), typeof(BFF.ViewModel.Helper.IConvertFromTransBaseToTransLikeViewModel))]
    [Register(typeof(BFF.ViewModel.Helper.BffSettingsProxy), typeof(BFF.Model.Helper.IBffSettingsProxy))]
    [Register(typeof(BFF.ViewModel.Helper.ManageCsvBankStatementImportProfiles), typeof(BFF.Model.Helper.IManageCsvBankStatementImportProfiles))]
    [Register(typeof(BFF.ViewModel.Contexts.EmptyContextViewModel), typeof(BFF.ViewModel.Contexts.IContextViewModel), typeof(BFF.ViewModel.Contexts.IEmptyContextViewModel))]
    [Register(typeof(BFF.ViewModel.Contexts.LoadContextViewModel), typeof(BFF.ViewModel.Contexts.IContextViewModel), typeof(BFF.ViewModel.Contexts.ILoadContextViewModel))]
    [Register(typeof(BFF.Model.UpdateBudgetCategory), typeof(BFF.Model.IUpdateBudgetCategory), typeof(BFF.Model.IObserveUpdateBudgetCategory))]
    [Register(typeof(BFF.Model.Models.CategoryComparer))]
    [Register(typeof(BFF.Model.Models.Utility.CsvBankStatementImportProfile), typeof(BFF.Model.Models.Utility.ICsvBankStatementImportProfile))]
    [Register(typeof(BFF.Model.Models.Utility.CsvBankStatementProfileManager), typeof(BFF.Model.Models.Utility.ICsvBankStatementProfileManager), typeof(BFF.Model.Models.Utility.ICreateCsvBankStatementImportProfile))]
    [Register(typeof(BFF.Model.IoC.ContextManager), typeof(BFF.Model.IoC.IContextManager))]
    [Register(typeof(BFF.Model.IoC.CurrentProject), typeof(BFF.Model.IoC.ICurrentProject))]
    [Register(typeof(BFF.Model.Import.DtoImportContainer))]
    [Register(typeof(BFF.Model.Import.DtoImportContainerBuilder), typeof(BFF.Model.Import.IDtoImportContainerBuilder))]
    [Register(typeof(BFF.Model.Import.Models.BudgetEntryDto))]
    [Register(typeof(BFF.Model.Import.Models.CategoryDto))]
    [Register(typeof(BFF.Model.Import.Models.ParentTransactionDto))]
    [Register(typeof(BFF.Model.Import.Models.SubTransactionDto))]
    [Register(typeof(BFF.Model.Import.Models.TransactionDto))]
    [Register(typeof(BFF.Model.Import.Models.TransferDto))]
    [Register(typeof(BFF.Model.ImportExport.Ynab4CsvImportConfiguration), typeof(BFF.Model.ImportExport.IYnab4CsvImportConfiguration), typeof(BFF.Model.ImportExport.IImportConfiguration), typeof(BFF.Model.ImportExport.IConfiguration))]
    [Register(typeof(BFF.Model.Helper.LastSetDate), typeof(BFF.Model.Helper.ILastSetDate))]
    [Register(typeof(BFF.Model.Contexts.EmptyContext), typeof(BFF.Model.Contexts.IContext))]
    [Register(typeof(BFF.Persistence.Realm.RealmContext), typeof(BFF.Persistence.Realm.IRealmContext), typeof(BFF.Model.Contexts.IExportContext), typeof(BFF.Model.Contexts.IContext), typeof(BFF.Model.Contexts.IImportContext), typeof(BFF.Model.Contexts.ILoadContext))]
    [Register(typeof(BFF.Persistence.Realm.RealmContextFactory), typeof(BFF.Model.IoC.IContextFactory))]
    [Register(typeof(BFF.Persistence.Realm.RealmExporter), typeof(BFF.Persistence.Realm.IRealmExporter))]
    [Register(typeof(BFF.Persistence.Realm.RealmOperations), typeof(BFF.Persistence.Realm.IRealmOperations))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.RealmBudgetCategoryRepository), typeof(BFF.Model.Repositories.IBudgetCategoryRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.RealmBudgetMonthRepository), typeof(BFF.Model.Repositories.IBudgetMonthRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.AccountComparer))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmAccountRepository), typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.IRealmAccountRepositoryInternal), typeof(BFF.Model.Repositories.IAccountRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmBudgetEntryRepository), typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.IRealmBudgetEntryRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmCategoryBaseRepository), typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.IRealmCategoryBaseRepositoryInternal))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmCategoryRepository), typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.IRealmCategoryRepositoryInternal), typeof(BFF.Model.Repositories.ICategoryRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmDbSettingRepository), typeof(BFF.Model.Repositories.IDbSettingRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.FlagComparer))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmFlagRepository), typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.IRealmFlagRepositoryInternal), typeof(BFF.Model.Repositories.IFlagRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.IncomeCategoryComparer))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmIncomeCategoryRepository), typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.IRealmIncomeCategoryRepositoryInternal), typeof(BFF.Model.Repositories.IIncomeCategoryRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmParentTransactionRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.PayeeComparer))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmPayeeRepository), typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.IRealmPayeeRepositoryInternal), typeof(BFF.Model.Repositories.IPayeeRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmSubTransactionRepository), typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.IRealmSubTransactionRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmTransactionRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmTransferRepository))]
    [Register(typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.RealmTransRepository), typeof(BFF.Persistence.Realm.Repositories.ModelRepositories.IRealmTransRepository))]
    [Register(typeof(BFF.Persistence.Realm.ORM.ProvideConnection), typeof(BFF.Persistence.Realm.ORM.IProvideRealmConnection))]
    [Register(typeof(BFF.Persistence.Realm.ORM.RealmAccountOrm), typeof(BFF.Persistence.Realm.ORM.Interfaces.IAccountOrm))]
    [Register(typeof(BFF.Persistence.Realm.ORM.RealmBudgetOrm), typeof(BFF.Persistence.Realm.ORM.Interfaces.IBudgetOrm))]
    [Register(typeof(BFF.Persistence.Realm.ORM.RealmCategoryOrm), typeof(BFF.Persistence.Realm.ORM.Interfaces.ICategoryOrm))]
    [Register(typeof(BFF.Persistence.Realm.ORM.RealmCreateBackendOrm), typeof(BFF.Persistence.Common.ICreateBackendOrm))]
    [Register(typeof(BFF.Persistence.Realm.ORM.RealmExportingOrm), typeof(BFF.Persistence.Realm.ORM.Interfaces.IExportingOrm))]
    [Register(typeof(BFF.Persistence.Realm.ORM.RealmImporter), typeof(BFF.Persistence.Realm.ORM.IRealmImporter))]
    [Register(typeof(BFF.Persistence.Realm.ORM.RealmMergeOrm), typeof(BFF.Persistence.Realm.ORM.Interfaces.IMergeOrm))]
    [Register(typeof(BFF.Persistence.Realm.ORM.RealmParentalOrm), typeof(BFF.Persistence.Realm.ORM.Interfaces.IParentalOrm))]
    [Register(typeof(BFF.Persistence.Realm.ORM.RealmTransOrm), typeof(BFF.Persistence.Realm.ORM.Interfaces.ITransOrm))]
    [Register(typeof(BFF.Persistence.Realm.Models.RealmCreateNewModels), typeof(BFF.Persistence.Realm.Models.IRealmCreateNewModels), typeof(BFF.Model.Repositories.ICreateNewModels))]
    [Register(typeof(BFF.Persistence.Realm.Models.Persistence.Account))]
    [Register(typeof(BFF.Persistence.Realm.Models.Persistence.BudgetEntry))]
    [Register(typeof(BFF.Persistence.Realm.Models.Persistence.Category))]
    [Register(typeof(BFF.Persistence.Realm.Models.Persistence.DbSetting))]
    [Register(typeof(BFF.Persistence.Realm.Models.Persistence.Flag))]
    [Register(typeof(BFF.Persistence.Realm.Models.Persistence.Payee))]
    [Register(typeof(BFF.Persistence.Realm.Models.Persistence.SubTransaction))]
    [Register(typeof(BFF.Persistence.Realm.Models.Persistence.Trans))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.Account), typeof(BFF.Model.Models.Structure.IDataModel), typeof(BFF.Model.Models.Structure.ICommonProperty), typeof(BFF.Model.Models.IAccount))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.BudgetCategory), typeof(BFF.Model.Models.IBudgetCategory))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.BudgetEntry), typeof(BFF.Model.Models.Structure.IDataModel), typeof(BFF.Model.Models.IBudgetEntry))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.BudgetMonth), typeof(BFF.Model.Models.IBudgetMonth))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.Category), typeof(BFF.Model.Models.Structure.IDataModel), typeof(BFF.Model.Models.Structure.ICommonProperty), typeof(BFF.Model.Models.Structure.ICategoryBase), typeof(BFF.Model.Models.ICategory))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.DbSetting), typeof(BFF.Model.Models.Structure.IDataModel), typeof(BFF.Model.Models.IDbSetting))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.Flag), typeof(BFF.Model.Models.Structure.IDataModel), typeof(BFF.Model.Models.Structure.ICommonProperty), typeof(BFF.Model.Models.IFlag))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.IncomeCategory), typeof(BFF.Model.Models.Structure.IDataModel), typeof(BFF.Model.Models.Structure.ICommonProperty), typeof(BFF.Model.Models.Structure.ICategoryBase), typeof(BFF.Model.Models.IIncomeCategory))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.ParentTransaction), typeof(BFF.Model.Models.Structure.IDataModel), typeof(BFF.Model.Models.Structure.ITransLike), typeof(BFF.Model.Models.Structure.ITransBase), typeof(BFF.Model.Models.Structure.ITransactionBase), typeof(BFF.Model.Models.IParentTransaction))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.Payee), typeof(BFF.Model.Models.Structure.IDataModel), typeof(BFF.Model.Models.Structure.ICommonProperty), typeof(BFF.Model.Models.IPayee))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.SubTransaction), typeof(BFF.Model.Models.Structure.IDataModel), typeof(BFF.Model.Models.Structure.ITransLike), typeof(BFF.Model.Models.ISubTransaction))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.SummaryAccount), typeof(BFF.Model.Models.Structure.IDataModel), typeof(BFF.Model.Models.Structure.ICommonProperty), typeof(BFF.Model.Models.IAccount), typeof(BFF.Model.Models.ISummaryAccount))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.Transaction), typeof(BFF.Model.Models.Structure.IDataModel), typeof(BFF.Model.Models.Structure.ITransLike), typeof(BFF.Model.Models.Structure.ITransBase), typeof(BFF.Model.Models.Structure.ITransactionBase), typeof(BFF.Model.Models.ITransaction))]
    [Register(typeof(BFF.Persistence.Realm.Models.Domain.Transfer), typeof(BFF.Model.Models.Structure.IDataModel), typeof(BFF.Model.Models.Structure.ITransLike), typeof(BFF.Model.Models.Structure.ITransBase), typeof(BFF.Model.Models.ITransfer))]
    [Register(typeof(BFF.Persistence.Import.Ynab4CsvImporter), typeof(BFF.Persistence.Import.IYnab4CsvImporter), typeof(BFF.Model.Contexts.IImportContext), typeof(BFF.Model.Contexts.IContext))]
    [Register(typeof(BFF.Persistence.Import.Ynab4CsvImportContextFactory), typeof(BFF.Model.IoC.IContextFactory))]
    [Register(typeof(BFF.Persistence.Import.Models.YNAB.Helper))]
    [Register(typeof(BFF.Persistence.Import.Models.YNAB.Ynab4BudgetEntry))]
    [Register(typeof(BFF.Persistence.Import.Models.YNAB.Ynab4Transaction))]
    [Register(typeof(BFF.Persistence.Contexts.CreateFileAccessConfiguration), typeof(BFF.Model.ImportExport.ICreateFileAccessConfiguration))]
    [Register(typeof(BFF.Persistence.Contexts.RealmProjectFileAccessConfiguration), typeof(BFF.Model.ImportExport.IRealmProjectFileAccessConfiguration), typeof(BFF.Model.ImportExport.IEncryptedProjectFileAccessConfiguration), typeof(BFF.Model.ImportExport.IProjectFileAccessConfiguration), typeof(BFF.Model.ImportExport.ILoadConfiguration), typeof(BFF.Model.ImportExport.IConfiguration), typeof(BFF.Model.ImportExport.IExportConfiguration))]
    [Register(typeof(BFF.Persistence.Common.BudgetEntryData))]
    [Register(typeof(BFF.Persistence.Common.ClearBudgetCache), typeof(BFF.Model.IClearBudgetCache), typeof(BFF.Persistence.Common.IObserveClearBudgetCache))]
    public partial class MyContainer : IContainer<App>
    {
        [Factory] private CompositeDisposable CreateCompositeDisposable() => new ();
    }//*/
}