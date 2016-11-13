using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AlphaChiTech.Virtualization;
using BFF.Helper.Import;
using BFF.MVVM.ViewModels;
using BFF.MVVM.Views;
using BFF.Properties;
using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;

namespace BFF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
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

        public MainWindow()
        {
            InitializeComponent();
            //this routine only needs to run once, so first check to make sure the
            //VirtualizationManager isn’t already initialized
            if (!VirtualizationManager.IsInitialized)
            {
                //set the VirtualizationManager’s UIThreadExcecuteAction. In this case
                //we’re using Dispatcher.Invoke to give the VirtualizationManager access
                //to the dispatcher thread, and using a DispatcherTimer to run the background
                //operations the VirtualizationManager needs to run to reclaim pages and manage memory.
                VirtualizationManager.Instance.UIThreadExcecuteAction =
                    a => Dispatcher.Invoke(a);
                new DispatcherTimer(
                    TimeSpan.FromSeconds(1),
                    DispatcherPriority.Background,
                    delegate
                    {
                        VirtualizationManager.Instance.ProcessActions();
                    },
                    Dispatcher).Start();
            }
            InitializeCultureComboBoxes();
            InitializeAppThemeAndAccentComboBoxes();
            SetBinding(ImportCommandProperty, nameof(MainWindowViewModel.ImportBudgetPlanCommand));

            DataContext = new MainWindowViewModel();
        }

        private void InitializeCultureComboBoxes()
        {
            LanguageCombo.ItemsSource = WPFLocalizeExtension.Providers.ResxLocalizationProvider.Instance.AvailableCultures.Where(culture => !Equals(culture, CultureInfo.InvariantCulture));
            LanguageCombo.SelectedItem = Settings.Default.Culture_DefaultLanguage;

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
                Importable = new YnabCsvImport()
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
