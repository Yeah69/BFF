using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Model.Models.Structure;

namespace BFF.Persistence.Sql.Repositories
{
    internal interface ISqliteWriteOnlyRepository<in T> : IOncePerBackend 
        where T : class, IDataModel
    {
        Task<bool> AddAsync(T dataModel);
    }
    internal interface ISqliteReadOnlyRepository<T> : IOncePerBackend
        where T : class, IDataModel
    {
        Task<T> FindAsync(long id);
    }

    internal interface ISqliteRepository<TDomain> : ISqliteReadOnlyRepository<TDomain>, ISqliteWriteOnlyRepository<TDomain>
        where TDomain : class, IDataModel
    {
    }

    internal interface ISqliteCollectiveRepository<T> : IOncePerBackend 
        where T : class, IDataModel
    {
        Task<IEnumerable<T>> FindAllAsync();
    }

    internal interface ISqliteDbTableRepository<TDomain> : ISqliteRepository<TDomain>, ISqliteCollectiveRepository<TDomain>
        where TDomain : class, IDataModel
    { }
}