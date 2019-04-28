using System;
using BFF.Core.Persistence;
using BFF.Model.Contexts;
using BFF.Model.Models.Utility;
using BFF.Persistence.Import;

namespace BFF.Persistence.Contexts
{
    internal class ImportProxyContext : IImportProxyContext
    {
        public ImportProxyContext(
            IImportingConfiguration importingConfiguration,
            ILoadProjectFromFileConfiguration loadProjectFromFileConfiguration,
            Func<IImportingConfiguration, ILoadProjectFromFileConfiguration, IImportContext> importContextFactory,
            Func<IImportable, IImportProxy> importContextProxyFactory)
        {
            var importable = importContextFactory(importingConfiguration, loadProjectFromFileConfiguration)
                .Importable;
            ImportProxy =
                importContextProxyFactory(importable);
        }

        public IImportProxy ImportProxy { get; }
    }
}