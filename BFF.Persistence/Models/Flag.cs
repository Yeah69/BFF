using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Models
{
    public interface IFlagDto : IPersistenceModelDto
    {
        string Name { get; set; }
        long Color { get; set; }
    }
    
    internal class Flag : IFlagDto
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public long Color { get; set; }
    }
}
