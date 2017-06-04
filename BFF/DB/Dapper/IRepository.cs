using BFF.MVVM.Models.Native.Structure;

namespace BFF.DB.Dapper
{
    public interface IRepository<T> where T : class, IDataModel
    {
        void Add(T dataModel);
        void Update(T dataModel);
        void Delete(T dataModel);
        T Find(long id);
    }
}