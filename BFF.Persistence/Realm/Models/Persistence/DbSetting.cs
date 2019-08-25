using Realms;

namespace BFF.Persistence.Realm.Models.Persistence
{
    public interface IDbSettingRealm : IUniqueIdPersistenceModelRealm
    {
        string CurrencyCultureName { get; set; }
        string DateCultureName { get; set; }
    }
    
    internal class DbSetting : RealmObject, IDbSettingRealm
    {
        [PrimaryKey]
        public int Id { get; set; } = 0;
        public string CurrencyCultureName { get; set; } = "de-DE";
        public string DateCultureName { get; set; } = "de-DE";

        public override bool Equals(object obj)
        {
            if (obj is null || !(obj is IDbSettingRealm other)) return false;
            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}