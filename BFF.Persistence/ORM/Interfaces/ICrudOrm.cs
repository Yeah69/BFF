using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Models;

namespace BFF.Persistence.ORM.Interfaces
{
    public interface ICrudOrm : IOncePerBackend
    {
        Task CreateAsync<T>(T model) where T : class, IPersistenceModelDto;
        Task CreateAsync<T>(IEnumerable<T> models) where T : class, IPersistenceModelDto;

        Task<T> ReadAsync<T>(long id) where T : class, IPersistenceModelDto;
        Task<IEnumerable<T>> ReadAllAsync<T>() where T : class, IPersistenceModelDto;

        Task UpdateAsync<T>(T model) where T : class, IPersistenceModelDto;
        Task UpdateAsync<T>(IEnumerable<T> models) where T : class, IPersistenceModelDto;

        Task DeleteAsync<T>(T model) where T : class, IPersistenceModelDto;
        Task DeleteAsync<T>(IEnumerable<T> models) where T : class, IPersistenceModelDto;
    }
}