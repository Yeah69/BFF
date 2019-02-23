using System.Threading.Tasks;
using BFF.Persistence.ORM.Sqlite.Interfaces;
using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Models.Sql
{
    public interface IPayeeSql : IPersistenceModelSql
    {
        string Name { get; set; }
    }
    
    internal class Payee : IPayeeSql
    {
        private readonly ICrudOrm<Payee> _crudOrm;

        public Payee(ICrudOrm<Payee> crudOrm)
        {
            _crudOrm = crudOrm;
        }

        [Key]
        public long Id { get; set; }
        public string Name { get; set; }

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