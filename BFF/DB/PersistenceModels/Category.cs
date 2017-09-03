namespace BFF.DB.PersistenceModels
{
    public class Category : IPersistenceModel
    {
        public long Id { get; set; }
        public long? ParentId { get; set; }
        public string Name { get; set; }
    }
}