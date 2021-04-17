using BFF.Model.Contexts;
using BFF.Model.Import;
using BFF.Model.ImportExport;
using BFF.Persistence.Common;
using BFF.Persistence.Realm.ORM;
using System;
using System.Threading.Tasks;

namespace BFF.Persistence.Realm
{
    public interface IRealmContext : IExportContext, IImportContext, ILoadContext
    {
    }

    internal class RealmContext : IRealmContext
    {
        private readonly IRealmExporter _realmExporter;
        private readonly Lazy<IProvideRealmConnection> _provideRealmConnection;
        private readonly IDisposable _cleanUpContext;
        private readonly IRealmImporter _realmImporter;

        public RealmContext(
            // parameters
            IDisposable cleanUpContext,
            
            // dependencies
            IRealmProjectFileAccessConfiguration configuration,
            IRealmImporter realmImporter,
            IRealmExporter realmExporter,
            Lazy<IProvideRealmConnection> provideRealmConnection)
        {
            _realmExporter = realmExporter;
            _provideRealmConnection = provideRealmConnection;
            _cleanUpContext = cleanUpContext;
            _realmImporter = realmImporter;

            Title = configuration.Path;
        }
        
        public Task Export(DtoImportContainer container) => _realmExporter.Export(container);

        public Task<DtoImportContainer> Import() => _realmImporter.Import();

        public void Load()
        {
            var _ = _provideRealmConnection.Value.Connection;
        }

        public void Dispose() => 
            _cleanUpContext.Dispose();

        public string Title { get; }
    }

    internal class RealmContextFactory : ContextFactoryBase<IRealmProjectFileAccessConfiguration, IRealmContext>
    {
        public RealmContextFactory(Func<IRealmProjectFileAccessConfiguration, IRealmContext> factory) : base(factory)
        {}
    }
}