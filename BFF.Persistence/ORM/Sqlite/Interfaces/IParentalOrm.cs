using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Models.Sql;

namespace BFF.Persistence.ORM.Sqlite.Interfaces
{
    public interface IParentalOrm : IOncePerBackend
    {
        Task<IEnumerable<ISubTransactionSql>> ReadSubTransactionsOfAsync(long parentTransactionId);
    }
}