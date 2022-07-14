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
        private readonly IProvideRealmConnection _provideRealmConnection;
        private readonly Func<IContext, ILoadContextViewModelProxy> _loadContextViewModelProxyFactory;
        private readonly IAsyncDisposable _cleanUpContext;
        private readonly IRealmImporter _realmImporter;

        public RealmContext(
            // parameters
            IAsyncDisposable cleanUpContext,
            
            // dependencies
            IRealmProjectFileAccessConfiguration configuration,
            IRealmImporter realmImporter,
            IRealmExporter realmExporter,
            IProvideRealmConnection provideRealmConnection,
            Func<IContext, ILoadContextViewModelProxy> loadContextViewModelProxyFactory)
        {
            _realmExporter = realmExporter;
            _provideRealmConnection = provideRealmConnection;
            _loadContextViewModelProxyFactory = loadContextViewModelProxyFactory;
            _cleanUpContext = cleanUpContext;
            _realmImporter = realmImporter;

            Title = configuration.Path;
        }
        
        public Task Export(DtoImportContainer container) => _realmExporter.Export(container);

        public Task<DtoImportContainer> Import() => _realmImporter.Import();

        public void Load()
        {
            var _ = _provideRealmConnection.Connection;
        }

        public ILoadContextViewModelProxy ViewModelContext(IContext context) => 
            _loadContextViewModelProxyFactory(context);

        public void Dispose() => 
            Task.Run(() => _cleanUpContext.DisposeAsync());

        public string Title { get; }
    }

    internal class RealmContextFactory : ContextFactoryBase<IRealmProjectFileAccessConfiguration, IRealmContext>
    {
        public RealmContextFactory(Func<IRealmProjectFileAccessConfiguration, IRealmContext> factory) : base(factory)
        {}
    }
}