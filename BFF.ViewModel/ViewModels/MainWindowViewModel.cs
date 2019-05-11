using System;
using System.Globalization;
using System.IO;
using System.Reactive.Disposables;
using System.Threading;
using BFF.Core.Helper;
using BFF.Core.IoC;
using BFF.ViewModel.Contexts;
using BFF.ViewModel.Helper;
using BFF.ViewModel.Managers;
using BFF.ViewModel.ViewModels.ForModels;
using NLog;
using Reactive.Bindings;

namespace BFF.ViewModel.ViewModels
{
    public interface IMainWindowViewModel : IViewModel
    {
        IRxRelayCommand NewBudgetPlanCommand { get; }
        IRxRelayCommand OpenBudgetPlanCommand { get; }
        IRxRelayCommand CloseBudgetPlanCommand { get; }
        IRxRelayCommand ParentTransactionOnClose { get; }
        IAccountTabsViewModel AccountTabsViewModel { get; set; }
        IBudgetOverviewViewModel BudgetOverviewViewModel { get; set; }
        IEditAccountsViewModel EditAccountsViewModel { get; }
        IEditCategoriesViewModel EditCategoriesViewModel { get; }
        IEditPayeesViewModel EditPayeesViewModel { get; }
        IEditFlagsViewModel EditFlagsViewModel { get; }
        IEmptyViewModel EmptyViewModel { get; }
        ICultureManager CultureManager { get; }
        bool IsEmpty { get; }
        CultureInfo LanguageCulture { get; set; }
        IReadOnlyReactiveProperty<IParentTransactionViewModel> OpenParentTransaction { get; }
        bool ParentTransFlyoutOpen { get; set; }
        double Width { get; set; }
        double Height { get; set; }
        double X { get; set; }
        double Y { get; set; }
        BffWindowState WindowState { get; set; }
        string Title { get; }
        ITransDataGridColumnManager TransDataGridColumnManager { get; }
    }

    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel, IOncePerApplication //todo IDisposable
    {
        private readonly Func<string, ILoadProjectContext> _loadedProjectContextFactory;
        private readonly Func<IEmptyProjectContext> _emptyContextFactory;
        private readonly IBffSettings _bffSettings;
        private readonly ISetupLocalizationFramework _setupLocalizationFramework;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        protected bool FileFlyoutIsOpen;
        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public IRxRelayCommand NewBudgetPlanCommand { get; }

        public IRxRelayCommand OpenBudgetPlanCommand { get; }

        public IRxRelayCommand CloseBudgetPlanCommand { get; }

        public IRxRelayCommand ParentTransactionOnClose { get; }

        private IAccountTabsViewModel _accountTabsViewModel;
        public IAccountTabsViewModel AccountTabsViewModel
        {
            get => _accountTabsViewModel;
            set
            {
                if (_accountTabsViewModel == value) return;
                _accountTabsViewModel = value;
                OnPropertyChanged();
            }
        }

        public IBudgetOverviewViewModel BudgetOverviewViewModel
        {
            get => _budgetOverviewViewModel;
            set
            {
                if (value == _budgetOverviewViewModel)
                    return;
                _budgetOverviewViewModel = value;
                OnPropertyChanged();
            }
        }

        public IEditAccountsViewModel EditAccountsViewModel
        {
            get => _editAccountsViewModel;
            private set
            {
                if (value == _editAccountsViewModel) return;
                _editAccountsViewModel = value;
                OnPropertyChanged();
            }
        }

        public IEditCategoriesViewModel EditCategoriesViewModel
        {
            get => _editCategoriesViewModel;
            private set
            {
                if (value == _editCategoriesViewModel) return;
                _editCategoriesViewModel = value;
                OnPropertyChanged();
            }
        }

        public IEditPayeesViewModel EditPayeesViewModel
        {
            get => _editPayeesViewModel;
            private set
            {
                if (value == _editPayeesViewModel) return;
                _editPayeesViewModel = value;
                OnPropertyChanged();
            }
        }

        public IEditFlagsViewModel EditFlagsViewModel
        {
            get => _editFlagsViewModel;
            private set
            {
                if (value == _editFlagsViewModel) return;
                _editFlagsViewModel = value;
                OnPropertyChanged();
            }
        }

        public ICultureManager CultureManager
        {
            get => _cultureManager;
            private set
            {
                if (value == _cultureManager) return;
                _cultureManager = value;
                OnPropertyChanged();
            }
        }

        public ITransDataGridColumnManager TransDataGridColumnManager { get; }
        public IEmptyViewModel EmptyViewModel { get; set; }
        public bool IsEmpty => AccountTabsViewModel is null || BudgetOverviewViewModel is null;

        public CultureInfo LanguageCulture
        {
            get => _bffSettings.Culture_DefaultLanguage;
            set
            {
                _bffSettings.Culture_DefaultLanguage = value;

                CultureInfo customCulture = CultureInfo.CreateSpecificCulture(_bffSettings.Culture_DefaultLanguage.Name);
                customCulture.NumberFormat = CultureManager.CurrencyCulture.NumberFormat;
                customCulture.DateTimeFormat = CultureManager.DateCulture.DateTimeFormat;

                _setupLocalizationFramework.With(customCulture);
                Thread.CurrentThread.CurrentCulture = customCulture;
                Thread.CurrentThread.CurrentUICulture = customCulture;

                OnPropertyChanged();
            }
        }

        private const double BorderOffset = 50.0;

        private IParentTransactionViewModel _parentTransViewModel;

        public IParentTransactionViewModel ParentTransactionViewModel
        {
            get => _parentTransViewModel;
            set
            {
                _parentTransViewModel = value;
                OnPropertyChanged();
            }
        }

        private bool _parentTransFlyoutOpen;
        private IBudgetOverviewViewModel _budgetOverviewViewModel;
        private readonly SerialDisposable _contextSequence = new SerialDisposable();
        private IEditAccountsViewModel _editAccountsViewModel;
        private IEditCategoriesViewModel _editCategoriesViewModel;
        private IEditPayeesViewModel _editPayeesViewModel;
        private IEditFlagsViewModel _editFlagsViewModel;
        private ICultureManager _cultureManager;

        public IReadOnlyReactiveProperty<IParentTransactionViewModel> OpenParentTransaction { get; }

        public bool ParentTransFlyoutOpen
        {
            get => _parentTransFlyoutOpen;
            set
            {
                _parentTransFlyoutOpen = value;
                OnPropertyChanged();
            }
        }

        public MainWindowViewModel(
            Func<string, ILoadProjectContext> loadedProjectContextFactory,
            Func<IEmptyProjectContext> emptyContextFactory,
            Func<string, ICreateProjectContext> newBackendContextFactory,
            Func<IBffOpenFileDialog> bffOpenFileDialogFactory,
            Func<IBffSaveFileDialog> bffSaveFileDialogFactory,
            ITransDataGridColumnManager transDataGridColumnManager,
            IBffSettings bffSettings,
            IBffSystemInformation bffSystemInformation,
            ILocalizer localizer,
            ISetupLocalizationFramework setupLocalizationFramework,
            IParentTransactionFlyoutManager parentTransactionFlyoutManager,
            IEmptyViewModel emptyViewModel)
        {
            TransDataGridColumnManager = transDataGridColumnManager;
            EmptyViewModel = emptyViewModel;
            _loadedProjectContextFactory = loadedProjectContextFactory;
            _emptyContextFactory = emptyContextFactory;
            _bffSettings = bffSettings;
            _setupLocalizationFramework = setupLocalizationFramework;
            Logger.Debug("Initializing …");
            Reset(_bffSettings.DBLocation);

            //If the application is not visible on screen, than reset the default position
            //This might occur when one of multiple monitors is switched off or the screen resolution is changed while BFF is off
            if (X - BorderOffset > bffSystemInformation.VirtualScreenRight ||
                Y - BorderOffset > bffSystemInformation.VirtualScreenBottom ||
                X + Width - BorderOffset < bffSystemInformation.VirtualScreenLeft ||
                Y + Height - BorderOffset < bffSystemInformation.VirtualScreenTop)
            {
                X = 50.0;
                Y = 50.0;
            }

            OpenParentTransaction = parentTransactionFlyoutManager.OpenParentTransaction;

            NewBudgetPlanCommand = new RxRelayCommand(() =>
            {
                var bffSaveFileDialog = bffSaveFileDialogFactory();
                bffSaveFileDialog.Title = localizer.Localize("OpenSaveDialog_TitleNew");
                bffSaveFileDialog.Filter = localizer.Localize("OpenSaveDialog_Filter");
                bffSaveFileDialog.DefaultExt = "*.sqlite";
                if (bffSaveFileDialog.ShowDialog() == true)
                {
                    using (var backendContextFactory = newBackendContextFactory(bffSaveFileDialog.FileName))
                    {
                        backendContextFactory
                            .CreateProjectAsync();
                    }
                    Reset(bffSaveFileDialog.FileName);
                }
            });

            OpenBudgetPlanCommand = new RxRelayCommand(() =>
            {
                var bffOpenFileDialog = bffOpenFileDialogFactory();
                bffOpenFileDialog.Title = localizer.Localize("OpenSaveDialog_TitleOpen");
                bffOpenFileDialog.Filter = localizer.Localize("OpenSaveDialog_Filter");
                bffOpenFileDialog.DefaultExt = "*.sqlite";
                if (bffOpenFileDialog.ShowDialog() == true)
                {
                    Reset(bffOpenFileDialog.FileName);
                }
            });

            CloseBudgetPlanCommand = new RxRelayCommand(() => Reset(null));

            ParentTransactionOnClose = new RxRelayCommand(parentTransactionFlyoutManager.Close);

            Logger.Trace("Initializing done.");
        }

        private void Reset(string dbPath)
        {
            IProjectContext context;
            if (dbPath != null && File.Exists(dbPath))
            {
                var loadedProjectContext = _loadedProjectContextFactory(dbPath);
                _contextSequence.Disposable = loadedProjectContext;
                Title = $"{new FileInfo(dbPath).FullName} - BFF";
                _bffSettings.DBLocation = dbPath;
                context = (IProjectContext) loadedProjectContext;
            }
            else
            {
                var emptyProject = _emptyContextFactory();
                _contextSequence.Disposable = emptyProject;
                Title = "BFF";
                _bffSettings.DBLocation = "";
                context = (IProjectContext) emptyProject;
            }

            AccountTabsViewModel = context.AccountTabsViewModel;
            BudgetOverviewViewModel = context.BudgetOverviewViewModel;
            EditAccountsViewModel = context.EditAccountsViewModel;
            EditCategoriesViewModel = context.EditCategoriesViewModel;
            EditPayeesViewModel = context.EditPayeesViewModel;
            EditFlagsViewModel = context.EditFlagsViewModel;
            CultureManager = context.CultureManager;

            OnPropertyChanged(nameof(IsEmpty));
        }

        #region SizeLocationWindowState

        public double Width
        {
            get => _bffSettings.MainWindow_Width;
            set
            {
                _bffSettings.MainWindow_Width = value;
                OnPropertyChanged();
            }
        }

        public double Height
        {
            get => _bffSettings.MainWindow_Height;
            set
            {
                _bffSettings.MainWindow_Height = value;
                OnPropertyChanged();
            }
        }

        public double X
        {
            get => _bffSettings.MainWindow_X;
            set
            {
                _bffSettings.MainWindow_X = value;
                OnPropertyChanged();
            }
        }

        public double Y
        {
            get => _bffSettings.MainWindow_Y;
            set
            {
                _bffSettings.MainWindow_Y = value;
                OnPropertyChanged();
            }
        }

        public BffWindowState WindowState
        {
            get => _bffSettings.MainWindow_WindowState;
            set
            {
                _bffSettings.MainWindow_WindowState = value;
                OnPropertyChanged();
            }
        }

        #endregion
    }
}
