using System;
using BFF.Core.Persistence;
using BFF.Persistence.Import;

namespace BFF.Persistence.Contexts
{
    public interface IYnab4ImportContext : IImportContext { }
    internal class Ynab4ImportContext : IYnab4ImportContext
    {
        public Ynab4ImportContext(
            IYnab4ImportConfiguration ynab4ImportConfiguration,
            IPersistenceConfiguration persistenceConfiguration,
            Func<IPersistenceConfiguration, IPersistenceContext> persistenceContextFactory,
            Func<IYnab4ImportConfiguration, IYnab4Import> ynab4ImportFactory)
        {
            persistenceContextFactory(persistenceConfiguration);
            Importable = ynab4ImportFactory(ynab4ImportConfiguration);
        }

        public IImportable Importable { get; }
    }
}
