using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using BFF.Core.IoC;
using BFF.Model.Helper;

namespace BFF.Model.Models.Utility
{
    public interface ICsvBankStatementProfileManager
    {
        ReadOnlyObservableCollection<ICsvBankStatementImportProfile> Profiles { get; }

        void Save();

        void Remove(string name);
    }
    
    public interface ICreateCsvBankStatementImportProfile
    {
        ICsvBankStatementImportProfile Create(
            string header,
            char delimiter,
            string? dateSegment,
            string dateFormat,
            string payeeFormat,
            bool shouldCreateNewPayeeIfNotExisting,
            string memoFormat,
            string sumFormat,
            string sumLocalization,
            string name);
    }

    public class CsvBankStatementProfileManager : ICsvBankStatementProfileManager, ICreateCsvBankStatementImportProfile, IContainerInstance
    {
        private readonly IManageCsvBankStatementImportProfiles _manageCsvBankStatementImportProfiles;
        private readonly ObservableCollection<ICsvBankStatementImportProfile> _profiles;

        public CsvBankStatementProfileManager(IManageCsvBankStatementImportProfiles manageCsvBankStatementImportProfiles)
        {
            _manageCsvBankStatementImportProfiles = manageCsvBankStatementImportProfiles;
            var csvBankStatementImportProfiles = _manageCsvBankStatementImportProfiles.LoadProfiles();
            _profiles = new ObservableCollection<ICsvBankStatementImportProfile>(csvBankStatementImportProfiles);
            Profiles = new ReadOnlyObservableCollection<ICsvBankStatementImportProfile>(_profiles);
            (Profiles as INotifyCollectionChanged).CollectionChanged += (sender, args) => { };
        }

        public ReadOnlyObservableCollection<ICsvBankStatementImportProfile> Profiles { get; }

        public void Save()
        {
            _manageCsvBankStatementImportProfiles.SaveProfiles(_profiles);
        }

        public void Remove(string name)
        {
            if (_profiles.FirstOrDefault(p => p.Name == name) is { } profile)
            {
                _profiles.Remove(profile);
                Save();
            }
        }

        public ICsvBankStatementImportProfile Create(
            string header, 
            char delimiter,
            string? dateSegment,
            string dateFormat, 
            string payeeFormat,
            bool shouldCreateNewPayeeIfNotExisting,
            string memoFormat, 
            string sumFormat,
            string sumLocalization,
            string name)
        {
            if (_profiles.Any(p => p.Name == name)) throw new Exception();

            var newProfile = new CsvBankStatementImportProfile(
                header,
                delimiter,
                dateSegment,
                dateFormat,
                payeeFormat,
                shouldCreateNewPayeeIfNotExisting,
                memoFormat,
                sumFormat,
                sumLocalization,
                name);

            _profiles.Add(newProfile);

            Save();

            return newProfile;
        }
    }
}
