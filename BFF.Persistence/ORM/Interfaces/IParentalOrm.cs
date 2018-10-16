using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoCMarkerInterfaces;
using BFF.Persistence.Models;

namespace BFF.Persistence.ORM.Interfaces
{
    public interface IParentalOrm : IOncePerBackend
    {
        Task<IEnumerable<SubTransactionDto>> ReadSubTransactionsOfAsync(long parentTransactionId);
    }
}