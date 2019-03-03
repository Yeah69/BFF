using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Realm.Persistence.Models;

namespace BFF.Persistence.Realm.ORM.Interfaces
{
    public interface ITransOrm : IOncePerBackend
    {
        Task<IEnumerable<ITransRealm>> GetPageFromSpecificAccountAsync(int offset, int pageSize, IAccountRealm account);
        Task<IEnumerable<ITransRealm>> GetPageFromSummaryAccountAsync(int offset, int pageSize);
        Task<IEnumerable<ITransRealm>> GetFromMonthAsync(DateTime month);
        Task<IEnumerable<ITransRealm>> GetFromMonthAndCategoryAsync(DateTime month, ICategoryRealm category);
        Task<IEnumerable<ITransRealm>> GetFromMonthAndCategoriesAsync(DateTime month, ICategoryRealm[] categories);

        Task<long> GetCountFromSpecificAccountAsync(IAccountRealm account);
        Task<long> GetCountFromSummaryAccountAsync();
    }
}