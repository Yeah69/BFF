using System;
using BFF.Helper.Import;
using Microsoft.Win32;
using Reactive.Bindings;

namespace BFF.MVVM.ViewModels
{
    class ImportDialogViewModel : ViewModelBase
    {
        public IImportable Importable { get; }

        public ReactiveCommand BrowseYnabCsvTransactionCommand { get; } = new ReactiveCommand();

        public ReactiveCommand BrowseYnabCsvBudgetCommand { get; } = new ReactiveCommand();

        public ReactiveCommand BrowseSaveCommand { get; } = new ReactiveCommand();

        public ImportDialogViewModel(IImportable importable)
        {
            Importable = importable;

            BrowseYnabCsvTransactionCommand.Subscribe(_ =>
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

            BrowseYnabCsvBudgetCommand.Subscribe(_ =>
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

            BrowseSaveCommand.Subscribe(_ =>
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
