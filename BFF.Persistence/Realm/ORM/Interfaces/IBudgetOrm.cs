using System.Threading.Tasks;
using BFF.Core.IoC;

namespace BFF.Persistence.Realm.ORM.Interfaces
{
    internal interface IBudgetOrm : IOncePerBackend
    {
        Task<BudgetBlock> FindAsync(int year);
    }
}