using System;
using Realms;

namespace BFF.Persistence.Realm.Models.Persistence
{
    internal class SubTransaction : RealmObject, IUniqueIdPersistenceModelRealm
    {
        [PrimaryKey]
        public int Id { get; set; }
        public Trans? Parent { get; set; }
        public Category? Category { get; set; }
        public string Memo { get; set; } = String.Empty;
        public long Sum { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is SubTransaction other)) return false;
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}