using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using BFF.DB;
using BFF.Helper;
using BFF.Helper.Import;
using BFF.Model.Native;
using BFF.Properties;
using BFF.WPFStuff;
using NLog;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace BFF.ViewModel
{
    public class MainWindowViewModel : ObservableObject
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IBffOrm _orm;
        protected bool FileFlyoutIsOpen;
        protected string _title;
        private EmptyContentViewModel _contentViewModel;

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public ICommand NewBudgetPlanCommand => new RelayCommand(param => NewBudgetPlan(), param => true);
        public ICommand OpenBudgetPlanCommand => new RelayCommand(param => OpenBudgetPlan(), param => true);
        public ICommand ImportBudgetPlanCommand => new RelayCommand(ImportBudgetPlan, param => true);

        public EmptyContentViewModel ContentViewModel
        {
            get { return _contentViewModel; }
            set
            {
                _contentViewModel = value;
                OnPropertyChanged();
            }
        }

        public CultureInfo CurrencyCulture => BffEnvironment.CultureProvider.CurrencyCulture;
        public CultureInfo DateCulture => BffEnvironment.CultureProvider.DateCulture;
        public bool DateLong
        {
            get { return BffEnvironment.CultureProvider.DateLong; }
            set { BffEnvironment.CultureProvider.DateLong = value; }
        }

        private const double BorderOffset = 50.0;

        private ParentTitViewModel _parentTitViewModel;

        public ParentTitViewModel ParentTitViewModel
        {
            get { return _parentTitViewModel; }
            set
            {
                _parentTitViewModel = value;
                OnPropertyChanged();
            }
        }

        private bool _parentTitFlyoutOpen;

        public bool ParentTitFlyoutOpen
        {
            get { return _parentTitFlyoutOpen; }
            set
            {
                _parentTitFlyoutOpen = value;
                OnPropertyChanged();
            }
        }

        public MainWindowViewModel(IBffOrm orm)
        {
            logger.Debug("Initializing …");
            _orm = orm;
            Reset();

            //If the application is not visible on screen, than reset the default position
            //This might occure when one of multipe monitors is switched off or the screen resolution is changed while BFF is off
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
            logger.Trace("Initializing done.");
        }

        protected void NewBudgetPlan()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = (string) WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("OpenSaveDialog_TitleNew", null, BffEnvironment.CultureProvider.LanguageCulture), 
                Filter = (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("OpenSaveDialog_Filter", null, BffEnvironment.CultureProvider.LanguageCulture), 
                DefaultExt = "*.sqlite"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                _orm.CreateNewDatabase(saveFileDialog.FileName);
                Reset();
            }
        }

        protected void OpenBudgetPlan()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("OpenSaveDialog_TitleOpen", null, BffEnvironment.CultureProvider.LanguageCulture),
                Filter = (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("OpenSaveDialog_Filter", null, BffEnvironment.CultureProvider.LanguageCulture),
                DefaultExt = "*.sqlite"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                _orm.DbPath = openFileDialog.FileName;
                Reset();
            }
        }

        protected void ImportBudgetPlan(object importableObject)
        {
            string savePath = ((IImportable)importableObject).Import();
            _orm.DbPath = savePath; //todo: maybe remove, because the import does that already
            Reset();
        }

        protected void Reset()
        {
            if (File.Exists(_orm.DbPath) && ContentViewModel is AccountTabsViewModel)
            {
                ResetCultures();
                ContentViewModel.Refresh();
                Title = $"{new FileInfo(_orm.DbPath).Name} - BFF";
            }
            else if (File.Exists(_orm.DbPath) && !(ContentViewModel is AccountTabsViewModel))
            {
                ResetCultures();
                ContentViewModel = new AccountTabsViewModel(_orm);
                Title = $"{new FileInfo(_orm.DbPath).Name} - BFF";
            }
            else
            {
                ResetCultures(true);
                ContentViewModel = new EmptyContentViewModel();
                Title = "BFF";
            }
        }

        protected void ResetCultures(bool noDb = false)
        {
            if (noDb)
            {
                BffEnvironment.CultureProvider.CurrencyCulture = CultureInfo.GetCultureInfo("de-DE");
                BffEnvironment.CultureProvider.DateCulture = CultureInfo.GetCultureInfo("de-DE");
            }
            else
            {
                DbSetting dbSetting = _orm.Get<DbSetting>(1);
                BffEnvironment.CultureProvider.CurrencyCulture = CultureInfo.GetCultureInfo(dbSetting.CurrencyCultrureName);
                BffEnvironment.CultureProvider.DateCulture = CultureInfo.GetCultureInfo(dbSetting.DateCultureName);
            }
            OnPropertyChanged(nameof(CurrencyCulture));
            OnPropertyChanged(nameof(DateCulture));
        }

        #region SizeLocationWindowState

        public double Width
        {
            get { return Settings.Default.MainWindow_Width; }
            set
            {
                Settings.Default.MainWindow_Width = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public double Height
        {
            get { return Settings.Default.MainWindow_Height; }
            set
            {
                Settings.Default.MainWindow_Height = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public double X
        {
            get { return Settings.Default.MainWindow_X; }
            set
            {
                Settings.Default.MainWindow_X = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public double Y
        {
            get { return Settings.Default.MainWindow_Y; }
            set
            {
                Settings.Default.MainWindow_Y = value;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public WindowState WindowState
        {
            get { return Settings.Default.MainWindow_WindowState; }
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
