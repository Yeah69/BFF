using System.Threading.Tasks;
using BFF.Persistence.ORM.Realm.Interfaces;

namespace BFF.Persistence.ORM.Realm
{
    internal class RealmCreateBackendOrm : ICreateBackendOrm
    {
        private readonly IProvideRealmConnection _provideConnection;

        public RealmCreateBackendOrm(IProvideRealmConnection provideConnection)
        {
            _provideConnection = provideConnection;
        }

        public async Task CreateAsync()
        {
            var realm = _provideConnection.Connection;
        }
    }
}
