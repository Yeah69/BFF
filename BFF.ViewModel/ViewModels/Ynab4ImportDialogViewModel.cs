using System;
using Autofac.Features.OwnedInstances;
using BFF.Core.Helper;
using BFF.Core.Persistence;
using BFF.Model.Contexts;
using BFF.ViewModel.Helper;

namespace BFF.ViewModel.ViewModels
{
    public interface IImportDialogViewModel : IViewModel
    {
        IRxRelayCommand BrowseYnabCsvTransactionCommand { get; }

        IRxRelayCommand BrowseYnabCsvBudgetCommand { get; }

        IRxRelayCommand BrowseSaveCommand { get; }

        string TransactionPath { get; set; }

        string BudgetPath { get; set; }

        string SavePath { get; set; }

        void Import();
    }

    public class Ynab4ImportDialogViewModel : ViewModelBase, IImportDialogViewModel
    {
        private readonly Func<Owned<Func<IImportingConfiguration, IPersistenceConfiguration, IImportProxyContext>>> _ownedImportContextFactory;
        private readonly Func<(string TransactionPath, string BudgetPath, string SavePath), IYnab4ImportConfiguration> _importingConfigurationFactory;
        private readonly Func<string, ISqlitePersistenceConfiguration> _persistenceConfigurationFactory;

        private string _transactionPath;
        private string _budgetPath;
        private string _savePath;
        public IRxRelayCommand BrowseYnabCsvTransactionCommand { get; }

        public IRxRelayCommand BrowseYnabCsvBudgetCommand { get; }

        public IRxRelayCommand BrowseSaveCommand { get; }

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
        public string SavePath
        {
            get => _savePath;
            set
            {
                if (_savePath == value) return;
                _savePath = value;
                OnPropertyChanged();
            }
        }

        public void Import()
        {
            using (var ownedImportContextFactory = _ownedImportContextFactory())
            {
                var importingConfiguration = _importingConfigurationFactory((_transactionPath, _budgetPath, _savePath));
                var persistenceConfiguration = _persistenceConfigurationFactory(_savePath);
                var importContextProxyFactory = ownedImportContextFactory.Value;
                var importContextProxy = importContextProxyFactory(
                    importingConfiguration,
                    persistenceConfiguration);
                importContextProxy.ImportProxy.Import();
            }
        }

        public Ynab4ImportDialogViewModel(
            Func<Owned<Func<IImportingConfiguration, IPersistenceConfiguration, IImportProxyContext>>> ownedImportContextFactory,
            Func<IBffOpenFileDialog> openFileDialogFactory,
            Func<IBffSaveFileDialog> saveFileDialogFactory,
            Func<(string TransactionPath, string BudgetPath, string SavePath), IYnab4ImportConfiguration> importingConfigurationFactory,
            Func<string, ISqlitePersistenceConfiguration> persistenceConfigurationFactory,
            IBffSettings bffSettings)
        {
            TransactionPath = bffSettings.Import_YnabCsvTransaction;
            BudgetPath = bffSettings.Import_YnabCsvBudget;
            SavePath = bffSettings.Import_SavePath;
            _ownedImportContextFactory = ownedImportContextFactory;
            _importingConfigurationFactory = importingConfigurationFactory;
            _persistenceConfigurationFactory = persistenceConfigurationFactory;
            BrowseYnabCsvTransactionCommand = new RxRelayCommand(() =>
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
                    bffSettings.Save();
                }
            });

            BrowseYnabCsvBudgetCommand = new RxRelayCommand(() =>
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
                    bffSettings.Save();
                }
            });

            BrowseSaveCommand = new RxRelayCommand(() =>
            {
                var bffSaveFileDialog = saveFileDialogFactory();
                bffSaveFileDialog.DefaultExt = "sqlite";
                bffSaveFileDialog.Filter = "SQLite Budget Plan (*.sqlite)|*.sqlite";
                bffSaveFileDialog.FileName = SavePath;
                if (bffSaveFileDialog.ShowDialog() == true)
                {
                    SavePath = bffSaveFileDialog.FileName;
                    bffSettings.Import_SavePath = _savePath;
                    bffSettings.Save();
                }
            });
        }
    }
}
