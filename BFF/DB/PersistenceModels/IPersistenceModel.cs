using Dapper.Contrib.Extensions;

namespace BFF.DB.PersistenceModels
{
    public interface IPersistenceModel
    {
        [Key]
        long Id { get; set;  }
    }
}