using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Model.Models.Structure;
using BFF.Persistence.Models;

namespace BFF.Model.Repositories
{
    internal interface IWriteOnlyRepository<in T> : IOncePerBackend 
        where T : class, IDataModel
    {
        Task<bool> AddAsync(T dataModel);
        Task UpdateAsync(T dataModel);
        Task DeleteAsync(T dataModel);
    }
    internal interface IReadOnlyRepository<T> : IOncePerBackend
        where T : class, IDataModel
    {
        Task<T> FindAsync(long id);
    }
    internal interface IMergingRepository<T> : IOncePerBackend 
        where T : class, IDataModel
    {
        Task MergeAsync(T from, T to);
    }

    internal interface IRepository<TDomain, TPersistence> : IReadOnlyRepository<TDomain>, IWriteOnlyRepository<TDomain>
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModel
    {
    }

    public interface ISpecifiedPagedAccessAsync<TDomainBase, in TSpecifying>
        where TDomainBase : class, IDataModel
    {
        Task<IEnumerable<TDomainBase>> GetPageAsync(int offset, int pageSize, TSpecifying specifyingObject);
        Task<long> GetCountAsync(TSpecifying specifyingObject);
    }

    internal interface ICollectiveRepository<T> : IOncePerBackend 
        where T : class, IDataModel
    {
        Task<IEnumerable<T>> FindAllAsync();
    }

    internal interface IDbTableRepository<TDomain, TPersistence> : IRepository<TDomain, TPersistence>, ICollectiveRepository<TDomain>
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModel
    { }
}