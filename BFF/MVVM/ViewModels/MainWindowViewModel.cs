using System;
using System.Globalization;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using Autofac.Features.OwnedInstances;
using BFF.DB;
using BFF.Helper;
using BFF.Helper.Extensions;
using BFF.Helper.Import;
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
        IRxRelayCommand ParentTransactionOnClose { get; }
        IRxRelayCommand<IImportable> ImportBudgetPlanCommand { get; }
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
        bool ParentTitFlyoutOpen { get; set; }
        double Width { get; set; }
        double Height { get; set; }
        double X { get; set; }
        double Y { get; set; }
        WindowState WindowState { get; set; }
    }

    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel, IOncePerApplication //todo IDisposable
    {
        private readonly Func<Owned<Func<string, ISqLiteBackendContext>>> _sqliteBackendContextFactory;
        private readonly Func<Owned<Func<IEmptyContext>>> _emptyContextFactory;
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
        public IRxRelayCommand ParentTransactionOnClose { get; }

        public IRxRelayCommand<IImportable> ImportBudgetPlanCommand { get; }

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

        public IAccountModuleColumnManager AccountModuleColumnManager { get; }
        public IEmptyViewModel EmptyViewModel { get; set; }
        public bool IsEmpty => AccountTabsViewModel is null || BudgetOverviewViewModel is null;

        public CultureInfo LanguageCulture
        {
            get => Settings.Default.Culture_DefaultLanguage;
            set
            {
                Settings.Default.Culture_DefaultLanguage = value;

                CultureInfo customCulture = CultureInfo.CreateSpecificCulture(Settings.Default.Culture_DefaultLanguage.Name);
                customCulture.NumberFormat = CultureManager.CurrencyCulture.NumberFormat;
                customCulture.DateTimeFormat = CultureManager.DateCulture.DateTimeFormat;

                WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture = customCulture;
                Thread.CurrentThread.CurrentCulture = customCulture;
                Thread.CurrentThread.CurrentUICulture = customCulture;

                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        private const double BorderOffset = 50.0;

        private IParentTransactionViewModel _parentTitViewModel;

        public IParentTransactionViewModel ParentTransactionViewModel
        {
            get => _parentTitViewModel;
            set
            {
                _parentTitViewModel = value;
                OnPropertyChanged();
            }
        }

        private bool _parentTitFlyoutOpen;
        private IBudgetOverviewViewModel _budgetOverviewViewModel;
        private readonly SerialDisposable _contextSequence = new SerialDisposable();
        private IEditAccountsViewModel _editAccountsViewModel;
        private IEditCategoriesViewModel _editCategoriesViewModel;
        private IEditPayeesViewModel _editPayeesViewModel;
        private IEditFlagsViewModel _editFlagsViewModel;
        private ICultureManager _cultureManager;

        public IReadOnlyReactiveProperty<IParentTransactionViewModel> OpenParentTransaction { get; }

        public bool ParentTitFlyoutOpen
        {
            get => _parentTitFlyoutOpen;
            set
            {
                _parentTitFlyoutOpen = value;
                OnPropertyChanged();
            }
        }

        public MainWindowViewModel(
            Func<Owned<Func<string, ISqLiteBackendContext>>> sqliteBackendContextFactory,
            Func<Owned<Func<IEmptyContext>>> emptyContextFactory,
            Func<Owned<Func<string, IProvideConnection>>> ownedCreateProvideConnectionFactory,
            Func<IProvideConnection, ICreateBackendOrm> createCreateBackendOrm,
            IAccountModuleColumnManager accountModuleColumnManager,
            IParentTransactionFlyoutManager parentTransactionFlyoutManager,
            IRxSchedulerProvider schedulerProvider,
            IEmptyViewModel emptyViewModel)
        {
            AccountModuleColumnManager = accountModuleColumnManager;
            EmptyViewModel = emptyViewModel;
            _sqliteBackendContextFactory = sqliteBackendContextFactory;
            _emptyContextFactory = emptyContextFactory;
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
                    var ownedCreateProvideConnection = ownedCreateProvideConnectionFactory();
                    createCreateBackendOrm(ownedCreateProvideConnection.Value(saveFileDialog.FileName))
                        .CreateAsync()
                        .ContinueWith(
                            t =>
                            {
                                schedulerProvider.UI.Schedule(Unit.Default, (sc, st) =>
                                {
                                    ownedCreateProvideConnection.Dispose();
                                    Reset(saveFileDialog.FileName);
                                    return Disposable.Empty;
                                });
                                
                            });
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

            ParentTransactionOnClose = new RxRelayCommand(parentTransactionFlyoutManager.Close);

            ImportBudgetPlanCommand = new RxRelayCommand<IImportable>(importableObject =>
            {
                string savePath = importableObject.Import();
                Reset(savePath);
            });

            Logger.Trace("Initializing done.");
        }

        protected void Reset(string dbPath)
        {
            IBackendContext context;
            if (File.Exists(dbPath))
            {
                var contextOwner = _sqliteBackendContextFactory();
                context = contextOwner.Value(dbPath);
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
