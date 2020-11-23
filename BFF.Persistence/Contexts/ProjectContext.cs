using BFF.Model.Import;
using System;
using System.Threading.Tasks;
using BFF.Model.IoC;
using BFF.Persistence.Common;
using BFF.Persistence.Realm;

namespace BFF.Persistence.Contexts
{
    internal class ProjectContext : IProjectContext
    {
        private readonly IDisposable _disposeContext;
        private readonly ICreateBackendOrm _createBackendOrm;
        private readonly Func<IExporter> _exporterFactory;

        public ProjectContext(
            // parameters
            IDisposable disposeContext,

            // dependencies
            ICreateBackendOrm createBackendOrm,
            Func<IExporter> exporterFactory)
        {
            _disposeContext = disposeContext;
            _createBackendOrm = createBackendOrm;
            _exporterFactory = exporterFactory;
        }

        public void Dispose() => _disposeContext.Dispose();

        public Task CreateProject() => _createBackendOrm.CreateAsync();

        public Task CreateProject(DtoImportContainer container) => _exporterFactory().ExportAsync(container);
    }
}
