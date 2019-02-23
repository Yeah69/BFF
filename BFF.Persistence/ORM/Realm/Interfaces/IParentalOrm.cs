using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Models.Realm;

namespace BFF.Persistence.ORM.Realm.Interfaces
{
    public interface IParentalOrm : IOncePerBackend
    {
        Task<IEnumerable<ISubTransactionRealm>> ReadSubTransactionsOfAsync(ITransRealm parentTransaction);
    }
}