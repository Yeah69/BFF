using System;
using System.Globalization;
using System.IO;
using System.Reactive.Disposables;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using Autofac.Features.OwnedInstances;
using BFF.Contexts;
using BFF.Core.IoC;
using BFF.Core.Persistence;
using BFF.Helper.Extensions;
using BFF.Model;
using BFF.Model.Contexts;
using BFF.MVVM.Managers;
using BFF.MVVM.ViewModels.ForModels;
using BFF.Properties;
using NLog;
using Reactive.Bindings;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace BFF.MVVM.ViewModels
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
        WindowState WindowState { get; set; }
        string Title { get; }
        ITransDataGridColumnManager TransDataGridColumnManager { get; }
    }

    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel, IOncePerApplication //todo IDisposable
    {
        private readonly Func<Owned<Func<IPersistenceConfiguration, ILoadedProjectContext>>> _loadedProjectContextFactory;
        private readonly Func<Owned<Func<IEmptyProjectContext>>> _emptyContextFactory;
        private readonly Func<string, ISqlitePersistenceConfiguration> _sqlitePersistenceConfigurationFactory;
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
            get => Settings.Default.Culture_DefaultLanguage;
            set
            {
                Settings.Default.Culture_DefaultLanguage = value;
                Settings.Default.Save();

                CultureInfo customCulture = CultureInfo.CreateSpecificCulture(Settings.Default.Culture_DefaultLanguage.Name);
                customCulture.NumberFormat = CultureManager.CurrencyCulture.NumberFormat;
                customCulture.DateTimeFormat = CultureManager.DateCulture.DateTimeFormat;

                WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture = customCulture;
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
            Func<Owned<Func<IPersistenceConfiguration, ILoadedProjectContext>>> loadedProjectContextFactory,
            Func<Owned<Func<IEmptyProjectContext>>> emptyContextFactory,
            Func<Owned<Func<IPersistenceConfiguration, INewBackendContext>>> newBackendContextFactory,
            Func<string, ISqlitePersistenceConfiguration> sqlitePersistenceConfigurationFactory,
            ITransDataGridColumnManager transDataGridColumnManager,
            IParentTransactionFlyoutManager parentTransactionFlyoutManager,
            IEmptyViewModel emptyViewModel)
        {
            TransDataGridColumnManager = transDataGridColumnManager;
            EmptyViewModel = emptyViewModel;
            _loadedProjectContextFactory = loadedProjectContextFactory;
            _emptyContextFactory = emptyContextFactory;
            _sqlitePersistenceConfigurationFactory = sqlitePersistenceConfigurationFactory;
            Logger.Debug("Initializing …");
            Reset(Settings.Default.DBLocation);

            //If the application is not visible on screen, than reset the default position
            //This might occur when one of multiple monitors is switched off or the screen resolution is changed while BFF is off
            if (X - BorderOffset > SystemInformation.VirtualScreen.Right ||
                Y - BorderOffset > SystemInformation.VirtualScreen.Bottom ||
                X + Width - BorderOffset < SystemInformation.VirtualScreen.Left ||
                Y + Height - BorderOffset < SystemInformation.VirtualScreen.Top)
            {
                X = 50.0;
                Y = 50.0;
            }

            OpenParentTransaction = parentTransactionFlyoutManager.OpenParentTransaction;

            NewBudgetPlanCommand = new RxRelayCommand(() =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Title = "OpenSaveDialog_TitleNew".Localize(),
                    Filter = "OpenSaveDialog_Filter".Localize(),
                    DefaultExt = "*.sqlite"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var backendContextFactory = newBackendContextFactory())
                    {
                        backendContextFactory
                            .Value(sqlitePersistenceConfigurationFactory(saveFileDialog.FileName))
                            .CreateNewBackend();
                    }
                    Reset(saveFileDialog.FileName);
                }
            });

            OpenBudgetPlanCommand = new RxRelayCommand(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "OpenSaveDialog_TitleOpen".Localize(),
                    Filter = "OpenSaveDialog_Filter".Localize(),
                    DefaultExt = "*.sqlite"
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    Reset(openFileDialog.FileName);
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
                var contextOwner = _loadedProjectContextFactory();
                var sqlitePersistenceConfiguration = _sqlitePersistenceConfigurationFactory(dbPath);
                context = contextOwner.Value(sqlitePersistenceConfiguration);
                _contextSequence.Disposable = contextOwner;
                Title = $"{new FileInfo(dbPath).FullName} - BFF";
                Settings.Default.DBLocation = dbPath;
            }
            else
            {
                var contextOwner = _emptyContextFactory();
                context = contextOwner.Value();
                _contextSequence.Disposable = contextOwner;
                Title = "BFF";
                Settings.Default.DBLocation = "";
            }
            Settings.Default.Save();

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
            get => Settings.Default.MainWindow_Width;
            set
            {
                Settings.Default.MainWindow_Width = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public double Height
        {
            get => Settings.Default.MainWindow_Height;
            set
            {
                Settings.Default.MainWindow_Height = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public double X
        {
            get => Settings.Default.MainWindow_X;
            set
            {
                Settings.Default.MainWindow_X = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public double Y
        {
            get => Settings.Default.MainWindow_Y;
            set
            {
                Settings.Default.MainWindow_Y = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public WindowState WindowState
        {
            get => Settings.Default.MainWindow_WindowState;
            set
            {
                Settings.Default.MainWindow_WindowState = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        #endregion
    }
}
