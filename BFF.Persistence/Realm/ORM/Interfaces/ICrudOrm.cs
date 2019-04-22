using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Realm.Models.Persistence;

namespace BFF.Persistence.Realm.ORM.Interfaces
{
    public interface ICrudOrm<T> : IOncePerBackend
        where T : class, IPersistenceModelRealm
    {
        Task<bool> CreateAsync(T model);

        Task<IEnumerable<T>> ReadAllAsync();

        Task UpdateAsync(T model);

        Task DeleteAsync(T model);
    }
}