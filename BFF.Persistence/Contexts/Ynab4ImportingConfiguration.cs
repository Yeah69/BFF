using BFF.Core.Persistence;

namespace BFF.Persistence.Contexts
{
    public interface IYnab4ImportConfiguration : IImportingConfiguration
    {
        string TransactionPath { get; }

        string BudgetPath { get; }

        string SavePath { get; }
    }

    internal class Ynab4ImportConfiguration : IYnab4ImportConfiguration
    {
        public Ynab4ImportConfiguration(
            (string TransactionPath, string BudgetPath, string SavePath) paths)
        {
            TransactionPath = paths.TransactionPath;
            BudgetPath = paths.BudgetPath;
            SavePath = paths.SavePath;
        }

        public string TransactionPath { get; }
        public string BudgetPath { get; }
        public string SavePath { get; }
    }
}
