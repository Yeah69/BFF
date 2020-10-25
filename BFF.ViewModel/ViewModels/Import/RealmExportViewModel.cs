using System;
using BFF.Core.Helper;
using BFF.Core.Persistence;
using BFF.ViewModel.Helper;
using BFF.ViewModel.ViewModels.Dialogs;

namespace BFF.ViewModel.ViewModels.Import
{
    public class RealmFileExportViewModel : ViewModelBase, IExportViewModel
    {
        private readonly Func<(string Path, string Password), IRealmExportConfiguration> _exportingConfigurationFactory;
        private string _path;

        public string Path
        {
            get => _path;
            set
            {
                if (value == _path) return;
                _path = value;
                OnPropertyChanged();
            }
        }

        public PasswordProtectedFileAccessViewModel PasswordConfiguration { get; }

        public RealmFileExportViewModel(
            PasswordProtectedFileAccessViewModel passwordProtectedFileAccessViewModel,
            Func<IBffSaveFileDialog> saveFileDialogFactory,
            Func<(string Path, string Password), IRealmExportConfiguration> exportingConfigurationFactory,
            IBffSettings bffSettings)
        {
            PasswordConfiguration = passwordProtectedFileAccessViewModel;
            _exportingConfigurationFactory = exportingConfigurationFactory;

            Path = bffSettings.Import_SavePath;

            BrowseCommand = new RxRelayCommand(() =>
            {
                var bffSaveFileDialog = saveFileDialogFactory();
                bffSaveFileDialog.DefaultExt = "realm";
                bffSaveFileDialog.Filter = "Realm Budget Plan (*.realm)|*.realm";
                bffSaveFileDialog.FileName = Path;
                if (bffSaveFileDialog.ShowDialog() == true)
                {
                    Path = bffSaveFileDialog.FileName;
                    bffSettings.Import_SavePath = _path;
                }
            });
        }

        public RxRelayCommand BrowseCommand { get; set; }

        public IExportingConfiguration GenerateConfiguration() =>
            _exportingConfigurationFactory((
                Path, 
                PasswordConfiguration.IsEncryptionActive 
                    ? PasswordConfiguration.Password 
                    : null));
    }
}
