using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Sql.Models.Persistence;

namespace BFF.Persistence.Sql.ORM.Interfaces
{
    public interface ICrudOrm<T> : IOncePerBackend
        where T : class, IPersistenceModelSql
    {
        Task<T> ReadAsync(long id);

        Task<long> CreateAsync(T model);

        Task<IEnumerable<T>> ReadAllAsync();

        Task UpdateAsync(T model);

        Task DeleteAsync(T model);
    }
}