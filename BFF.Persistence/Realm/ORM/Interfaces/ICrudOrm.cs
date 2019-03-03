using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Realm.Persistence.Models;

namespace BFF.Persistence.Realm.ORM.Interfaces
{
    public interface ICrudOrm<T> : IOncePerBackend
        where T : class, IPersistenceModelRealm
    {
        Task<bool> CreateAsync(T model);

        Task UpdateAsync(T model);

        Task DeleteAsync(T model);
    }
}