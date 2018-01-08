using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using BFF.DB.SQLite;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.DB
{
    public interface IWriteOnlyRepository<in T> : IOncePerBackend where T : class, IDataModel
    {
        void Add(T dataModel, DbConnection connection = null);
        void Update(T dataModel, DbConnection connection = null);
        void Delete(T dataModel, DbConnection connection = null);
    }
    public interface IReadOnlyRepository<out T> : IOncePerBackend where T : class, IDataModel
    {
        T Find(long id, DbConnection connection = null);
    }

    public interface IRepository<T> : 
        IReadOnlyRepository<T>, IWriteOnlyRepository<T>
        where T : class, IDataModel
    {
    }

    public interface ISpecifiedPagedAccess<out TDomainBase, in TSpecifying>
        where TDomainBase : class, IDataModel
    {
        IEnumerable<TDomainBase> GetPage(int offset, int pageSize, TSpecifying specifyingObject,
            DbConnection connection = null);
        int GetCount(TSpecifying specifyingObject, DbConnection connection = null);
    }

    public interface ISpecifiedPagedAccessAsync<TDomainBase, in TSpecifying>
        where TDomainBase : class, IDataModel
    {
        Task<IEnumerable<TDomainBase>> GetPageAsync(int offset, int pageSize, TSpecifying specifyingObject,
            DbConnection connection = null);
        Task<int> GetCountAsync(TSpecifying specifyingObject, DbConnection connection = null);
    }

    public interface ICollectiveRepository<out T> : IOncePerBackend where T : class, IDataModel
    {
        IEnumerable<T> FindAll(DbConnection connection = null);
    }

    public interface ICreateTable : IOncePerBackend
    {
        void CreateTable(DbConnection connection = null);
    }

    public interface ICreateDatabase : IOncePerBackend
    {
        IProvideSqLiteConnection Create();
    }

    public interface IDbTableRepository<T> : IRepository<T>, ICollectiveRepository<T>
        where T : class, IDataModel { }
}