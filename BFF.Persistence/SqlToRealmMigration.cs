using BFF.Model.Contexts;
using BFF.Model.Helper;
using BFF.Persistence.Contexts;
using BFF.Persistence.Realm;
using BFF.Persistence.Sql;
using System.Threading.Tasks;

namespace BFF.Persistence
{
    internal class SqlToRealmMigration : ISqlToRealmMigration
    {
        private readonly SqliteContextFactory _sqliteContextFactory;
        private readonly RealmContextFactory _realmContextFactory;

        public SqlToRealmMigration(
            SqliteContextFactory sqliteContextFactory,
            RealmContextFactory realmContextFactory)
        {
            _sqliteContextFactory = sqliteContextFactory;
            _realmContextFactory = realmContextFactory;
        }

        public async Task JustDoIt()
        {
            var importContext = (IImportContext) _sqliteContextFactory.Create(new SqliteProjectFileAccessConfiguration(
                @"C:\Users\Yeah69\Documents\BFF_Budgets\Yeah.sqlite"));
            var exportContest = (IExportContext) _realmContextFactory.Create(new RealmProjectFileAccessConfiguration(
                @"C:\Users\Yeah69\Documents\BFF_Budgets\Yeah.realm",
                @",]r5«M×ÊÈÄ~Í¿Ô¨û}Ã'ÚV:>+ß¤!GTñÂ\"));
            await exportContest
                .Export(await importContext.Import().ConfigureAwait(false))
                .ConfigureAwait(false);
        }
    }
}