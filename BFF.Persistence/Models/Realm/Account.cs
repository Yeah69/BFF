using System;
using System.Threading.Tasks;
using BFF.Persistence.ORM.Realm.Interfaces;
using Realms;

namespace BFF.Persistence.Models.Realm
{
    public interface IAccountRealm : IUniquelyNamedPersistenceModelRealm
    {
        long StartingBalance { get; set; }
        DateTimeOffset StartingDate { get; set; }
    }
    
    internal class Account : RealmObject, IAccountRealm
    {
        private readonly ICrudOrm<Account> _crudOrm;

        public Account(bool isInserted, ICrudOrm<Account> crudOrm)
        {
            _crudOrm = crudOrm;
            IsInserted = isInserted;
        }

        [PrimaryKey]
        public string Name { get; set; }
        public long StartingBalance { get; set; }
        public DateTimeOffset StartingDate { get; set; }

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