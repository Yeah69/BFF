using BFF.Model.ImportExport;
using System;
using System.IO;
using BFF.ViewModel.Helper;
using BFF.ViewModel.ViewModels.Dialogs;

namespace BFF.ViewModel.ViewModels.Import
{
    public class RealmFileExportViewModel : ViewModelBase, IExportViewModel
    {
        private readonly Func<(string Path, string? Password), IRealmProjectFileAccessConfiguration> _exportingConfigurationFactory;
        private string? _path;

        public string? Path
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
            Func<(string Path, string? Password), IRealmProjectFileAccessConfiguration> exportingConfigurationFactory,
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
                    bffSettings.Import_SavePath = _path ?? throw new FileNotFoundException();
                }
            });
        }

        public RxRelayCommand BrowseCommand { get; set; }

        public IExportConfiguration GenerateConfiguration() =>
            _exportingConfigurationFactory((
                Path ?? throw new FileNotFoundException(), 
                PasswordConfiguration.IsEncryptionActive 
                    ? PasswordConfiguration.Password 
                    : null));
    }
}
