using System.Threading.Tasks;
using BFF.Persistence.ORM.Sqlite.Interfaces;
using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Models.Sql
{
    public interface IFlagSql : IPersistenceModelSql
    {
        string Name { get; set; }
        long Color { get; set; }
    }
    
    internal class Flag : IFlagSql
    {
        private readonly ICrudOrm<Flag> _crudOrm;

        public Flag(ICrudOrm<Flag> crudOrm)
        {
            _crudOrm = crudOrm;
        }

        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public long Color { get; set; }

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
