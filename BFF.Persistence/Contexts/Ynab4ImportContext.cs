using System;
using BFF.Persistence.Import;

namespace BFF.Persistence.Contexts
{
    public interface IYnab4ImportContext : IImportContext { }
    internal class Ynab4ImportContext : IYnab4ImportContext
    {
        public Ynab4ImportContext(
            IYnab4ImportConfiguration ynab4ImportConfiguration,
            Func<IYnab4ImportConfiguration, IYnab4CsvImport> ynab4ImportFactory)
        {
            Importable = ynab4ImportFactory(ynab4ImportConfiguration);
        }

        public IImportable Importable { get; }
    }
}
