using System;
using System.IO;
using BFF.Model.ImportExport;
using BFF.ViewModel.Extensions;
using BFF.ViewModel.Helper;
using MrMeeseeks.Windows;
using System.Reactive.Disposables;
using System.Windows.Input;

namespace BFF.ViewModel.ViewModels.Import
{
    public class Ynab4CsvImportViewModel : ViewModelBase, IImportViewModel
    {
        private readonly Func<(string TransactionPath, string BudgetPath), IYnab4CsvImportConfiguration> _importingConfigurationFactory;
        private readonly CompositeDisposable _compositeDisposable = new();

        private string? _transactionPath;
        private string? _budgetPath;
        public ICommand BrowseTransactionCommand { get; }

        public ICommand BrowseBudgetCommand { get; }

        public string? TransactionPath
        {
            get => _transactionPath;
            set
            {
                if (_transactionPath == value) return;
                _transactionPath = value;
                OnPropertyChanged();
            }
        }

        public string? BudgetPath
        {
            get => _budgetPath;
            set
            {
                if (_budgetPath == value) return;
                _budgetPath = value;
                OnPropertyChanged();
            }
        }

        public Ynab4CsvImportViewModel(
            Func<IBffOpenFileDialog> openFileDialogFactory,
            Func<(string TransactionPath, string BudgetPath), IYnab4CsvImportConfiguration> importingConfigurationFactory,
            IBffSettings bffSettings)
        {
            _importingConfigurationFactory = importingConfigurationFactory;
            TransactionPath = bffSettings.Import_YnabCsvTransaction;
            BudgetPath = bffSettings.Import_YnabCsvBudget;

            BrowseTransactionCommand = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    _compositeDisposable,
                    () =>
                    {
                        var bffOpenFileDialog = openFileDialogFactory();
                        bffOpenFileDialog.Multiselect = false;
                        bffOpenFileDialog.DefaultExt = "csv";
                        bffOpenFileDialog.Filter = "YNAB Transaction Export(*.csv) | *.csv";
                        bffOpenFileDialog.FileName = TransactionPath;
                        if (bffOpenFileDialog.ShowDialog() == true)
                        {
                            TransactionPath = bffOpenFileDialog.FileName;
                            bffSettings.Import_YnabCsvTransaction =
                                _transactionPath ?? throw new FileNotFoundException();
                        }
                    });

            BrowseBudgetCommand = RxCommand
                .CanAlwaysExecute()
                .StandardCase(
                    _compositeDisposable,
                    () =>
                    {
                        var bffOpenFileDialog = openFileDialogFactory();
                        bffOpenFileDialog.Multiselect = false;
                        bffOpenFileDialog.DefaultExt = "csv";
                        bffOpenFileDialog.Filter = "YNAB Budget Export(*.csv) | *.csv";
                        bffOpenFileDialog.FileName = BudgetPath;
                        if (bffOpenFileDialog.ShowDialog() == true)
                        {
                            BudgetPath = bffOpenFileDialog.FileName;
                            bffSettings.Import_YnabCsvBudget = _budgetPath ?? throw new FileNotFoundException();
                        }
                    });
        }

        public IImportConfiguration GenerateConfiguration() =>
            _importingConfigurationFactory((TransactionPath ?? throw new FileNotFoundException(), BudgetPath ?? throw new FileNotFoundException()));

        public void Dispose() => _compositeDisposable.Dispose();
    }
}
