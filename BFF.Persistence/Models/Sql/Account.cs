using System;
using System.Threading.Tasks;
using BFF.Persistence.ORM.Sqlite.Interfaces;
using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Models.Sql
{
    public interface IAccountSql : IPersistenceModelSql
    {
        string Name { get; set; }
        long StartingBalance { get; set; }
        DateTime StartingDate { get; set; }
    }
    
    internal class Account : IAccountSql
    {
        private readonly ICrudOrm<Account> _crudOrm;

        public Account(ICrudOrm<Account> crudOrm)
        {
            _crudOrm = crudOrm;
        }

        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public long StartingBalance { get; set; }
        public DateTime StartingDate { get; set; }

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
    }
}