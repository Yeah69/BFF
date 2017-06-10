namespace BFF.DB.PersistanceModels
{
    public class DbSetting : IPersistanceModel
    {
        public long Id { get; set; }
        public string CurrencyCultureName { get; set; } = "de-DE";
        public string DateCultureName { get; set; } = "de-DE";
    }
}