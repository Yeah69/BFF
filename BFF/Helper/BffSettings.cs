using BFF.Core.Helper;
using BFF.Properties;

namespace BFF.Helper
{
    internal class BffSettings : IBffSettings
    {
        public string CsvBankStatementImportProfiles
        {
            get => Settings.Default.CsvBankStatementImportProfiles;
            set => Settings.Default.CsvBankStatementImportProfiles = value;
        }
        public void Save()
        {
            Settings.Default.Save();
        }
    }
}
