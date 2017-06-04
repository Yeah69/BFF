namespace BFF.DB.PersistanceModels
{
    public class Category : IPersistanceModel
    {
        public long Id { get; set; }
        public long? ParentId { get; set; }
        public string Name { get; set; }
    }
}