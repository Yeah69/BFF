using System.IO;
using System.Windows.Input;
using BFF.Helper.Import;
using BFF.WPFStuff;
using Microsoft.Win32;

namespace BFF.ViewModel
{
    class ImportDialogViewModel : ObservableObject
    {
        public IImportable Importable { get; set; }

        public ICommand BrowseYnabCsvTransactionCommand => new RelayCommand(param => BrowseYnabCsvTransaction(), param => true);
        public ICommand BrowseYnabCsvBudgetCommand => new RelayCommand(param => BrowseYnabCsvBudget(), param => true);
        public ICommand BrowseSaveCommand => new RelayCommand(param => BrowseSave(), param => true);

        private void BrowseYnabCsvTransaction()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Multiselect = false, DefaultExt = "csv", Filter = "YNAB Transaction Export (*.csv)|*.csv", FileName = ((YnabCsvImport)Importable).TransactionPath };
            if (openFileDialog.ShowDialog() == true)
            {
                FileInfo fi = new FileInfo(openFileDialog.FileName);
                ((YnabCsvImport)Importable).TransactionPath = openFileDialog.FileName;
            }
        }

        private void BrowseYnabCsvBudget()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Multiselect = false, DefaultExt = "csv", Filter = "YNAB Transaction Export (*.csv)|*.csv", FileName = ((YnabCsvImport)Importable).BudgetPath };
            if (openFileDialog.ShowDialog() == true)
            {
                ((YnabCsvImport)Importable).BudgetPath = openFileDialog.FileName;
            }
        }

        private void BrowseSave()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog { DefaultExt = "sqlite", Filter = "SQLite Budget Plan (*.sqlite)|*.sqlite", FileName = Importable.SavePath};
            if (saveFileDialog.ShowDialog() == true)
            {
                Importable.SavePath = saveFileDialog.FileName;
            }
        }
    }
}
