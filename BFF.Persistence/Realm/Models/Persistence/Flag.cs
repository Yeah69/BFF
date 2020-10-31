using System;
using Realms;

namespace BFF.Persistence.Realm.Models.Persistence
{
    internal class Flag : RealmObject, IUniquelyNamedPersistenceModelRealm
    {
        [PrimaryKey]
        public string Name { get; set; } = String.Empty;
        public long Color { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is Flag other)) return false;
            return Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
