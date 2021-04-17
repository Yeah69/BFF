using System.Collections.Generic;
using BFF.Model.Models.Utility;

namespace BFF.Model.Helper
{
    public interface IManageCsvBankStatementImportProfiles
    {
        IReadOnlyList<ICsvBankStatementImportProfile> LoadProfiles();

        void SaveProfiles(IReadOnlyList<ICsvBankStatementImportProfile> profiles);
    }
}