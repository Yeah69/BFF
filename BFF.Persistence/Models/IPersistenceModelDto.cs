using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Models
{
    public interface IPersistenceModelDto
    {
        [Key]
        long Id { get; set;  }
    }
}