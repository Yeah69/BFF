using System.Windows;
using BFF.DB;
using BFF.DB.SQLite;
using BFF.Helper;
using MahApps.Metro;
using Ninject;

namespace BFF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            IBffOrm orm = new SqLiteBffOrm();
            IBffCultureProvider cultureProvider = new BffCultureProvider();
            BffEnvironment.CultureProvider = cultureProvider;
            MainWindow mainWindow = new MainWindow(orm);
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
