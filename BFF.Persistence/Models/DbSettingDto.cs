using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Models
{
    public class DbSettingDto : IPersistenceModelDto
    {
        [Key]
        public long Id { get; set; }
        public string CurrencyCultureName { get; set; } = "de-DE";
        public string DateCultureName { get; set; } = "de-DE";
    }
}