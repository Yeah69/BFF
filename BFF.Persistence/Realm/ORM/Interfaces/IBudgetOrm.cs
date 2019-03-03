using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Realm.Persistence.Models;

namespace BFF.Persistence.Realm.ORM.Interfaces
{
    public interface IBudgetOrm : IOncePerBackend
    {
        Task<BudgetBlock> FindAsync(int year, ICategoryRealm[] categories, (ICategoryRealm category, int MonthOffset)[] incomeCategories);
    }
}