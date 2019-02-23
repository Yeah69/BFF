using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Models.Sql
{
    public interface IPersistenceModelSql : IPersistenceModel
    {
        [Key]
        long Id { get; set;  }
    }
}