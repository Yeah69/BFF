using BFF.Persistence.Realm.Models.Persistence;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal interface IRealmModel<T> where T : class, IPersistenceModelRealm
    {
        T? RealmObject { get; }
    }
}
