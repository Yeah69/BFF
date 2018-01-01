namespace BFF.DB.PersistenceModels
{
    public class DbSetting : IPersistenceModel
    {
        public long Id { get; set; }
        public string CurrencyCultureName { get; set; } = "de-DE";
        public string DateCultureName { get; set; } = "de-DE";
    }
}