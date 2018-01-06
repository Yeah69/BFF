using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Autofac.Features.OwnedInstances;
using BFF.DB.Dapper;
using BFF.Helper.Extensions;
using BFF.Helper.Import;
using BFF.Properties;
using NLog;
using Reactive.Bindings;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace BFF.MVVM.ViewModels
{
    public interface IMainWindowViewModel : IViewModel
    {
        ReactiveCommand NewBudgetPlanCommand { get; }
        ReactiveCommand OpenBudgetPlanCommand { get; }
        ReactiveCommand<IImportable> ImportBudgetPlanCommand { get; }
        IAccountTabsViewModel AccountTabsViewModel { get; set; }
        IBudgetOverviewViewModel BudgetOverviewViewModel { get; set; }
        IEmptyViewModel EmptyViewModel { get; }
        bool IsEmpty { get; }
        CultureInfo LanguageCulture { get; set; }
        CultureInfo CurrencyCulture { get; set; }
        CultureInfo DateCulture { get; set; }
        bool DateLong { get; set; }
        ParentTitViewModel ParentTitViewModel { get; set; }
        bool ParentTitFlyoutOpen { get; set; }
        double Width { get; set; }
        double Height { get; set; }
        double X { get; set; }
        double Y { get; set; }
        WindowState WindowState { get; set; }
    }

    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
    {
        private readonly Func<Owned<Func<string, ISqLiteBackendContext>>> _sqliteBackendContextFactory;
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

        public ReactiveCommand NewBudgetPlanCommand { get; } = new ReactiveCommand();

        public ReactiveCommand OpenBudgetPlanCommand { get; } = new ReactiveCommand();

        public ReactiveCommand<IImportable> ImportBudgetPlanCommand { get; } = new ReactiveCommand<IImportable>();

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

        public ISqLiteBackendContext Context
        {
            get => _context;
            set
            {
                if (value == _context) return;
                _context = value;
                OnPropertyChanged();
            }
        }

        public string Text => "Hello, Worldddd";

        public IEmptyViewModel EmptyViewModel { get; set; }
        public bool IsEmpty => AccountTabsViewModel == null || BudgetOverviewViewModel == null;

        public CultureInfo LanguageCulture
        {
            get => Settings.Default.Culture_DefaultLanguage;
            set
            {
                Settings.Default.Culture_DefaultLanguage = value;
                _accountTabsViewModel?.ManageCultures();
                OnPropertyChanged();
            }
        }

        public CultureInfo CurrencyCulture
        {
            get => Settings.Default.Culture_SessionCurrency;
            set
            {
                Settings.Default.Culture_SessionCurrency = value;
                _accountTabsViewModel?.ManageCultures();
                Messenger.Default.Send(CultureMessage.RefreshCurrency);
                OnPropertyChanged();
            }
        }

        public CultureInfo DateCulture
        {
            get => Settings.Default.Culture_SessionDate;
            set
            {
                Settings.Default.Culture_SessionDate = value;
                _accountTabsViewModel?.ManageCultures();
                Messenger.Default.Send(CultureMessage.RefreshDate);
                OnPropertyChanged();
            }
        }

        //todo: put DateLong into Database, too?
        public bool DateLong
        {
            get => Settings.Default.Culture_DefaultDateLong;
            set
            {
                Settings.Default.Culture_DefaultDateLong = value;
                _accountTabsViewModel?.ManageCultures();
                Messenger.Default.Send(CultureMessage.RefreshDate);
                OnPropertyChanged();
            }
        }

        private const double BorderOffset = 50.0;

        private ParentTitViewModel _parentTitViewModel;

        public ParentTitViewModel ParentTitViewModel
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
        private Owned<Func<string, ISqLiteBackendContext>> _contextOwner;
        private ISqLiteBackendContext _context;

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
            IEmptyViewModel emptyViewModel)
        {
            EmptyViewModel = emptyViewModel;
            _sqliteBackendContextFactory = sqliteBackendContextFactory;
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

            Messenger.Default.Register<ParentTitViewModel>(this, parentTitViewModel =>
            {
                ParentTitViewModel = parentTitViewModel;
                ParentTitFlyoutOpen = true;
            });

            NewBudgetPlanCommand.Subscribe(_ =>
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Title = "OpenSaveDialog_TitleNew".Localize<string>(),
                    Filter = "OpenSaveDialog_Filter".Localize<string>(),
                    DefaultExt = "*.sqlite"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    new CreateSqLiteDatabase(saveFileDialog.FileName).Create();
                    Reset(saveFileDialog.FileName);
                }
            });

            OpenBudgetPlanCommand.Subscribe(_ =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "OpenSaveDialog_TitleOpen".Localize<string>(),
                    Filter = "OpenSaveDialog_Filter".Localize<string>(),
                    DefaultExt = "*.sqlite"
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    Reset(openFileDialog.FileName);
                }
            });

            ImportBudgetPlanCommand.Subscribe(importableObject =>
            {
                string savePath = importableObject.Import();
                Reset(savePath);
            });

            Logger.Trace("Initializing done.");
        }

        protected void Reset(string dbPath)
        {
            _contextOwner?.Dispose();
            if (File.Exists(dbPath))
            {
                //_ormOwner = _ormFactory();
                //IBffOrm orm = _ormOwner.Value(_provideSqLiteConnectionFactory(dbPath));
                _contextOwner = _sqliteBackendContextFactory();
                Context = _contextOwner.Value(dbPath);
                
                AccountTabsViewModel = Context.AccountTabsViewModel;
                    //new AccountTabsViewModel(
                    //orm, 
                    //new NewAccountViewModel( 
                    //    orm.BffRepository.AccountRepository,
                    //    orm.CommonPropertyProvider.AccountViewModelService));
                BudgetOverviewViewModel = Context.BudgetOverviewViewModel;
                    //new BudgetOverviewViewModel(
                    //    orm.BffRepository.BudgetMonthRepository,
                    //    orm.BudgetEntryViewModelService, 
                    //    orm.CommonPropertyProvider.CategoryViewModelService,
                    //    orm.BffRepository.CategoryRepository);
                Title = $"{new FileInfo(dbPath).Name} - BFF";
                Settings.Default.DBLocation = dbPath;
                Settings.Default.Save();
            }
            else
            {
                AccountTabsViewModel = null;
                BudgetOverviewViewModel = null;
                Title = "BFF";
                Settings.Default.DBLocation = "";
                Settings.Default.Save();
            }
            OnPropertyChanged(nameof(IsEmpty));
            OnPropertyChanged(nameof(CurrencyCulture));
            OnPropertyChanged(nameof(DateCulture));
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
