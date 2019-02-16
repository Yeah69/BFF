using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Model.Models.Structure;

namespace BFF.Model.Repositories
{
    public interface IWriteOnlyRepository<in T> : IOncePerBackend where T : class, IDataModel
    {
        Task AddAsync(T dataModel);
        Task UpdateAsync(T dataModel);
        Task DeleteAsync(T dataModel);
    }
    internal interface IReadOnlyRepository<T> : IOncePerBackend where T : class, IDataModel
    {
        Task<T> FindAsync(long id);
    }
    public interface IMergingRepository<T> : IOncePerBackend where T : class, IDataModel
    {
        Task MergeAsync(T from, T to);
    }

    public interface IRepository<T> : IWriteOnlyRepository<T>
        where T : class, IDataModel
    {
    }

    public interface ISpecifiedPagedAccessAsync<TDomainBase, in TSpecifying>
        where TDomainBase : class, IDataModel
    {
        Task<IEnumerable<TDomainBase>> GetPageAsync(int offset, int pageSize, TSpecifying specifyingObject);
        Task<long> GetCountAsync(TSpecifying specifyingObject);
    }

    public interface ICollectiveRepository<T> : IOncePerBackend where T : class, IDataModel
    {
        Task<IEnumerable<T>> FindAllAsync();
    }

    public interface IDbTableRepository<T> : IRepository<T>, ICollectiveRepository<T>
        where T : class, IDataModel { }
}