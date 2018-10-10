using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoCMarkerInterfaces;
using BFF.Persistence.Models;

namespace BFF.Persistence.ORM.Interfaces
{
    public interface ITransOrm : IOncePerBackend
    {
        Task<IEnumerable<Trans>> GetPageFromSpecificAccountAsync(int offset, int pageSize, long accountId);
        Task<IEnumerable<Trans>> GetPageFromSummaryAccountAsync(int offset, int pageSize);
        Task<IEnumerable<Trans>> GetFromMonthAsync(DateTime month);
        Task<IEnumerable<Trans>> GetFromMonthAndCategoryAsync(DateTime month, long categoryId);
        Task<IEnumerable<Trans>> GetFromMonthAndCategoriesAsync(DateTime month, long[] categoryIds);

        Task<long> GetCountFromSpecificAccountAsync(long accountId);
        Task<long> GetCountFromSummaryAccountAsync();
    }
}