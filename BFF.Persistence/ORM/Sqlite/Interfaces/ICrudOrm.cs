using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Models.Sql;

namespace BFF.Persistence.ORM.Sqlite.Interfaces
{
    public interface ICrudOrm<T> : ICrudOrmCommon<T>, IOncePerBackend
        where T : class, IPersistenceModelSql
    {
        Task<T> ReadAsync(long id);
    }
}