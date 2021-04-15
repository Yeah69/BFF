using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using BFF.Properties;
using BFF.View.Views.Dialogs;
using BFF.ViewModel.ViewModels;
using BFF.ViewModel.ViewModels.Import;
using ControlzEx.Theming;
using MahApps.Metro.Controls.Dialogs;
using MrMeeseeks.Extensions;
using WPFLocalizeExtension.Providers;

namespace BFF.View.Views
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
                LanguageCombo.ItemsSource = ResxLocalizationProvider.Instance.AvailableCultures.Where(culture => !Equals(culture, CultureInfo.InvariantCulture));
                LanguageCombo.SelectedItem = Settings.Default.Culture_DefaultLanguage;
                
                foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.AllCultures).ToList().OrderBy(x => x.Name))
                {
                    CurrencyCombo.Items.Add(culture);
                    DateCombo.Items.Add(culture);
                }
            }

            void InitializeAppThemeAndAccentComboBoxes()
            {
                Theme? initialTheme = ThemeManager.Current.GetTheme(Settings.Default.MahApps_AppTheme, Settings.Default.MahApps_Accent);
                foreach (var theme in 
                    ThemeManager
                        .Current
                        .BaseColors
                        .Select(c => ThemeManager.Current.GetTheme(c, "Blue"))
                        .WhereNotNullRef()
                        .Select(theme => new ThemeWrap(theme.BaseColorScheme, (SolidColorBrush)theme.Resources["MahApps.Brushes.ThemeBackground"]))
                        .OrderBy(x => x.Name))
                {
                    ThemeCombo.Items.Add(theme);
                    if (theme.Name == initialTheme?.BaseColorScheme)
                        ThemeCombo.SelectedItem = theme;
                }
                foreach (var theme in 
                    ThemeManager
                        .Current
                        .ColorSchemes
                        .Select(c => ThemeManager.Current.GetTheme("Dark", c))
                        .WhereNotNullRef()
                        .Select(theme => new ThemeWrap(theme.ColorScheme, (SolidColorBrush)theme.Resources["MahApps.Brushes.Accent"]))
                        .OrderBy(x => x.Name))
                {
                    AccentCombo.Items.Add(theme);
                    if (theme.Name == initialTheme?.ColorScheme)
                        AccentCombo.SelectedItem = theme;
                }
            }
        }

        private void SettingsButt_Click(object sender, RoutedEventArgs e)
        {
            SettingsFlyout.IsOpen = true;
        }

        private void ThemeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var baseColorScheme = ((ThemeWrap)ThemeCombo.SelectedItem).Name;
            var theme = ThemeManager.Current.GetTheme(baseColorScheme, Settings.Default.MahApps_Accent);
            ThemeManager.Current.ChangeTheme(Application.Current, theme ?? throw new NullReferenceException());
            Settings.Default.MahApps_AppTheme = baseColorScheme;
            Settings.Default.Save();
        }

        private void AccentCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var colorScheme = ((ThemeWrap)AccentCombo.SelectedItem).Name;
            var theme = ThemeManager.Current.GetTheme(Settings.Default.MahApps_AppTheme, colorScheme);
            ThemeManager.Current.ChangeTheme(Application.Current, theme ?? throw new NullReferenceException());
            Settings.Default.MahApps_Accent = colorScheme;
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
            var importDialog = new ImportDialog{ DataContext = importDialogViewModel };
            importDialog.ButtCancel.Click += (_, _) => this.HideMetroDialogAsync(importDialog);
            importDialog.ButtImport.Click += (_, _) =>
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
        public ThemeWrap(string name, SolidColorBrush brush)
        {
            Name = name;
            Brush = brush;
        }

        public string Name { get; }
        public SolidColorBrush Brush { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}
