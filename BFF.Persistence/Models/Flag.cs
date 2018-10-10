using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Models
{
    public class Flag : IPersistenceModel
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public long Color { get; set; }
    }
}
