using BFF.Helper.Import;
using Microsoft.Win32;

namespace BFF.MVVM.ViewModels
{
    public interface IImportDialogViewModel : IViewModel
    {
        IImportable Importable { get; }

        IRxRelayCommand BrowseYnabCsvTransactionCommand { get; }

        IRxRelayCommand BrowseYnabCsvBudgetCommand { get; }

        IRxRelayCommand BrowseSaveCommand { get; }
    }

    public class ImportDialogViewModel : ViewModelBase, IImportDialogViewModel
    {
        public IImportable Importable { get; }

        public IRxRelayCommand BrowseYnabCsvTransactionCommand { get; }

        public IRxRelayCommand BrowseYnabCsvBudgetCommand { get; }

        public IRxRelayCommand BrowseSaveCommand { get; }

        public ImportDialogViewModel(IImportable importable)
        {
            Importable = importable;

            BrowseYnabCsvTransactionCommand = new RxRelayCommand(() =>
            {
                OpenFileDialog openFileDialog =
                    new OpenFileDialog
                    {
                        Multiselect = false,
                        DefaultExt = "csv",
                        Filter = "YNAB Transaction Export (*.csv)|*.csv",
                        FileName = ((YnabCsvImport) Importable).TransactionPath
                    };
                if (openFileDialog.ShowDialog() == true)
                {
                    ((YnabCsvImport) Importable).TransactionPath = openFileDialog.FileName;
                }
            });

            BrowseYnabCsvBudgetCommand = new RxRelayCommand(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Multiselect = false,
                    DefaultExt = "csv",
                    Filter = "YNAB Transaction Export (*.csv)|*.csv",
                    FileName = ((YnabCsvImport)Importable).BudgetPath
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    ((YnabCsvImport)Importable).BudgetPath = openFileDialog.FileName;
                }
            });

            BrowseSaveCommand = new RxRelayCommand(() =>
            {
                SaveFileDialog saveFileDialog =
                    new SaveFileDialog
                    {
                        DefaultExt = "sqlite",
                        Filter = "SQLite Budget Plan (*.sqlite)|*.sqlite",
                        FileName = Importable.SavePath
                    };
                if (saveFileDialog.ShowDialog() == true)
                {
                    Importable.SavePath = saveFileDialog.FileName;
                }
            });
        }
    }
}
