using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Models;

namespace BFF.Persistence.ORM.Interfaces
{
    public interface IParentalOrm : IOncePerBackend
    {
        Task<IEnumerable<ISubTransactionDto>> ReadSubTransactionsOfAsync(long parentTransactionId);
    }
}