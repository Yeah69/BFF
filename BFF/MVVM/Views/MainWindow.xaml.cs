using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using BFF.MVVM.ViewModels;
using BFF.Properties;
using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;

namespace BFF.MVVM.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        
        private readonly Func<IImportDialogViewModel> _importDialogViewModelFactory;
        

        public MainWindow(
            IMainWindowViewModel dataContext,
            Func<IImportDialogViewModel> importDialogViewModelFactory)
        {
            InitializeComponent();
            
            InitializeCultureComboBoxes();
            InitializeAppThemeAndAccentComboBoxes();

            DataContext = dataContext;
            
            _importDialogViewModelFactory = importDialogViewModelFactory;

            void InitializeCultureComboBoxes()
            {
                LanguageCombo.ItemsSource = WPFLocalizeExtension.Providers.ResxLocalizationProvider.Instance.AvailableCultures.Where(culture => !Equals(culture, CultureInfo.InvariantCulture));
                LanguageCombo.SelectedItem = Settings.Default.Culture_DefaultLanguage;

                foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.AllCultures).ToList().OrderBy(x => x.Name))
                {
                    CurrencyCombo.Items.Add(culture);
                    DateCombo.Items.Add(culture);
                }
            }

            void InitializeAppThemeAndAccentComboBoxes()
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

            IImportDialogViewModel importDialogViewModel = _importDialogViewModelFactory();
            ImportDialog importDialog = new ImportDialog{ DataContext = importDialogViewModel };
            importDialog.ButtCancel.Click += (o, args) => this.HideMetroDialogAsync(importDialog);
            importDialog.ButtImport.Click += (o, args) =>
            {
                this.HideMetroDialogAsync(importDialog);
                Dispatcher.Invoke(() => importDialogViewModel.Import(), DispatcherPriority.Background);
            };
            this.ShowMetroDialogAsync(importDialog);
        }

        private void EditAccounts_OnClick(object sender, RoutedEventArgs e)
        {
            EditAccountsFlyout.IsOpen = true;
        }

        private void EditCategories_OnClick(object sender, RoutedEventArgs e)
        {
            EditCategoriesFlyout.IsOpen = true;
        }

        private void EditPayees_OnClick(object sender, RoutedEventArgs e)
        {
            EditPayeesFlyout.IsOpen = true;
        }

        private void EditFlags_OnClick(object sender, RoutedEventArgs e)
        {
            EditFlagsFlyout.IsOpen = true;
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
