using Realms;

namespace BFF.Persistence.Realm.Models.Persistence
{
    internal class Payee : RealmObject, IUniquelyNamedPersistenceModelRealm
    {
        [PrimaryKey]
        public string Name { get; set; }
    }
}