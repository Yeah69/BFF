using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Models.Realm;

namespace BFF.Persistence.ORM.Realm.Interfaces
{
    public interface IBudgetOrm : IOncePerBackend
    {
        Task<BudgetBlock> FindAsync(int year, ICategoryRealm[] categories, (ICategoryRealm category, int MonthOffset)[] incomeCategories);
    }
}