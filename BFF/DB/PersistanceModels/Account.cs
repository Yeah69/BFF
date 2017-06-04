namespace BFF.DB.PersistanceModels
{
    public class Account : IPersistanceModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long StartingBalance { get; set; }
    }
}