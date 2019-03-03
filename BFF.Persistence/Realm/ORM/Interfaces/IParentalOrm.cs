using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Realm.Persistence.Models;

namespace BFF.Persistence.Realm.ORM.Interfaces
{
    public interface IParentalOrm : IOncePerBackend
    {
        Task<IEnumerable<ISubTransactionRealm>> ReadSubTransactionsOfAsync(ITransRealm parentTransaction);
    }
}