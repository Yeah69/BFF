using BFF.Core.IoC;
using BFF.Persistence.Models.Realm;

namespace BFF.Persistence.ORM.Realm.Interfaces
{
    public interface ICrudOrm<T> : ICrudOrmCommon<T>, IOncePerBackend
        where T : class, IPersistenceModelRealm
    {
    }
}