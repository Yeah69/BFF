using Realms;

namespace BFF.Persistence.Realm.Models.Persistence
{
    public interface IPersistenceModelRealm
    {
    }

    public interface IUniquelyNamedPersistenceModelRealm : IPersistenceModelRealm
    {
        [PrimaryKey]
        string Name { get; set; }
    }

    public interface IUniqueIdPersistenceModelRealm : IPersistenceModelRealm
    {
        [PrimaryKey]
        int Id { get; set; }
    }
}