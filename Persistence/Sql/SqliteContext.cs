using BFF.Model.Contexts;
using BFF.Model.Import;
using BFF.Model.ImportExport;
using BFF.Persistence.Common;
using BFF.Persistence.Sql.ORM;
using System;
using System.Threading.Tasks;

namespace BFF.Persistence.Sql
{
    public interface ISqliteContext : IImportContext, ILoadContext
    {
    }

    internal class SqliteContext : ISqliteContext
    {
        private readonly IDapperImportingOrm _dapperImportingOrm;
        private readonly IDisposable _cleanUpContext;

        public SqliteContext(
            IDapperImportingOrm dapperImportingOrm,
            ISqliteProjectFileAccessConfiguration configuration,
            IDisposable cleanUpContext)
        {
            _dapperImportingOrm = dapperImportingOrm;
            _cleanUpContext = cleanUpContext;

            Title = configuration.Path;
        }

        public void Load()
        {
        }

        public Task<DtoImportContainer> Import() => 
            _dapperImportingOrm.Import();

        public void Dispose() => 
            _cleanUpContext.Dispose();

        public string Title { get; }
    }

    internal class SqliteContextFactory : ContextFactoryBase<ISqliteProjectFileAccessConfiguration, ISqliteContext>
    {
        public SqliteContextFactory(Func<ISqliteProjectFileAccessConfiguration, ISqliteContext> factory) : base(factory)
        {}
    }
}