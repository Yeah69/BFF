using System.Threading.Tasks;
using BFF.Core.IoC;

namespace BFF.Persistence.ORM.Interfaces
{
    public interface IAccountOrm : IOncePerBackend
    {
        Task<long?> GetClearedBalanceAsync(long id);
        Task<long?> GetClearedBalanceUntilNowAsync(long id);
        Task<long?> GetClearedOverallBalanceAsync();
        Task<long?> GetClearedOverallBalanceUntilNowAsync();
        Task<long?> GetUnclearedBalanceAsync(long id);
        Task<long?> GetUnclearedBalanceUntilNowAsync(long id);
        Task<long?> GetUnclearedOverallBalanceAsync();
        Task<long?> GetUnclearedOverallBalanceUntilNowAsync();
    }
}