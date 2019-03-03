using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Realm.Persistence.Models;

namespace BFF.Persistence.Realm.ORM.Interfaces
{
    public interface IAccountOrm : IOncePerBackend
    {
        Task<long?> GetClearedBalanceAsync(IAccountRealm account);
        Task<long?> GetClearedBalanceUntilNowAsync(IAccountRealm account);
        Task<long?> GetClearedOverallBalanceAsync();
        Task<long?> GetClearedOverallBalanceUntilNowAsync();
        Task<long?> GetUnclearedBalanceAsync(IAccountRealm account);
        Task<long?> GetUnclearedBalanceUntilNowAsync(IAccountRealm account);
        Task<long?> GetUnclearedOverallBalanceAsync();
        Task<long?> GetUnclearedOverallBalanceUntilNowAsync();
    }
}