using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.DB
{
    public interface IRepository<T> where T : class, IDataModel
    {
        T Create();
        void Add(T dataModel, DbConnection connection = null);
        void Update(T dataModel, DbConnection connection = null);
        void Delete(T dataModel, DbConnection connection = null);
        T Find(long id, DbConnection connection = null);
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

    public interface ICollectiveRepository<T> where T : class, IDataModel
    {
        IEnumerable<T> FindAll(DbConnection connection = null);
    }

    public interface ICreateTable
    {
        void CreateTable(DbConnection connection = null);
    }

    public interface ICreateDatabase
    {
        IProvideConnection Create();
    }

    public interface IViewRepository<out TDomainBase, in TSpecifying>
        : ISpecifiedPagedAccess<TDomainBase, TSpecifying>
        where TDomainBase : class, IDataModel
    { }

    public interface IViewRepositoryAsync<TDomainBase, in TSpecifying>
        : ISpecifiedPagedAccessAsync<TDomainBase, TSpecifying>
        where TDomainBase : class, IDataModel
    { }

    public interface IDbTableRepository<T> : IRepository<T>, ICollectiveRepository<T>
        where T : class, IDataModel { }
}