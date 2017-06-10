using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using BFF.DB;
using BFF.DB.Dapper;
using BFF.DB.SQLite;
using BFF.Helper.Import;
using BFF.Properties;
using NLog;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace BFF.MVVM.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
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

        public ICommand NewBudgetPlanCommand => new RelayCommand(param => 
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("OpenSaveDialog_TitleNew", null, Settings.Default.Culture_DefaultLanguage),
                Filter = (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("OpenSaveDialog_Filter", null, Settings.Default.Culture_DefaultLanguage),
                DefaultExt = "*.sqlite"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                IProvideConnection provideConnection = new CreateSqLiteDatebase(saveFileDialog.FileName).Create();
                SqLiteBffOrm orm = new SqLiteBffOrm(provideConnection);
                Reset(saveFileDialog.FileName);
            }
        });

        public ICommand OpenBudgetPlanCommand => new RelayCommand(param => 
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("OpenSaveDialog_TitleOpen", null, Settings.Default.Culture_DefaultLanguage),
                Filter = (string)WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.GetLocalizedObject("OpenSaveDialog_Filter", null, Settings.Default.Culture_DefaultLanguage),
                DefaultExt = "*.sqlite"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                Reset(openFileDialog.FileName);
            }
        });

        public ICommand ImportBudgetPlanCommand => new RelayCommand(importableObject =>
        {
            string savePath = ((IImportable)importableObject).Import();
            Reset(savePath);
        });

        private SessionViewModelBase _contentViewModel;
        public SessionViewModelBase ContentViewModel
        {
            get => _contentViewModel;
            set
            {
                _contentViewModel = value;
                OnPropertyChanged();
            }
        }

        public CultureInfo LanguageCulture
        {
            get => Settings.Default.Culture_DefaultLanguage;
            set
            {
                Settings.Default.Culture_DefaultLanguage = value;
                _contentViewModel?.ManageCultures();
                OnPropertyChanged();
            }
        }

        public CultureInfo CurrencyCulture
        {
            get => Settings.Default.Culture_SessionCurrency;
            set
            {
                Settings.Default.Culture_SessionCurrency = value;
                _contentViewModel?.ManageCultures();
                OnPropertyChanged();
            }
        }

        public CultureInfo DateCulture
        {
            get => Settings.Default.Culture_SessionDate;
            set
            {
                Settings.Default.Culture_SessionDate = value;
                _contentViewModel?.ManageCultures();
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
                _contentViewModel?.ManageCultures();
                Messenger.Default.Send(CutlureMessage.RefreshDate);
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

        public bool ParentTitFlyoutOpen
        {
            get => _parentTitFlyoutOpen;
            set
            {
                _parentTitFlyoutOpen = value;
                OnPropertyChanged();
            }
        }

        public MainWindowViewModel()
        {
            Logger.Debug("Initializing …");
            Reset(Settings.Default.DBLocation);

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
            Logger.Trace("Initializing done.");
        }

        protected void Reset(string dbPath)
        {
            (ContentViewModel as IDisposable)?.Dispose();
            if (File.Exists(dbPath))
            {
                IBffOrm orm = new SqLiteBffOrm(new ProvideSqLiteConnection(dbPath));
                ContentViewModel = new AccountTabsViewModel(orm);
                Title = $"{new FileInfo(dbPath).Name} - BFF";
                Settings.Default.DBLocation = dbPath;
                Settings.Default.Save();
            }
            else
            {
                ContentViewModel = new EmptyContentViewModel();
                Title = "BFF";
                Settings.Default.DBLocation = "";
                Settings.Default.Save();
            }
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
