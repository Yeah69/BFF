using System;
using System.Linq;
using Realms;

namespace BFF.Persistence.Realm.Models.Persistence
{
    internal class Trans : RealmObject, IUniqueIdPersistenceModelRealm
    {
        [PrimaryKey]
        public int Id { get; set; }
        public Flag Flag { get; set; }
        public string CheckNumber { get; set; }
        public Account Account { get; set; }
        public Payee Payee { get; set; }
        public Category Category { get; set; }
        public DateTimeOffset Date { get; set; }
        public string Memo { get; set; }
        public long Sum { get; set; }
        public bool Cleared { get; set; }

        public Account ToAccount { get; set; }
        public Account FromAccount { get; set; }

        public int TypeIndex { get; set; }

        [Backlink(nameof(SubTransaction.Parent))]
        public IQueryable<SubTransaction> SubTransactions { get; }

        public override bool Equals(object obj)
        {
            if (obj is null || !(obj is Trans other)) return false;
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}