using System;
using Realms;

namespace BFF.Persistence.Realm.Models.Persistence
{
    internal class Account : RealmObject, IUniquelyNamedPersistenceModelRealm
    {
        [PrimaryKey]
        public string Name { get; set; }
        public long StartingBalance { get; set; }
        public DateTimeOffset StartingDate { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is null || !(obj is Account other)) return false;
            return Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}