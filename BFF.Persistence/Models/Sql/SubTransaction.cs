using System.Threading.Tasks;
using BFF.Persistence.ORM.Sqlite.Interfaces;
using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Models.Sql
{
    public interface ISubTransactionSql : IPersistenceModelSql, IHaveCategorySql
    {
        long ParentId { get; set; }
        string Memo { get; set; }
        long Sum { get; set; }
    }
    
    internal class SubTransaction : ISubTransactionSql
    {
        private readonly ICrudOrm<SubTransaction> _crudOrm;

        public SubTransaction(ICrudOrm<SubTransaction> crudOrm)
        {
            _crudOrm = crudOrm;
        }

        [Key]
        public long Id { get; set; }
        public long ParentId { get; set; }
        public long? CategoryId { get; set; }
        public string Memo { get; set; }
        public long Sum { get; set; }

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