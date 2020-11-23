using System;
using System.IO;
using BFF.Model.ImportExport;
using BFF.ViewModel.Helper;

namespace BFF.ViewModel.ViewModels.Import
{
    public class SqliteProjectImportViewModel : ViewModelBase, IImportViewModel
    {
        private readonly Func<string, ISqliteImportConfiguration> _importingConfigurationFactory;
        private string? _path;
        
        public IRxRelayCommand BrowseCommand { get; }

        public string? Path
        {
            get => _path;
            set
            {
                if (_path == value) return;
                _path = value;
                OnPropertyChanged();
            }
        }

        public SqliteProjectImportViewModel(
            Func<IBffOpenFileDialog> openFileDialogFactory,
            Func<string, ISqliteImportConfiguration> importingConfigurationFactory)
        {
            _importingConfigurationFactory = importingConfigurationFactory;
            BrowseCommand = new RxRelayCommand(() =>
            {
                var bffOpenFileDialog = openFileDialogFactory();
                bffOpenFileDialog.Multiselect = false;
                bffOpenFileDialog.DefaultExt = "sqlite";
                bffOpenFileDialog.Filter = "BFF Sqlite Project (*.sqlite) | *.sqlite";
                if (bffOpenFileDialog.ShowDialog() == true)
                    Path = bffOpenFileDialog.FileName;
            });
        }

        public IImportingConfiguration GenerateConfiguration() =>
            _importingConfigurationFactory(Path ?? throw new FileNotFoundException());
    }
}
