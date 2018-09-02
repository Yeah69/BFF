using Dapper.Contrib.Extensions;

namespace BFF.DB.PersistenceModels
{
    public class Flag : IPersistenceModel
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public long Color { get; set; }
    }
}
