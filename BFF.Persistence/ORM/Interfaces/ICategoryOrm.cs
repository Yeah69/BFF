using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoCMarkerInterfaces;
using BFF.Persistence.Models;

namespace BFF.Persistence.ORM.Interfaces
{
    public interface ICategoryOrm : IOncePerBackend
    {
        Task<IEnumerable<Category>> ReadCategoriesAsync();
        Task<IEnumerable<Category>> ReadIncomeCategoriesAsync();
    }
}