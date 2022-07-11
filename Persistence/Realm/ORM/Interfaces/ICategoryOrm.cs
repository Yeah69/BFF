using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Realm.Models.Persistence;

namespace BFF.Persistence.Realm.ORM.Interfaces
{
    internal interface ICategoryOrm : IScopeInstance
    {
        Task<IEnumerable<Category>> ReadCategoriesAsync();
        Task<IEnumerable<Category>> ReadIncomeCategoriesAsync();
    }
}