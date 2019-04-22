using Realms;

namespace BFF.Persistence.Realm.Models.Persistence
{
    public interface ISubTransactionRealm : IUniqueIdPersistenceModelRealm, IHaveCategoryRealm
    {
        ITransRealm Parent { get; set; }
        string Memo { get; set; }
        long Sum { get; set; }
    }
    
    internal class SubTransaction : RealmObject, ISubTransactionRealm
    {
        [PrimaryKey]
        public int Id { get; set; }
        public Trans ParentRef { get; set; }
        [Ignored]
        public ITransRealm Parent { get => ParentRef; set => ParentRef = value as Trans; }
        public Category CategoryRef { get; set; }
        [Ignored]
        public ICategoryRealm Category { get => CategoryRef; set => CategoryRef = value as Category; }
        public string Memo { get; set; }
        public long Sum { get; set; }
    }
}