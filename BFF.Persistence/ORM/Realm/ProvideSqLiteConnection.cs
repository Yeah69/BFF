using BFF.Core.Persistence;
using Realms;

namespace BFF.Persistence.ORM.Realm
{

    public interface IProvideRealmConnection : IProvideConnection<Realms.Realm>
    { }

    internal class ProvideConnection : ProvideConnectionBase<Realms.Realm>, IProvideRealmConnection
    {
        public override Realms.Realm Connection
        {
            get
            {
                var config = new RealmConfiguration(DbPath)
                {
                    SchemaVersion = 0
                };
                var realm = Realms.Realm.GetInstance(config);
                return realm;
            }
        }

        protected override string ConnectionString => DbPath;

        public ProvideConnection(string dbPath) : base(dbPath)
        {
        }
    }
}