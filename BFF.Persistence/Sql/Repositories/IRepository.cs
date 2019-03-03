using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Model.Models.Structure;

namespace BFF.Persistence.Sql.Repositories
{
    internal interface IWriteOnlyRepository<in T> : IOncePerBackend 
        where T : class, IDataModel
    {
        Task<bool> AddAsync(T dataModel);
    }
    internal interface IReadOnlyRepository<T> : IOncePerBackend
        where T : class, IDataModel
    {
        Task<T> FindAsync(long id);
    }

    internal interface IRepository<TDomain> : IReadOnlyRepository<TDomain>, IWriteOnlyRepository<TDomain>
        where TDomain : class, IDataModel
    {
    }

    internal interface ICollectiveRepository<T> : IOncePerBackend 
        where T : class, IDataModel
    {
        Task<IEnumerable<T>> FindAllAsync();
    }

    internal interface IDbTableRepository<TDomain> : IRepository<TDomain>, ICollectiveRepository<TDomain>
        where TDomain : class, IDataModel
    { }
}