using System;
using System.Threading.Tasks;
using BFF.Core.Persistence;
using BFF.Persistence.Realm;
using JetBrains.Annotations;

namespace BFF.Persistence.Import
{
    internal class ImportExport : IImportExport
    {
        private readonly Func<IImportingConfiguration, IImporter> _importerFactory;
        private readonly Func<IExportingConfiguration, IExporter> _exporterFactory;

        public ImportExport(
            [NotNull] Func<IImportingConfiguration, IImporter> importerFactory,
            [NotNull] Func<IExportingConfiguration, IExporter> exporterFactory)
        {
            importerFactory = importerFactory ?? throw new ArgumentNullException(nameof(importerFactory));
            exporterFactory = exporterFactory ?? throw new ArgumentNullException(nameof(exporterFactory));

            _importerFactory = importerFactory;
            _exporterFactory = exporterFactory;
        }

        public async Task ImportExportAsync(IImportingConfiguration importingConfiguration, IExportingConfiguration exportingConfiguration)
        {
            var exporter = _exporterFactory(exportingConfiguration);
            var importer = _importerFactory(importingConfiguration);
            try
            {
                var importContainer = importer.Import();
                await exporter.ExportAsync(importContainer).ConfigureAwait(false);
            }
            finally
            {
                (exporter as IDisposable)?.Dispose();
                (importer as IDisposable)?.Dispose();
            }
        }
    }
}
