using Realms;

namespace BFF.Persistence.Models.Realm
{
    public interface IPersistenceModelRealm : IPersistenceModel
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