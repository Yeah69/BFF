using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Persistence.Models;

namespace BFF.Persistence.ORM
{
    public interface ICrudOrmCommon<T> where T : class, IPersistenceModel
    {
        Task<bool> CreateAsync(T model);
        
        Task<IEnumerable<T>> ReadAllAsync();

        Task UpdateAsync(T model);

        Task DeleteAsync(T model);
    }
}
