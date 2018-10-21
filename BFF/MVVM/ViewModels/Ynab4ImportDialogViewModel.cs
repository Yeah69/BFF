using System;
using Autofac.Features.OwnedInstances;
using BFF.Core.Persistence;
using BFF.Model;
using BFF.Model.Contexts;
using BFF.Properties;
using Microsoft.Win32;

namespace BFF.MVVM.ViewModels
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
            Func<(string TransactionPath, string BudgetPath, string SavePath), IYnab4ImportConfiguration> importingConfigurationFactory,
            Func<string, ISqlitePersistenceConfiguration> persistenceConfigurationFactory)
        {
            TransactionPath = Settings.Default.Import_YnabCsvTransaction;
            BudgetPath = Settings.Default.Import_YnabCsvBudget;
            SavePath = Settings.Default.Import_SavePath;
            _ownedImportContextFactory = ownedImportContextFactory;
            _importingConfigurationFactory = importingConfigurationFactory;
            _persistenceConfigurationFactory = persistenceConfigurationFactory;
            BrowseYnabCsvTransactionCommand = new RxRelayCommand(() =>
            {
                OpenFileDialog openFileDialog =
                    new OpenFileDialog
                    {
                        Multiselect = false,
                        DefaultExt = "csv",
                        Filter = "YNAB Transaction Export (*.csv)|*.csv",
                        FileName = TransactionPath
                    };
                if (openFileDialog.ShowDialog() == true)
                {
                    TransactionPath = openFileDialog.FileName;
                    Settings.Default.Import_YnabCsvTransaction = _transactionPath;
                    Settings.Default.Save();
                }
            });

            BrowseYnabCsvBudgetCommand = new RxRelayCommand(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Multiselect = false,
                    DefaultExt = "csv",
                    Filter = "YNAB Transaction Export (*.csv)|*.csv",
                    FileName = BudgetPath
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    BudgetPath = openFileDialog.FileName;
                    Settings.Default.Import_YnabCsvBudget = _budgetPath;
                    Settings.Default.Save();
                }
            });

            BrowseSaveCommand = new RxRelayCommand(() =>
            {
                SaveFileDialog saveFileDialog =
                    new SaveFileDialog
                    {
                        DefaultExt = "sqlite",
                        Filter = "SQLite Budget Plan (*.sqlite)|*.sqlite",
                        FileName = SavePath
                    };
                if (saveFileDialog.ShowDialog() == true)
                {
                    SavePath = saveFileDialog.FileName;
                    Settings.Default.Import_SavePath = _savePath;
                    Settings.Default.Save();
                }
            });
        }
    }
}
