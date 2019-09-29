using System;
using System.Collections.Generic;
using System.Linq;
using BFF.Core.Helper;
using Realms;

namespace BFF.Persistence.Realm.Models.Persistence
{
    public interface ITransRealm : IUniqueIdPersistenceModelRealm, IHaveAccountRealm, IHaveCategoryRealm, IHavePayeeRealm, IHaveFlagRealm
    {
        string CheckNumber { get; set; }
        DateTime Date { get; set; }
        string Memo { get; set; }
        long Sum { get; set; }
        [Indexed]
        bool Cleared { get; set; }
        IAccountRealm ToAccount { get; set; }
        IAccountRealm FromAccount { get; set; }
        TransType Type { get; set; }
        IEnumerable<ISubTransactionRealm> SubTransactions { get; }
    }
    
    internal class Trans : RealmObject, ITransRealm
    {
        [PrimaryKey]
        public int Id { get; set; }
        public Flag FlagRef { get; set; }
        [Ignored]
        public IFlagRealm Flag { get => FlagRef; set => FlagRef = value as Flag; }
        public string CheckNumber { get; set; }
        public Account AccountRef { get; set; }
        [Ignored]
        public IAccountRealm Account { get => AccountRef; set => AccountRef = value as Account; }
        public Payee PayeeRef { get; set; }
        [Ignored]
        public IPayeeRealm Payee { get => PayeeRef; set => PayeeRef = value as Payee; }
        public Category CategoryRef { get; set; }
        [Ignored]
        public ICategoryRealm Category { get => CategoryRef; set => CategoryRef = value as Category; }
        [Ignored]
        public DateTime Date { get => DateOffset.UtcDateTime; set => DateOffset = value; }
        public DateTimeOffset DateOffset { get; set; }
        public string Memo { get; set; }
        public long Sum { get; set; }
        [Indexed]
        public bool Cleared { get; set; }

        public Account ToAccountRef { get; set; }
        [Ignored]
        public IAccountRealm ToAccount { get => ToAccountRef; set => ToAccountRef = value as Account; }
        public Account FromAccountRef { get; set; }
        [Ignored]
        public IAccountRealm FromAccount { get => FromAccountRef; set => FromAccountRef = value as Account; }

        public int TypeIndex { get; set; }
        [Ignored]
        public TransType Type { get => (TransType)TypeIndex; set => TypeIndex = (int)value; }

        [Backlink(nameof(SubTransaction.ParentRef))]
        public IQueryable<SubTransaction> SubTransactionsRef { get; }

        [Ignored]
        public IEnumerable<ISubTransactionRealm> SubTransactions => SubTransactionsRef;

        public override bool Equals(object obj)
        {
            if (obj is null || !(obj is ITransRealm other)) return false;
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}