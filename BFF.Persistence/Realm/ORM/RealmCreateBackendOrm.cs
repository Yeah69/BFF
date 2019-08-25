using System.Threading.Tasks;
using BFF.Persistence.Common;

namespace BFF.Persistence.Realm.ORM
{
    internal class RealmCreateBackendOrm : ICreateBackendOrm
    {
        private readonly IRealmOperations _realmOperations;

        public RealmCreateBackendOrm(
            IRealmOperations realmOperations)
        {
            _realmOperations = realmOperations;
        }

        public Task CreateAsync()
        {
            return _realmOperations.RunActionAsync(Inner);

            static void Inner(Realms.Realm realm)
            {
                // Getting the realm instance created is sufficient to create a Realm DB
            }
        }
    }
}
