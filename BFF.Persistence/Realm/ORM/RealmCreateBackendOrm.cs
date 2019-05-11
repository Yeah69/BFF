using System.Threading.Tasks;
using BFF.Persistence.Common;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.ORM
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
