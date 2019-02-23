using System.Threading.Tasks;
using BFF.Persistence.ORM.Sqlite.Interfaces;
using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Models.Sql
{
    public interface ICategorySql : IPersistenceModelSql
    {
        long? ParentId { get; set; }
        string Name { get; set; }
        bool IsIncomeRelevant { get; set; }
        int MonthOffset { get; set; }
    }
    
    internal class Category : ICategorySql
    {
        private readonly ICrudOrm<Category> _crudOrm;

        public Category(ICrudOrm<Category> crudOrm)
        {
            _crudOrm = crudOrm;
        }

        [Key]
        public long Id { get; set; }
        public long? ParentId { get; set; }
        public string Name { get; set; }
        public bool IsIncomeRelevant { get; set; }
        public int MonthOffset { get; set; }

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