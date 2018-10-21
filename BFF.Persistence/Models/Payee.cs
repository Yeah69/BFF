using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Models
{
    public interface IPayeeDto : IPersistenceModelDto
    {
        string Name { get; set; }
    }
    
    internal class Payee : IPayeeDto
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
    }
}