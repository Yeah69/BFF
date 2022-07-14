using BFF.Core.IoC;
using System.Threading.Tasks;
using BFF.Persistence.Common;
using BFF.Persistence.Realm.Models.Persistence;

namespace BFF.Persistence.Realm.ORM
{
    internal class RealmCreateBackendOrm : ICreateBackendOrm, IScopeInstance
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

                realm.Write(() => realm.Add(new DbSetting()));
            }
        }
    }
}
