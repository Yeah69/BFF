using System;
using System.Threading.Tasks;
using BFF.Persistence.ORM.Sqlite.Interfaces;
using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Models.Sql
{
    public interface IBudgetEntrySql : IPersistenceModelSql, IHaveCategorySql
    {
        DateTime Month { get; set; }
        long Budget { get; set; }
    }
    
    internal class BudgetEntry : IBudgetEntrySql
    {
        private readonly ICrudOrm<BudgetEntry> _crudOrm;

        public BudgetEntry(ICrudOrm<BudgetEntry> crudOrm)
        {
            _crudOrm = crudOrm;
        }

        [Key]
        public long Id { get; set; }
        public long? CategoryId { get; set; }
        public DateTime Month { get; set; }
        public long Budget { get; set; }

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