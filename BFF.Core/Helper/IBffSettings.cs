using BFF.Core.IoC;

namespace BFF.Core.Helper
{
    public interface IBffSettings : IOncePerApplication
    {
        string CsvBankStatementImportProfiles { get; set; }

        void Save();
    }
}
