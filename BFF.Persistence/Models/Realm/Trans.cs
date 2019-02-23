using System;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Persistence.ORM.Realm.Interfaces;
using Realms;

namespace BFF.Persistence.Models.Realm
{
    public interface ITransRealm : IUniqueIdPersistenceModelRealm, IHaveAccountRealm, IHaveCategoryRealm, IHavePayeeRealm, IHaveFlagRealm
    {
        string CheckNumber { get; set; }
        DateTimeOffset Date { get; set; }
        string Memo { get; set; }
        long Sum { get; set; }
        [Indexed]
        bool Cleared { get; set; }
        IAccountRealm ToAccount { get; set; }
        IAccountRealm FromAccount { get; set; }
        TransType Type { get; set; }
    }
    
    internal class Trans : RealmObject, ITransRealm
    {
        private readonly ICrudOrm<Trans> _crudOrm;

        public Trans(bool isInserted, ICrudOrm<Trans> crudOrm)
        {
            _crudOrm = crudOrm;
            IsInserted = isInserted;
        }

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
        public DateTimeOffset Date { get; set; }
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

        public Task<bool> InsertAsync()
        {
            return _crudOrm.CreateAsync(this);
        }

        public Task UpdateAsync()
        {
            return _crudOrm.UpdateAsync(this);
        }

        public Task DeleteAsync()
        {
            return _crudOrm.DeleteAsync(this);
        }

        [Ignored]
        public bool IsInserted { get; set; }
    }
}