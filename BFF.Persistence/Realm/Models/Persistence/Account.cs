using System;
using Realms;

namespace BFF.Persistence.Realm.Models.Persistence
{
    public interface IAccountRealm : IUniquelyNamedPersistenceModelRealm
    {
        long StartingBalance { get; set; }
        DateTime StartingDate { get; set; }
    }
    
    internal class Account : RealmObject, IAccountRealm
    {
        [PrimaryKey]
        public string Name { get; set; }
        public long StartingBalance { get; set; }
        [Ignored]
        public DateTime StartingDate { get => StartingDateOffset.UtcDateTime; set => StartingDateOffset = value; }
        public DateTimeOffset StartingDateOffset { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is null || !(obj is IAccountRealm other)) return false;
            return Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}