using Dapper.Contrib.Extensions;

namespace BFF.DB.PersistenceModels
{
    public class DbSetting : IPersistenceModel
    {
        [Key]
        public long Id { get; set; }
        public string CurrencyCultureName { get; set; } = "de-DE";
        public string DateCultureName { get; set; } = "de-DE";
    }
}