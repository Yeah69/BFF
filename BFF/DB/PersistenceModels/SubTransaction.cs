namespace BFF.DB.PersistenceModels
{
    public class SubTransaction : IPersistenceModel
    {
        public long Id { get; set; }
        public long ParentId { get; set; }
        public long CategoryId { get; set; }
        public string Memo { get; set; }
        public long Sum { get; set; }
    }
}