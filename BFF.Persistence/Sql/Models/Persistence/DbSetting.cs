using Dapper.Contrib.Extensions;

namespace BFF.Persistence.Sql.Models.Persistence
{
    public interface IDbSettingSql : IPersistenceModelSql
    {
        string CurrencyCultureName { get; set; }
        string DateCultureName { get; set; }
    }
    
    internal class DbSetting : IDbSettingSql
    {
        [Key]
        public long Id { get; set; }
        public string CurrencyCultureName { get; set; } = "de-DE"; // ToDo default values are business logic and should go to domain layer
        public string DateCultureName { get; set; } = "de-DE"; // ToDo default values are business logic and should go to domain layer
    }
}