using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using BFF.Views;
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

            MainWindow mainWindow = AutofacBootstrapper.Resolve<MainWindow>();
            mainWindow.Show();
        }

        public static Visibility IsDebug
        {
#if DEBUG
            get { return Visibility.Visible; }
#else
            get { return Visibility.Collapsed; }
#endif
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            void SetColorsDependingOnTheChosenTheme(AppTheme theme)
            {
                switch (theme.Name)
                {
                    case "BaseLight":
                        Resources["AlternatingRowBrush"] = Resources["GrayBrush8"];
                        Resources["OpaqueZeroBrush"] = Resources["GrayBrush6"];
                        break;
                    case "BaseDark":
                        Resources["AlternatingRowBrush"] = Resources["GrayBrush8"];
                        Resources["OpaqueZeroBrush"] = Resources["GrayBrush9"];
                        break;
                }
            }

            void ThemeManagerOnIsThemeChanged(object s, OnThemeChangedEventArgs args)
            {
                SetColorsDependingOnTheChosenTheme(args.AppTheme);
            }

            ThemeManager.IsThemeChanged += ThemeManagerOnIsThemeChanged;
            Accent initialAccent = ThemeManager.GetAccent(BFF.Properties.Settings.Default.MahApps_Accent);
            AppTheme initialAppTheme = ThemeManager.GetAppTheme(BFF.Properties.Settings.Default.MahApps_AppTheme);

            SetColorsDependingOnTheChosenTheme(initialAppTheme);
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

            Logger.Fatal(e.Exception, "An unhandled error occurred!");
            var mySettings = new MetroDialogSettings { AffirmativeButtonText = "Okay" };
            (MainWindow as MetroWindow).ShowMessageAsync("An unhandled error occurred!", $"Error message:\r\n{e.Exception.Message}", MessageDialogStyle.Affirmative, mySettings);
            e.Handled = true;
        }

        private void Flag_OnClick(object sender, EventArgs e)
        {
            if (sender is FrameworkElement element && element.FindName("Popup") is Popup popup)
            {
                popup.IsOpen = true;
            }
        }
    }
}
