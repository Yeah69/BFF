using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Models
{
    public class Payee : IPersistenceModel
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
    }
}