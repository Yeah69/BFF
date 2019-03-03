using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Sql.Models.Persistence
{
    public interface IPersistenceModelSql
    {
        [Key]
        long Id { get; set;  }
    }
}