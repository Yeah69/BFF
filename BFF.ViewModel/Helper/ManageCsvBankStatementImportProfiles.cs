using System.Collections.Generic;
using BFF.Model.Helper;
using BFF.Model.Models.Utility;
using Newtonsoft.Json;

namespace BFF.ViewModel.Helper
{
    internal class ManageCsvBankStatementImportProfiles : IManageCsvBankStatementImportProfiles
    {
        private readonly IBffSettings _bffSettings;

        public ManageCsvBankStatementImportProfiles(IBffSettings bffSettings)
        {
            _bffSettings = bffSettings;
        }

        public IReadOnlyList<ICsvBankStatementImportProfile> LoadProfiles() => 
            JsonConvert.DeserializeObject<List<ICsvBankStatementImportProfile>>(_bffSettings.CsvBankStatementImportProfiles);

        public void SaveProfiles(IReadOnlyList<ICsvBankStatementImportProfile> profiles) => 
            _bffSettings.CsvBankStatementImportProfiles = JsonConvert.SerializeObject(profiles);
    }
}