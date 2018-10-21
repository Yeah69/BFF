using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Models
{
    public interface IDbSettingDto : IPersistenceModelDto
    {
        string CurrencyCultureName { get; set; }
        string DateCultureName { get; set; }
    }
    
    internal class DbSetting : IDbSettingDto
    {
        [Key]
        public long Id { get; set; }
        public string CurrencyCultureName { get; set; } = "de-DE";
        public string DateCultureName { get; set; } = "de-DE";
    }
}