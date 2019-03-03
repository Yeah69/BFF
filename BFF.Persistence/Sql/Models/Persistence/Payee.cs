using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Sql.Models.Persistence
{
    public interface IPayeeSql : IPersistenceModelSql
    {
        string Name { get; set; }
    }
    
    internal class Payee : IPayeeSql
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
    }
}