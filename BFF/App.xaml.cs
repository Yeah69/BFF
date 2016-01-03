using System.Windows;
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
            using (StandardKernel kernel = new StandardKernel(new BffNinjectModule()))
            {
                MainWindow mainWindow = kernel.Get<MainWindow>();
                mainWindow.Show();
            }

        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Accent initialAccent = ThemeManager.GetAccent(BFF.Properties.Settings.Default.MahApps_Accent);
            AppTheme initialAppTheme = ThemeManager.GetAppTheme(BFF.Properties.Settings.Default.MahApps_AppTheme);
            ThemeManager.ChangeAppStyle(Current, initialAccent, initialAppTheme);
        }
    }
}
