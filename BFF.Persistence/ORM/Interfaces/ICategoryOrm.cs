using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Models;

namespace BFF.Persistence.ORM.Interfaces
{
    public interface ICategoryOrm : IOncePerBackend
    {
        Task<IEnumerable<ICategoryDto>> ReadCategoriesAsync();
        Task<IEnumerable<ICategoryDto>> ReadIncomeCategoriesAsync();
    }
}