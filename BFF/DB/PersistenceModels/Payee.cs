namespace BFF.DB.PersistenceModels
{
    public class Payee : IPersistenceModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}