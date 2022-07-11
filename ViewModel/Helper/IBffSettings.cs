using BFF.Core.IoC;
using BFF.Model.Helper;
using System.Globalization;

namespace BFF.ViewModel.Helper
{
    public interface IBffSettings
    {
        string CsvBankStatementImportProfiles { get; set; }

        string OpenMainTab { get; set; }

        bool Culture_DefaultDateLong { get; set; }

        string? OpenAccountTab { get; set; }

        string Import_YnabCsvTransaction { get; set; }

        string Import_YnabCsvBudget { get; set; }

        string Import_SavePath { get; set; }

        bool ShowFlags { get; set; }

        bool ShowCheckNumbers { get; set; }

        bool NeverShowEditHeaders { get; set; }

        CultureInfo Culture_SessionCurrency { get; set; }

        CultureInfo Culture_SessionDate { get; set; }

        CultureInfo Culture_DefaultCurrency { get; set; }

        CultureInfo Culture_DefaultDate { get; set; }

        CultureInfo Culture_DefaultLanguage { get; set; }

        string? SelectedCsvProfile { get; set; }

        string DBLocation { get; set; }

        double MainWindow_Width { get; set; }

        double MainWindow_Height { get; set; }

        double MainWindow_X { get; set; }

        double MainWindow_Y { get; set; }

        BffWindowState MainWindow_WindowState { get; set; }
    }

    internal class BffSettingsProxy : IBffSettingsProxy, IContainerInstance
    {
        private readonly IBffSettings _settings;
        public string CsvBankStatementImportProfiles
        {
            get => _settings.CsvBankStatementImportProfiles;
            set => _settings.CsvBankStatementImportProfiles = value;
        }

        public string OpenMainTab
        {
            get => _settings.OpenMainTab;
            set => _settings.OpenMainTab = value;
        }

        public bool Culture_DefaultDateLong
        {
            get => _settings.Culture_DefaultDateLong;
            set => _settings.Culture_DefaultDateLong = value;
        }

        public string? OpenAccountTab
        {
            get => _settings.OpenAccountTab;
            set => _settings.OpenAccountTab = value;
        }

        public string Import_YnabCsvTransaction
        {
            get => _settings.Import_YnabCsvTransaction;
            set => _settings.Import_YnabCsvTransaction = value;
        }

        public string Import_YnabCsvBudget
        {
            get => _settings.Import_YnabCsvBudget;
            set => _settings.Import_YnabCsvBudget = value;
        }

        public string Import_SavePath
        {
            get => _settings.Import_SavePath;
            set => _settings.Import_SavePath = value;
        }

        public bool ShowFlags
        {
            get => _settings.ShowFlags;
            set => _settings.ShowFlags = value;
        }

        public bool ShowCheckNumbers
        {
            get => _settings.ShowCheckNumbers;
            set => _settings.ShowCheckNumbers = value;
        }

        public bool NeverShowEditHeaders
        {
            get => _settings.NeverShowEditHeaders;
            set => _settings.NeverShowEditHeaders = value;
        }

        public CultureInfo Culture_SessionCurrency
        {
            get => _settings.Culture_SessionCurrency;
            set => _settings.Culture_SessionCurrency = value;
        }

        public CultureInfo Culture_SessionDate
        {
            get => _settings.Culture_SessionDate;
            set => _settings.Culture_SessionDate = value;
        }

        public CultureInfo Culture_DefaultCurrency
        {
            get => _settings.Culture_DefaultCurrency;
            set => _settings.Culture_DefaultCurrency = value;
        }

        public CultureInfo Culture_DefaultDate
        {
            get => _settings.Culture_DefaultDate;
            set => _settings.Culture_DefaultDate = value;
        }

        public CultureInfo Culture_DefaultLanguage
        {
            get => _settings.Culture_DefaultLanguage;
            set => _settings.Culture_DefaultLanguage = value;
        }

        public string? SelectedCsvProfile
        {
            get => _settings.SelectedCsvProfile;
            set => _settings.SelectedCsvProfile = value;
        }

        public string DBLocation
        {
            get => _settings.DBLocation;
            set => _settings.DBLocation = value;
        }

        public double MainWindow_Width
        {
            get => _settings.MainWindow_Width;
            set => _settings.MainWindow_Width = value;
        }

        public double MainWindow_Height
        {
            get => _settings.MainWindow_Height;
            set => _settings.MainWindow_Height = value;
        }

        public double MainWindow_X
        {
            get => _settings.MainWindow_X;
            set => _settings.MainWindow_X = value;
        }

        public double MainWindow_Y
        {
            get => _settings.MainWindow_Y;
            set => _settings.MainWindow_Y = value;
        }

        public BffWindowState MainWindow_WindowState
        {
            get => _settings.MainWindow_WindowState;
            set => _settings.MainWindow_WindowState = value;
        }

        public BffSettingsProxy(IBffSettings settings)
        {
            _settings = settings;
        }
    }
}
