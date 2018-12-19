using System;
using System.Globalization;
using System.Windows;
using BFF.Core.Helper;
using BFF.Properties;
using BFF.ViewModel.Helper;

namespace BFF.Helper
{
    internal class BffSettings : IBffSettings
    {
        public string CsvBankStatementImportProfiles
        {
            get => Settings.Default.CsvBankStatementImportProfiles;
            set => Settings.Default.CsvBankStatementImportProfiles = value;
        }

        public string OpenMainTab
        {
            get => Settings.Default.OpenMainTab;
            set => Settings.Default.OpenMainTab = value;
        }

        public bool Culture_DefaultDateLong
        {
            get => Settings.Default.Culture_DefaultDateLong;
            set => Settings.Default.Culture_DefaultDateLong = value;
        }
        public string OpenAccountTab
        {
            get => Settings.Default.OpenAccountTab;
            set => Settings.Default.OpenAccountTab = value;
        }

        public string Import_YnabCsvTransaction
        {
            get => Settings.Default.Import_YnabCsvTransaction;
            set => Settings.Default.Import_YnabCsvTransaction = value;
        }
        public string Import_YnabCsvBudget
        {
            get => Settings.Default.Import_YnabCsvBudget;
            set => Settings.Default.Import_YnabCsvBudget = value;
        }
        public string Import_SavePath
        {
            get => Settings.Default.Import_SavePath;
            set => Settings.Default.Import_SavePath = value;
        }
        public bool ShowFlags
        {
            get => Settings.Default.ShowFlags;
            set => Settings.Default.ShowFlags = value;
        }
        public bool ShowCheckNumbers
        {
            get => Settings.Default.ShowCheckNumbers;
            set => Settings.Default.ShowCheckNumbers = value;
        }
        public bool NeverShowEditHeaders
        {
            get => Settings.Default.NeverShowEditHeaders;
            set => Settings.Default.NeverShowEditHeaders = value;
        }
        public CultureInfo Culture_SessionCurrency
        {
            get => Settings.Default.Culture_SessionCurrency;
            set => Settings.Default.Culture_SessionCurrency = value;
        }
        public CultureInfo Culture_SessionDate
        {
            get => Settings.Default.Culture_SessionDate;
            set => Settings.Default.Culture_SessionDate = value;
        }
        public CultureInfo Culture_DefaultCurrency
        {
            get => Settings.Default.Culture_DefaultCurrency;
            set => Settings.Default.Culture_DefaultCurrency = value;
        }
        public CultureInfo Culture_DefaultDate
        {
            get => Settings.Default.Culture_DefaultDate;
            set => Settings.Default.Culture_DefaultDate = value;
        }
        public CultureInfo Culture_DefaultLanguage
        {
            get => Settings.Default.Culture_DefaultLanguage;
            set => Settings.Default.Culture_DefaultLanguage = value;
        }
        public string SelectedCsvProfile
        {
            get => Settings.Default.SelectedCsvProfile;
            set => Settings.Default.SelectedCsvProfile = value;
        }

        public string DBLocation
        {
            get => Settings.Default.DBLocation;
            set => Settings.Default.DBLocation = value;
        }
        public double MainWindow_Width
        {
            get => Settings.Default.MainWindow_Width;
            set => Settings.Default.MainWindow_Width = value;
        }
        public double MainWindow_Height
        {
            get => Settings.Default.MainWindow_Height;
            set => Settings.Default.MainWindow_Height = value;
        }
        public double MainWindow_X
        {
            get => Settings.Default.MainWindow_X;
            set => Settings.Default.MainWindow_X = value;
        }
        public double MainWindow_Y
        {
            get => Settings.Default.MainWindow_Y;
            set => Settings.Default.MainWindow_Y = value;
        }

        public BffWindowState MainWindow_WindowState
        {
            get {
                switch (Settings.Default.MainWindow_WindowState)
                {
                    case WindowState.Normal:
                        return BffWindowState.Normal;
                    case WindowState.Minimized:
                        return BffWindowState.Minimized;
                    case WindowState.Maximized:
                        return BffWindowState.Maximized;
                    default:
                        throw new InvalidOperationException("The enum value is unknown!");
                }
            }
            set
            {
                switch (value)
                {
                    case BffWindowState.Normal:
                        Settings.Default.MainWindow_WindowState = WindowState.Normal;
                        break;
                    case BffWindowState.Minimized:
                        Settings.Default.MainWindow_WindowState = WindowState.Minimized;
                        break;
                    case BffWindowState.Maximized:
                        Settings.Default.MainWindow_WindowState = WindowState.Maximized;
                        break;
                    default:
                        throw new InvalidOperationException("The enum value is unknown!");
                }
            }
        }

        public void Save()
        {
            Settings.Default.Save();
        }
    }
}
