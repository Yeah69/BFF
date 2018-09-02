using Dapper.Contrib.Extensions;

namespace BFF.DB.PersistenceModels
{
    public class Payee : IPersistenceModel
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
    }
}