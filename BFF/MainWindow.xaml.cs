using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro;


namespace BFF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        public MainWindow()
        {
            InitializeComponent();
            foreach (AppTheme theme in ThemeManager.AppThemes.OrderBy(x => x.Name))
                ThemeCombo.Items.Add(theme);
            foreach (Accent accent in ThemeManager.Accents.OrderBy(x => x.Name))
                AccentCombo.Items.Add(accent);
            LanguageCombo.Items.Add("de");
            LanguageCombo.Items.Add("en");
            Accent initialAccent = ThemeManager.GetAccent(Properties.Settings.Default.MahApps_Accent);
            AppTheme initialAppTheme = ThemeManager.GetAppTheme(Properties.Settings.Default.MahApps_AppTheme);
            ThemeCombo.SelectedItem = initialAppTheme;
            AccentCombo.SelectedItem = initialAccent;
            LanguageCombo.SelectedItem = Properties.Settings.Default.Localization_Language;

            ThemeManager.ChangeAppStyle(this, initialAccent, initialAppTheme);

            WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture = new System.Globalization.CultureInfo("de");

            //YnabCsvImport.ImportYnabTransactionsCsvtoDb(@"D:\Private\YNABExports\Yeah as of 2015-08-14 640 PM-Register.csv",
            //                                            @"D:\Private\YNABExports\Yeah as of 2015-08-14 640 PM-Budget.csv",
            //                                            @"testDatabase");
        }

        private void SettingsButt_Click(object sender, RoutedEventArgs e)
        {
            SettingsFlyout.IsOpen = true;
        }

        private void ThemeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Accent accent = ThemeManager.DetectAppStyle()?.Item2 ?? ThemeManager.GetAccent("Violet");
            ThemeManager.ChangeAppStyle(this, accent, ((AppTheme)ThemeCombo.SelectedItem));
            Properties.Settings.Default.MahApps_AppTheme = ((AppTheme)ThemeCombo.SelectedItem).Name;
            Properties.Settings.Default.Save();
        }

        private void AccentCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppTheme theme = ThemeManager.DetectAppStyle()?.Item1 ?? ThemeManager.GetAppTheme("BaseDark");
            ThemeManager.ChangeAppStyle(this, ((Accent)AccentCombo.SelectedItem), theme);
            Properties.Settings.Default.MahApps_Accent = ((Accent) AccentCombo.SelectedItem).Name;
            Properties.Settings.Default.Save();
        }

        private void LanguageCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string language = (string) LanguageCombo.SelectedItem;
            WPFLocalizeExtension.Engine.LocalizeDictionary.Instance.Culture = new System.Globalization.CultureInfo(language);
            Properties.Settings.Default.Localization_Language = language;
            Properties.Settings.Default.Save();
        }
    }
}
