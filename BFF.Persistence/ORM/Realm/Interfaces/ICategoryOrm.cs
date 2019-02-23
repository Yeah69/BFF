using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Models.Realm;

namespace BFF.Persistence.ORM.Realm.Interfaces
{
    public interface ICategoryOrm : IOncePerBackend
    {
        Task<IEnumerable<ICategoryRealm>> ReadCategoriesAsync();
        Task<IEnumerable<ICategoryRealm>> ReadIncomeCategoriesAsync();
    }
}