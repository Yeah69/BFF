using System;
using System.Threading.Tasks;
using BFF.Core.Persistence;

namespace BFF.ViewModel.ViewModels.Import
{
    public interface IImportDialogViewModel : IViewModel
    {
        ImportOption ImportOption { get; set; }
        ExportOption ExportOption { get; set; }
        IImportViewModel ImportViewModel { get; }
        IExportViewModel ExportViewModel { get; }
        Task Import();
    }

    internal class ImportDialogViewModel : ViewModelBase, IImportDialogViewModel
    {
        private readonly IImportExport _importExport;
        private readonly Func<Ynab4CsvImportViewModel> _ynab4CsvImportViewModelFactory;
        private readonly Func<RealmFileExportViewModel> _realmFileExportViewModel;
        private ImportOption _importOption;
        private ExportOption _exportOption;
        private IImportViewModel _importViewModel;
        private IExportViewModel _exportViewModel;

        public ImportOption ImportOption
        {
            get => _importOption;
            set
            {
                if (value == _importOption) return;
                _importOption = value;
                OnPropertyChanged();
                ImportViewModel = UpdateImportViewModel();
            }
        }

        public ExportOption ExportOption
        {
            get => _exportOption;
            set
            {
                if (value == _exportOption) return;
                _exportOption = value;
                OnPropertyChanged();
                ExportViewModel = UpdateExportViewModel();
            }
        }

        public IImportViewModel ImportViewModel
        {
            get => _importViewModel;
            private set
            {
                if (value == _importViewModel) return;
                _importViewModel = value;
                OnPropertyChanged();
            }
        }

        public IExportViewModel ExportViewModel
        {
            get => _exportViewModel;
            private set
            {
                if (value == _exportViewModel) return;
                _exportViewModel = value;
                OnPropertyChanged();
            }
        }


        public Task Import()
        {
            return _importExport.ImportExportAsync(
                ImportViewModel.GenerateConfiguration(),
                ExportViewModel.GenerateConfiguration());
        }

        public ImportDialogViewModel(
            IImportExport importExport,
            Func<Ynab4CsvImportViewModel> ynab4CsvImportViewModelFactory,
            Func<RealmFileExportViewModel> realmFileExportViewModel)
        {
            _importExport = importExport;
            _ynab4CsvImportViewModelFactory = ynab4CsvImportViewModelFactory;
            _realmFileExportViewModel = realmFileExportViewModel;

            _importViewModel = UpdateImportViewModel();
            _exportViewModel = UpdateExportViewModel();
        }

        private IImportViewModel UpdateImportViewModel()
        {
            return ImportOption switch
                {
                    ImportOption.Ynab4Csv => _ynab4CsvImportViewModelFactory(),
                    _ => throw new ArgumentException("Unexpected import option!")
                };
        }

        private IExportViewModel UpdateExportViewModel()
        {
            return ExportOption switch
                {
                ExportOption.RealmFile => _realmFileExportViewModel(),
                _ => throw new ArgumentException("Unexpected export option!")
                };
        }
    }
}
