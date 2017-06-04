namespace BFF.DB.PersistanceModels
{
    public class Payee : IPersistanceModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}