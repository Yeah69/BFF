namespace BFF.Model.ImportExport
{
    public interface IYnab4CsvImportConfiguration : IImportConfiguration
    {
        string TransactionPath { get; }

        string BudgetPath { get; }
    }

    internal class Ynab4CsvImportConfiguration : IYnab4CsvImportConfiguration
    {
        public Ynab4CsvImportConfiguration(
            (string TransactionPath, string BudgetPath) paths)
        {
            TransactionPath = paths.TransactionPath;
            BudgetPath = paths.BudgetPath;
        }

        public string TransactionPath { get; }
        public string BudgetPath { get; }
    }
}
