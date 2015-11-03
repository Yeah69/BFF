using System.Reflection;
using System.Windows;
using BFF.DB.SQLite;
using BFF.ViewModel;
using MahApps.Metro;

namespace BFF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();
            MainWindow mainWindow = new MainWindow {DataContext = mainWindowViewModel};
            mainWindow.Show();

        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Accent initialAccent = ThemeManager.GetAccent(BFF.Properties.Settings.Default.MahApps_Accent);
            AppTheme initialAppTheme = ThemeManager.GetAppTheme(BFF.Properties.Settings.Default.MahApps_AppTheme);
            ThemeManager.ChangeAppStyle(Current, initialAccent, initialAppTheme);
        }
    }
}
