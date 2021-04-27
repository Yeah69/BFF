using BFF.Model.ImportExport;
using BFF.ViewModel.Extensions;
using System;
using System.IO;
using BFF.ViewModel.Helper;
using BFF.ViewModel.ViewModels.Dialogs;
using MrMeeseeks.Windows;
using System.Reactive.Disposables;
using System.Windows.Input;

namespace BFF.ViewModel.ViewModels.Import
{
    public class RealmFileExportViewModel : ViewModelBase, IExportViewModel
    {
        private readonly Func<(string Path, string? Password), IRealmProjectFileAccessConfiguration> _exportingConfigurationFactory;
        private readonly CompositeDisposable _compositeDisposable = new();
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

            BrowseCommand = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    _compositeDisposable,
                    () =>
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

        public ICommand BrowseCommand { get; set; }

        public IExportConfiguration GenerateConfiguration() =>
            _exportingConfigurationFactory((
                Path ?? throw new FileNotFoundException(), 
                PasswordConfiguration.IsEncryptionActive 
                    ? PasswordConfiguration.Password 
                    : null));

        public void Dispose() => _compositeDisposable.Dispose();
    }
}
