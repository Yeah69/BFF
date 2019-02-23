using System.Threading.Tasks;
using BFF.Core.IoC;

namespace BFF.Persistence.ORM.Sqlite.Interfaces
{
    public interface IBudgetOrm : IOncePerBackend
    {
        Task<BudgetBlock> FindAsync(int year, long[] categoryIds, (long Id, int MonthOffset)[] incomeCategories);
    }
}