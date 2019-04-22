using Realms;

namespace BFF.Persistence.Realm.Models.Persistence
{
    public interface IFlagRealm : IUniquelyNamedPersistenceModelRealm
    {
        long Color { get; set; }
    }
    
    internal class Flag : RealmObject, IFlagRealm
    {
        [PrimaryKey]
        public string Name { get; set; }
        public long Color { get; set; }
    }
}
