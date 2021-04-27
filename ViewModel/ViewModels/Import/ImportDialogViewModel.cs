using BFF.Core.IoC;
using BFF.Model.Contexts;
using BFF.Model.ImportExport;
using MrMeeseeks.Reactive.Extensions;
using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace BFF.ViewModel.ViewModels.Import
{
    public interface IImportDialogViewModel : IViewModel, ITransient
    {
        ImportOption ImportOption { get; set; }
        ExportOption ExportOption { get; set; }
        IImportViewModel ImportViewModel { get; }
        IExportViewModel ExportViewModel { get; }
        Task Import();
    }

    internal class ImportDialogViewModel : ViewModelBase, IImportDialogViewModel
    {
        private readonly Func<IImportConfiguration, IImportContext> _importContextFactory;
        private readonly Func<IExportConfiguration, IExportContext> _exportContextFactory;
        private readonly SerialDisposable _importDisposable = new();
        private readonly SerialDisposable _exportDisposable = new();
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


        public async Task Import()
        {
            var importContext = _importContextFactory(ImportViewModel.GenerateConfiguration());
            var exportContext = _exportContextFactory(ExportViewModel.GenerateConfiguration());
            await exportContext.Export(
                await importContext.Import().ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        public ImportDialogViewModel(
            Func<IImportConfiguration, IImportContext> importContextFactory,
            Func<IExportConfiguration, IExportContext> exportContextFactory,
            Func<Ynab4CsvImportViewModel> ynab4CsvImportViewModelFactory,
            Func<RealmFileExportViewModel> realmFileExportViewModel)
        {
            _importContextFactory = importContextFactory;
            _exportContextFactory = exportContextFactory;
            _ynab4CsvImportViewModelFactory = ynab4CsvImportViewModelFactory;
            _realmFileExportViewModel = realmFileExportViewModel;

            _importViewModel = UpdateImportViewModel();
            _exportViewModel = UpdateExportViewModel();
        }

        private IImportViewModel UpdateImportViewModel()
        {
            return ImportOption switch
                {
                    ImportOption.Ynab4Csv => _ynab4CsvImportViewModelFactory().SerializeDisposalWith(_importDisposable),
                    _ => throw new ArgumentException("Unexpected import option!")
                };
        }

        private IExportViewModel UpdateExportViewModel()
        {
            return ExportOption switch
                {
                ExportOption.RealmFile => _realmFileExportViewModel().SerializeDisposalWith(_exportDisposable),
                _ => throw new ArgumentException("Unexpected export option!")
                };
        }

        public void Dispose()
        {
            _importDisposable.Dispose();
            _exportDisposable.Dispose();
        }
    }
}
