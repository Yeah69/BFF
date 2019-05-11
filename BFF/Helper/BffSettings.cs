using System;
using System.Globalization;
using System.Windows;
using BFF.Core.Helper;
using BFF.Core.IoC;
using BFF.Properties;

namespace BFF.Helper
{
    internal class BffSettings : IBffSettings, IOncePerApplication
    {
        public string CsvBankStatementImportProfiles
        {
            get => Settings.Default.CsvBankStatementImportProfiles;
            set
            {
                Settings.Default.CsvBankStatementImportProfiles = value;
                Save();
            }
        }

        public string OpenMainTab
        {
            get => Settings.Default.OpenMainTab;
            set
            {
                Settings.Default.OpenMainTab = value;
                Save();
            }
        }

        public bool Culture_DefaultDateLong
        {
            get => Settings.Default.Culture_DefaultDateLong;
            set
            {
                Settings.Default.Culture_DefaultDateLong = value;
                Save();
            }
        }

        public string OpenAccountTab
        {
            get => Settings.Default.OpenAccountTab;
            set
            {
                Settings.Default.OpenAccountTab = value;
                Save();
            }
        }

        public string Import_YnabCsvTransaction
        {
            get => Settings.Default.Import_YnabCsvTransaction;
            set
            {
                Settings.Default.Import_YnabCsvTransaction = value;
                Save();
            }
        }

        public string Import_YnabCsvBudget
        {
            get => Settings.Default.Import_YnabCsvBudget;
            set
            {
                Settings.Default.Import_YnabCsvBudget = value;
                Save();
            }
        }

        public string Import_SavePath
        {
            get => Settings.Default.Import_SavePath;
            set
            {
                Settings.Default.Import_SavePath = value;
                Save();
            }
        }

        public bool ShowFlags
        {
            get => Settings.Default.ShowFlags;
            set
            {
                Settings.Default.ShowFlags = value;
                Save();
            }
        }

        public bool ShowCheckNumbers
        {
            get => Settings.Default.ShowCheckNumbers;
            set
            {
                Settings.Default.ShowCheckNumbers = value;
                Save();
            }
        }

        public bool NeverShowEditHeaders
        {
            get => Settings.Default.NeverShowEditHeaders;
            set
            {
                Settings.Default.NeverShowEditHeaders = value;
                Save();
            }
        }

        public CultureInfo Culture_SessionCurrency
        {
            get => Settings.Default.Culture_SessionCurrency;
            set
            {
                Settings.Default.Culture_SessionCurrency = value;
                Save();
            }
        }

        public CultureInfo Culture_SessionDate
        {
            get => Settings.Default.Culture_SessionDate;
            set
            {
                Settings.Default.Culture_SessionDate = value;
                Save();
            }
        }

        public CultureInfo Culture_DefaultCurrency
        {
            get => Settings.Default.Culture_DefaultCurrency;
            set
            {
                Settings.Default.Culture_DefaultCurrency = value;
                Save();
            }
        }

        public CultureInfo Culture_DefaultDate
        {
            get => Settings.Default.Culture_DefaultDate;
            set
            {
                Settings.Default.Culture_DefaultDate = value;
                Save();
            }
        }

        public CultureInfo Culture_DefaultLanguage
        {
            get => Settings.Default.Culture_DefaultLanguage;
            set
            {
                Settings.Default.Culture_DefaultLanguage = value;
                Save();
            }
        }

        public string SelectedCsvProfile
        {
            get => Settings.Default.SelectedCsvProfile;
            set
            {
                Settings.Default.SelectedCsvProfile = value;
                Save();
            }
        }

        public string DBLocation
        {
            get => Settings.Default.DBLocation;
            set
            {
                Settings.Default.DBLocation = value;
                Save();
            }
        }

        public double MainWindow_Width
        {
            get => Settings.Default.MainWindow_Width;
            set
            {
                Settings.Default.MainWindow_Width = value;
                Save();
            }
        }

        public double MainWindow_Height
        {
            get => Settings.Default.MainWindow_Height;
            set
            {
                Settings.Default.MainWindow_Height = value;
                Save();
            }
        }

        public double MainWindow_X
        {
            get => Settings.Default.MainWindow_X;
            set
            {
                Settings.Default.MainWindow_X = value;
                Save();
            }
        }

        public double MainWindow_Y
        {
            get => Settings.Default.MainWindow_Y;
            set
            {
                Settings.Default.MainWindow_Y = value;
                Save();
            }
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
                Save();
            }
        }

        private void Save()
        {
            Settings.Default.Save();
        }
    }
}
