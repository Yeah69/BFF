using System.Data.Common;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.DB
{
    public interface IRepository<T> where T : class, IDataModel
    {
        void Add(T dataModel, DbConnection connection = null);
        void Update(T dataModel, DbConnection connection = null);
        void Delete(T dataModel, DbConnection connection = null);
        T Find(long id, DbConnection connection = null);
    }

    public interface ICreateTable
    {
        void CreateTable(DbConnection connection = null);
    }

    public interface IDbTableRepository<T> : IRepository<T>, ICreateTable where T : class, IDataModel { }
}