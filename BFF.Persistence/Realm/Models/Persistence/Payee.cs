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

        public override bool Equals(object obj)
        {
            if (obj is null || !(obj is IPayeeRealm other)) return false;
            return Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}