using System.Threading.Tasks;
using BFF.Persistence.ORM.Sqlite.Interfaces;
using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Models.Sql
{
    public interface IDbSettingSql : IPersistenceModelSql
    {
        string CurrencyCultureName { get; set; }
        string DateCultureName { get; set; }
    }
    
    internal class DbSetting : IDbSettingSql
    {
        private readonly ICrudOrm<DbSetting> _crudOrm;

        public DbSetting(ICrudOrm<DbSetting> crudOrm)
        {
            _crudOrm = crudOrm;
        }

        [Key]
        public long Id { get; set; }
        public string CurrencyCultureName { get; set; } = "de-DE";
        public string DateCultureName { get; set; } = "de-DE";

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