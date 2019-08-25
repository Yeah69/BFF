using System;
using BFF.Core.Helper;
using BFF.Core.Persistence;
using BFF.ViewModel.Helper;

namespace BFF.ViewModel.ViewModels.Import
{
    internal class RealmFileExportViewModel : ViewModelBase, IExportViewModel
    {
        private readonly Func<(string Path, string Password), IRealmExportConfiguration> _exportingConfigurationFactory;
        private string _password;
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

        public string Password
        {
            get => _password;
            set
            {
                if (value == _password) return;
                _password = value;
                OnPropertyChanged();
            }
        }

        public RealmFileExportViewModel(
            Func<IBffSaveFileDialog> saveFileDialogFactory,
            Func<(string Path, string Password), IRealmExportConfiguration> _exportingConfigurationFactory,
            IBffSettings bffSettings)
        {
            this._exportingConfigurationFactory = _exportingConfigurationFactory;

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
            _exportingConfigurationFactory((Path, Password));
    }
}
