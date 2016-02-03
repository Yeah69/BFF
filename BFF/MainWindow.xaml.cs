using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using BFF.DB;
using BFF.Helper;
using BFF.Helper.Import;
using BFF.Properties;
using BFF.ViewModel;
using BFF.WPFStuff.UserControls;
using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;

namespace BFF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IRefreshCurrencyVisuals, IRefreshDateVisuals
    {
        public static readonly DependencyProperty ImportCommandProperty =
            DependencyProperty.Register(nameof(ImportCommand), typeof(ICommand), typeof(MainWindow),
                new PropertyMetadata((depObj, args) =>
                {
                    var mw = (MainWindow)depObj;
                    mw.ImportCommand = (ICommand)args.NewValue;
                }));

        public ICommand ImportCommand
        {
            get { return (ICommand)GetValue(ImportCommandProperty); }
            set { SetValue(ImportCommandProperty, value); }
        }

        private readonly IBffOrm _orm;

        public MainWindow(IBffOrm orm)
        {
            _orm = orm;
            InitializeComponent();
            InitializeCultureComboBoxes();
            InitializeAppThemeAndAccentComboBoxes();
            SetBinding(ImportCommandProperty, nameof(MainWindowViewModel.ImportBudgetPlanCommand));

            DataContext = new MainWindowViewModel(_orm);
        }

        private void InitializeCultureComboBoxes()
        {
            string initialLocalization = Settings.Default.Localization_Language;
            LanguageCombo.Items.Add("de-DE");
            LanguageCombo.Items.Add("en-US");
            LanguageCombo.SelectedItem = initialLocalization;
            BffEnvironment.CultureProvider.LanguageCulture = CultureInfo.GetCultureInfo(initialLocalization);

            foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.AllCultures).ToList().OrderBy(x => x.Name))
            {
                CurrencyCombo.Items.Add(culture);
                DateCombo.Items.Add(culture);
            }
        }

        private void InitializeAppThemeAndAccentComboBoxes()
        {
            AppTheme initialAppTheme = ThemeManager.GetAppTheme(Settings.Default.MahApps_AppTheme);
            Accent initialAccent = ThemeManager.GetAccent(Settings.Default.MahApps_Accent);
            foreach (AppTheme theme in ThemeManager.AppThemes.OrderBy(x => x.Name))
            {
                ThemeWrap current = new ThemeWrap
                {
                    Theme = theme,
                    WhiteBrush = (SolidColorBrush)theme.Resources["WhiteBrush"]
                };
                ThemeCombo.Items.Add(current);
                if (theme == initialAppTheme)
                    ThemeCombo.SelectedItem = current;
            }
            foreach (Accent accent in ThemeManager.Accents.OrderBy(x => x.Name))
            {
                AccentWrap current = new AccentWrap
                {
                    Accent = accent,
                    AccentColorBrush = (SolidColorBrush)accent.Resources["AccentColorBrush"]
                };
                AccentCombo.Items.Add(current);
                if (accent == initialAccent)
                    AccentCombo.SelectedItem = current;
            }
        }

        private void SettingsButt_Click(object sender, RoutedEventArgs e)
        {
            SettingsFlyout.IsOpen = true;
        }

        private void ThemeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Accent accent = ThemeManager.GetAccent(Settings.Default.MahApps_Accent);
            ThemeManager.ChangeAppStyle(Application.Current, accent, ((ThemeWrap)ThemeCombo.SelectedItem).Theme);
            Settings.Default.MahApps_AppTheme = ((ThemeWrap)ThemeCombo.SelectedItem).Theme.Name;
            Settings.Default.Save();
        }

        private void AccentCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppTheme theme = ThemeManager.GetAppTheme(Settings.Default.MahApps_AppTheme);
            ThemeManager.ChangeAppStyle(Application.Current, ((AccentWrap)AccentCombo.SelectedItem).Accent, theme);
            Settings.Default.MahApps_Accent = ((AccentWrap)AccentCombo.SelectedItem).Accent.Name;
            Settings.Default.Save();
        }

        private void LanguageCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string language = (string) LanguageCombo.SelectedItem;

            BffEnvironment.CultureProvider.LanguageCulture = CultureInfo.GetCultureInfo(language);
        }

        private void FileButt_Click(object sender, RoutedEventArgs e)
        {
            FileFlyout.IsOpen = true;
        }

        private void Close_FileFlyout(object sender, RoutedEventArgs e)
        {
            FileFlyout.IsOpen = false;
        }

        private void Open_ImportDialog(object sender, RoutedEventArgs e)
        {
            FileFlyout.IsOpen = false;

            MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Theme;
            ImportDialogViewModel importDialogVm = new ImportDialogViewModel
            {
                Importable = new YnabCsvImport(_orm)
                {
                    TransactionPath = Settings.Default.Import_YnabCsvTransaction,
                    BudgetPath = Settings.Default.Import_YnabCsvBudget,
                    SavePath = Settings.Default.Import_SavePath
                }
            };
            ImportDialog importDialog = new ImportDialog{ DataContext = importDialogVm };
            importDialog.ButtCancel.Click += (o, args) => this.HideMetroDialogAsync(importDialog);
            importDialog.ButtImport.Click += (o, args) =>
            {
                Settings.Default.Import_YnabCsvTransaction = ((YnabCsvImport)importDialogVm.Importable).TransactionPath;
                Settings.Default.Import_YnabCsvBudget = ((YnabCsvImport)importDialogVm.Importable).BudgetPath;
                Settings.Default.Import_SavePath = importDialogVm.Importable.SavePath;
                Settings.Default.Save();
                this.HideMetroDialogAsync(importDialog);
                if(ImportCommand.CanExecute(importDialogVm.Importable))
                    Dispatcher.Invoke(() => ImportCommand.Execute(importDialogVm.Importable), DispatcherPriority.Background);
            };
            this.ShowMetroDialogAsync(importDialog);
        }

        private void CurrencyCombo_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BffEnvironment.CultureProvider.CurrencyCulture = (CultureInfo) ((ComboBox) sender).SelectedItem;
            RefreshCurrencyVisuals();
        }

        public void RefreshCurrencyVisuals()
        {
            if(MainContentControl.Content != null)
            {
                //When the MainContentControl has Content, then the first child is the ContentPresenter 
                //and its first child is the root of the DataTemplates generated content
                DependencyObject depObj = VisualTreeHelper.GetChild(MainContentControl, 0);
                if (VisualTreeHelper.GetChildrenCount(depObj) > 0)
                {
                    depObj = VisualTreeHelper.GetChild(depObj, 0);
                    if (depObj is IRefreshCurrencyVisuals)
                    {
                        IRefreshCurrencyVisuals rcv = depObj as IRefreshCurrencyVisuals;
                        rcv.RefreshCurrencyVisuals();
                    }
                }
            }
        }

        private void DateCombo_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BffEnvironment.CultureProvider.DateCulture = (CultureInfo)((ComboBox)sender).SelectedItem;
            RefreshDateVisuals();
        }

        public void RefreshDateVisuals()
        {
            if(MainContentControl.Content != null)
            {
                //When the MainContentControl has Content, then the first child is the ContentPresenter 
                //and its first child is the root of the DataTemplates generated content
                DependencyObject depObj = VisualTreeHelper.GetChild(MainContentControl, 0);
                if (VisualTreeHelper.GetChildrenCount(depObj) > 0)
                {
                    depObj = VisualTreeHelper.GetChild(depObj, 0);
                    if (depObj is IRefreshDateVisuals)
                    {
                        IRefreshDateVisuals rcv = depObj as IRefreshDateVisuals;
                        rcv.RefreshDateVisuals();
                    }
                }
            }
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            RefreshDateVisuals();
        }
    }

    internal class ThemeWrap
    {
        public AppTheme Theme { get; set; }
        public SolidColorBrush WhiteBrush { get; set; }

        public override string ToString()
        {
            return Theme.Name;
        }
    }

    internal class AccentWrap
    {
        public Accent Accent { get; set; }
        public SolidColorBrush AccentColorBrush { get; set; }
    }
}
