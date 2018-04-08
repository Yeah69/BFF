using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using BFF.DB;
using BFF.MVVM.Models.Native.Utility;
using BFF.Properties;
using Newtonsoft.Json;

namespace BFF.MVVM.Managers
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
            string dateFormat,
            string payeeFormat,
            bool shouldCreateNewPayeeIfNotExisting,
            string memoFormat,
            string sumFormat,
            string name);
    }

    public class CsvBankStatementProfileManager : ICsvBankStatementProfileManager, ICreateCsvBankStatementImportProfile, IOncePerApplication
    {
        private readonly ObservableCollection<ICsvBankStatementImportProfile> _profiles;

        public CsvBankStatementProfileManager()
        {
            var csvBankStatementImportProfiles = JsonConvert.DeserializeObject<List<CsvBankStatementImportProfile>>(Settings.Default.CsvBankStatementImportProfiles );
            _profiles = new ObservableCollection<ICsvBankStatementImportProfile>(csvBankStatementImportProfiles);
            Profiles = new ReadOnlyObservableCollection<ICsvBankStatementImportProfile>(_profiles);
            (Profiles as INotifyCollectionChanged).CollectionChanged += (sender, args) => { };
        }

        public ReadOnlyObservableCollection<ICsvBankStatementImportProfile> Profiles { get; }

        public void Save()
        {
            Settings.Default.CsvBankStatementImportProfiles = JsonConvert.SerializeObject(_profiles.ToList());
            Settings.Default.Save();
        }

        public void Remove(string name)
        {
            if (_profiles.FirstOrDefault(p => p.Name == name) is ICsvBankStatementImportProfile profile)
            {
                _profiles.Remove(profile);
                Save();
            }
        }

        public ICsvBankStatementImportProfile Create(
            string header, 
            char delimiter,
            string dateFormat, 
            string payeeFormat,
            bool shouldCreateNewPayeeIfNotExisting,
            string memoFormat, 
            string sumFormat,
            string name)
        {
            if (_profiles.Any(p => p.Name == name)) throw new Exception();

            var newProfile = new CsvBankStatementImportProfile(
                header,
                delimiter,
                dateFormat,
                payeeFormat,
                shouldCreateNewPayeeIfNotExisting,
                memoFormat,
                sumFormat,
                name);

            _profiles.Add(newProfile);

            Save();

            return newProfile;
        }
    }
}
