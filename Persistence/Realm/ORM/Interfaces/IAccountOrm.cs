using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Realm.Models.Persistence;

namespace BFF.Persistence.Realm.ORM.Interfaces
{
    internal interface IAccountOrm : IScopeInstance
    {
        Task<long?> GetClearedBalanceAsync(Account account);
        Task<long?> GetClearedBalanceUntilNowAsync(Account account);
        Task<long?> GetClearedOverallBalanceAsync();
        Task<long?> GetClearedOverallBalanceUntilNowAsync();
        Task<long?> GetUnclearedBalanceAsync(Account account);
        Task<long?> GetUnclearedBalanceUntilNowAsync(Account account);
        Task<long?> GetUnclearedOverallBalanceAsync();
        Task<long?> GetUnclearedOverallBalanceUntilNowAsync();
    }
}