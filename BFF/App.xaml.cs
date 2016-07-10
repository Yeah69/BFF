using System;
using System.Windows;
using System.Windows.Threading;
using BFF.DB;
using BFF.DB.SQLite;
using BFF.Helper;
using BFF.Model.Native;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NLog;

namespace BFF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public App()
        {
            Logger.Trace("Initializing App");
            InitializeComponent();
            new AllAccounts(); //todo: Find more elegant way
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
            Logger.Info("BFF started. (Version: {0})", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
        }

        /// <summary>
        /// Shows a MessageBox with the Error Message when an error occurs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {

            Logger.Fatal(e.Exception, "An unhandled error occured!");
            var mySettings = new MetroDialogSettings { AffirmativeButtonText = "Okay" };
            //(MainWindow as MetroWindow).ShowMessageAsync("An unhandled error occured!", $"Error message:\r\n{e.Exception.Message}\r\nStackTrace:\r\n{e.Exception.StackTrace}", MessageDialogStyle.Affirmative, mySettings);
            (MainWindow as MetroWindow).ShowMessageAsync("An unhandled error occured!", $"Error message:\r\n{e.Exception.Message}", MessageDialogStyle.Affirmative, mySettings);
            e.Handled = true;
        }
    }
}
