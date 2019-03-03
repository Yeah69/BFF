using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Sql.Models.Persistence;

namespace BFF.Persistence.Sql.ORM.Interfaces
{
    public interface IParentalOrm : IOncePerBackend
    {
        Task<IEnumerable<ISubTransactionSql>> ReadSubTransactionsOfAsync(long parentTransactionId);
    }
}