using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.ORM.Sqlite;

namespace BFF.Persistence.ORM.Interfaces
{
    public interface IBudgetOrm : IOncePerBackend
    {
        Task<BudgetBlock> FindAsync(int year, long[] categoryIds, (long Id, int MonthOffset)[] incomeCategories);
    }
}