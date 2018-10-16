using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Models
{
    public class PayeeDto : IPersistenceModelDto
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
    }
}