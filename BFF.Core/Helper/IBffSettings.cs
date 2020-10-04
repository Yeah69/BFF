using System.Globalization;

namespace BFF.Core.Helper
{
    public interface IBffSettings
    {
        string CsvBankStatementImportProfiles { get; set; }

        string OpenMainTab { get; set; }

        bool Culture_DefaultDateLong { get; set; }

        string OpenAccountTab { get; set; }

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

        string SelectedCsvProfile { get; set; }

        string DBLocation { get; set; }

        double MainWindow_Width { get; set; }

        double MainWindow_Height { get; set; }

        double MainWindow_X { get; set; }

        double MainWindow_Y { get; set; }

        BffWindowState MainWindow_WindowState { get; set; }
    }
}
