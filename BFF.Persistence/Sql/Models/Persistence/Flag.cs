using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Sql.Models.Persistence
{
    public interface IFlagSql : IPersistenceModelSql
    {
        string Name { get; set; }
        long Color { get; set; }
    }
    
    internal class Flag : IFlagSql
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public long Color { get; set; }
    }
}
