using Realms;

namespace BFF.Persistence.Realm.Models.Persistence
{
    public interface IPayeeRealm : IUniquelyNamedPersistenceModelRealm
    {
    }
    
    internal class Payee : RealmObject, IPayeeRealm
    {
        [PrimaryKey]
        public string Name { get; set; }
    }
}