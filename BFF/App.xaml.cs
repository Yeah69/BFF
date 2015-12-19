﻿using System.Reflection;
using System.Windows;
using BFF.DB;
using BFF.DB.SQLite;
using BFF.Properties;
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
            IDb database = new SqLiteDb(Settings.Default.DBLocation);
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(database);
            MainWindow mainWindow = new MainWindow (mainWindowViewModel);
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
