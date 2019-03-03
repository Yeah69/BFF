using Realms;

namespace BFF.Persistence.Realm.Persistence.Models
{
    public interface IPersistenceModelRealm
    {
        [Ignored]
        bool IsInserted { get; set; }
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