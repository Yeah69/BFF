using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Models
{
    public interface IPersistenceModel
    {
        [Key]
        long Id { get; set;  }
    }
}