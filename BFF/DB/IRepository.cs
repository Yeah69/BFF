using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using BFF.DB.SQLite;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.DB
{
    public interface IWriteOnlyRepository<in T> : IOncePerBackend where T : class, IDataModel
    {
        void Add(T dataModel);
        void Update(T dataModel);
        void Delete(T dataModel);
    }
    public interface IReadOnlyRepository<out T> : IOncePerBackend where T : class, IDataModel
    {
        T Find(long id);
    }

    public interface IRepository<T> : 
        IReadOnlyRepository<T>, IWriteOnlyRepository<T>
        where T : class, IDataModel
    {
    }

    public interface ISpecifiedPagedAccess<out TDomainBase, in TSpecifying>
        where TDomainBase : class, IDataModel
    {
        IEnumerable<TDomainBase> GetPage(int offset, int pageSize, TSpecifying specifyingObject);
        long GetCount(TSpecifying specifyingObject);
    }

    public interface ISpecifiedPagedAccessAsync<TDomainBase, in TSpecifying>
        where TDomainBase : class, IDataModel
    {
        Task<IEnumerable<TDomainBase>> GetPageAsync(int offset, int pageSize, TSpecifying specifyingObject);
        Task<long> GetCountAsync(TSpecifying specifyingObject);
    }

    public interface ICollectiveRepository<out T> : IOncePerBackend where T : class, IDataModel
    {
        IEnumerable<T> FindAll();
    }

    public interface IDbTableRepository<T> : IRepository<T>, ICollectiveRepository<T>
        where T : class, IDataModel { }
}