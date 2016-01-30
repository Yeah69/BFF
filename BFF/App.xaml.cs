﻿using System.Windows;
using BFF.DB;
using BFF.DB.SQLite;
using BFF.Helper;
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
            InitializeComponent();
            IBffOrm orm = new SqLiteBffOrm();
            IBffCultureProvider cultureProvider = new BffCultureProvider(orm);
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
