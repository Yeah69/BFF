using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using BFF.View.Views;
using ControlzEx.Theming;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using NLog;
using WPFLocalizeExtension.Providers;

namespace BFF.View
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static App()
        {
            ResxLocalizationProvider.Instance.SearchCultures =
                new List<System.Globalization.CultureInfo>
                {
                    System.Globalization.CultureInfo.GetCultureInfo("de-DE"),
                    System.Globalization.CultureInfo.GetCultureInfo("en-US"),
                    System.Globalization.CultureInfo.GetCultureInfo("hu-HU"),
                    System.Globalization.CultureInfo.GetCultureInfo("ru-RU"),
                    System.Globalization.CultureInfo.GetCultureInfo("es-ES"),
                    System.Globalization.CultureInfo.GetCultureInfo("fr-FR"),
                    System.Globalization.CultureInfo.GetCultureInfo("it-IT"),
                    System.Globalization.CultureInfo.GetCultureInfo("ja"),
                    System.Globalization.CultureInfo.GetCultureInfo("nl-NL"),
                    System.Globalization.CultureInfo.GetCultureInfo("pl-PL"),
                    System.Globalization.CultureInfo.GetCultureInfo("pt-PT"),
                    System.Globalization.CultureInfo.GetCultureInfo("zh"),
                };
        }

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
            void SetColorsDependingOnTheChosenTheme(Theme? theme)
            {
                switch (theme?.BaseColorScheme)
                {
                    case "Light":
                        Resources["AlternatingRowBrush"] = Resources["MahApps.Brushes.Gray8"];
                        Resources["OpaqueZeroBrush"] = Resources["MahApps.Brushes.Gray6"];
                        break;
                    case "Dark":
                        Resources["AlternatingRowBrush"] = Resources["MahApps.Brushes.Gray8"];
                        Resources["OpaqueZeroBrush"] = Resources["MahApps.Brushes.Gray9"];
                        break;
                }
            }

            void ThemeManagerOnIsThemeChanged(object? s, ThemeChangedEventArgs args) => SetColorsDependingOnTheChosenTheme(args.NewTheme);

            ThemeManager.Current.ThemeChanged += ThemeManagerOnIsThemeChanged;

            var initialTheme = ThemeManager.Current.GetTheme(
                BFF.Properties.Settings.Default.MahApps_AppTheme,
                BFF.Properties.Settings.Default.MahApps_Accent);

            SetColorsDependingOnTheChosenTheme(initialTheme);
            ThemeManager.Current.ChangeTheme(this, initialTheme ?? throw new NullReferenceException());
            
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
