using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Model.Models.Structure;
using BFF.Persistence.Realm.Models.Persistence;

namespace BFF.Persistence.Realm.Repositories
{
    internal interface IRealmWriteOnlyRepository<in T> : IOncePerBackend
        where T : class, IDataModel
    {
        Task<bool> AddAsync(T dataModel);
    }
    internal interface IRealmReadOnlyRepository<TDomain, TPersistence> : IOncePerBackend
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModelRealm
    {
        Task<TDomain> FindAsync(TPersistence realmObject);
    }

    internal interface IRealmRepository<TDomain, TPersistence> : IRealmReadOnlyRepository<TDomain, TPersistence>, IRealmWriteOnlyRepository<TDomain>
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModelRealm
    {
    }

    internal interface IRealmCollectiveRepository<T> : IOncePerBackend
        where T : class, IDataModel
    {
        Task<IEnumerable<T>> FindAllAsync();
    }

    internal interface IRealmDbTableRepository<TDomain, TPersistence> : IRealmRepository<TDomain, TPersistence>, IRealmCollectiveRepository<TDomain>
        where TDomain : class, IDataModel
        where TPersistence : class, IPersistenceModelRealm
    { }
}