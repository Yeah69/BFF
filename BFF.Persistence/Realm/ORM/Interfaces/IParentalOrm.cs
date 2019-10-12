using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Realm.Models.Persistence;

namespace BFF.Persistence.Realm.ORM.Interfaces
{
    internal interface IParentalOrm : IOncePerBackend
    {
        Task<IEnumerable<SubTransaction>> ReadSubTransactionsOfAsync(Trans parentTransaction);
    }
}