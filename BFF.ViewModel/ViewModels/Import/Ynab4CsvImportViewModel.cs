using System;
using BFF.Core.Helper;
using BFF.Core.Persistence;
using BFF.ViewModel.Helper;
using JetBrains.Annotations;

namespace BFF.ViewModel.ViewModels.Import
{
    internal class Ynab4CsvImportViewModel : ViewModelBase, IImportViewModel
    {
        private readonly Func<(string TransactionPath, string BudgetPath), IYnab4CsvImportConfiguration> _importingConfigurationFactory;

        private string _transactionPath;
        private string _budgetPath;
        public IRxRelayCommand BrowseTransactionCommand { get; }

        public IRxRelayCommand BrowseBudgetCommand { get; }

        public string TransactionPath
        {
            get => _transactionPath;
            set
            {
                if (_transactionPath == value) return;
                _transactionPath = value;
                OnPropertyChanged();
            }
        }

        public string BudgetPath
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
            [NotNull] Func<IBffOpenFileDialog> openFileDialogFactory,
            [NotNull] Func<(string TransactionPath, string BudgetPath), IYnab4CsvImportConfiguration> importingConfigurationFactory,
            [NotNull] IBffSettings bffSettings)
        {
            openFileDialogFactory = openFileDialogFactory ?? throw new ArgumentNullException(nameof(openFileDialogFactory));
            importingConfigurationFactory = importingConfigurationFactory ?? throw new ArgumentNullException(nameof(importingConfigurationFactory));
            bffSettings = bffSettings ?? throw new ArgumentNullException(nameof(bffSettings));

            _importingConfigurationFactory = importingConfigurationFactory;
            TransactionPath = bffSettings.Import_YnabCsvTransaction;
            BudgetPath = bffSettings.Import_YnabCsvBudget;
            BrowseTransactionCommand = new RxRelayCommand(() =>
            {
                var bffOpenFileDialog = openFileDialogFactory();
                bffOpenFileDialog.Multiselect = false;
                bffOpenFileDialog.DefaultExt = "csv";
                bffOpenFileDialog.Filter = "YNAB Transaction Export(*.csv) | *.csv";
                bffOpenFileDialog.FileName = TransactionPath;
                if (bffOpenFileDialog.ShowDialog() == true)
                {
                    TransactionPath = bffOpenFileDialog.FileName;
                    bffSettings.Import_YnabCsvTransaction = _transactionPath;
                }
            });

            BrowseBudgetCommand = new RxRelayCommand(() =>
            {
                var bffOpenFileDialog = openFileDialogFactory();
                bffOpenFileDialog.Multiselect = false;
                bffOpenFileDialog.DefaultExt = "csv";
                bffOpenFileDialog.Filter = "YNAB Budget Export(*.csv) | *.csv";
                bffOpenFileDialog.FileName = BudgetPath;
                if (bffOpenFileDialog.ShowDialog() == true)
                {
                    BudgetPath = bffOpenFileDialog.FileName;
                    bffSettings.Import_YnabCsvBudget = _budgetPath;
                }
            });
        }

        public IImportingConfiguration GenerateConfiguration() =>
            _importingConfigurationFactory((TransactionPath, BudgetPath));
    }
}
