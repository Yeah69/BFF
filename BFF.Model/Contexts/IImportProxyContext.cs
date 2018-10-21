using System;
using BFF.Core.Persistence;
using BFF.Model.Models.Utility;
using BFF.Persistence.Contexts;
using BFF.Persistence.Import;

namespace BFF.Model.Contexts
{
    public interface IImportProxyContext
    {
        IImportProxy ImportProxy { get; }
    }

    internal class ImportProxyContext : IImportProxyContext
    {
        public ImportProxyContext(
            IImportingConfiguration importingConfiguration,
            IPersistenceConfiguration persistenceConfiguration,
            Func<IImportingConfiguration, IPersistenceConfiguration, IImportContext> importContextFactory,
            Func<IImportable, IImportProxy> importContextProxyFactory)
        {
            var importable = importContextFactory(importingConfiguration, persistenceConfiguration)
                .Importable;
            ImportProxy =
                importContextProxyFactory(importable);
        }

        public IImportProxy ImportProxy { get; }
    }
}