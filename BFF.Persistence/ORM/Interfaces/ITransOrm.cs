using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoCMarkerInterfaces;
using BFF.Persistence.Models;

namespace BFF.Persistence.ORM.Interfaces
{
    public interface ITransOrm : IOncePerBackend
    {
        Task<IEnumerable<TransDto>> GetPageFromSpecificAccountAsync(int offset, int pageSize, long accountId);
        Task<IEnumerable<TransDto>> GetPageFromSummaryAccountAsync(int offset, int pageSize);
        Task<IEnumerable<TransDto>> GetFromMonthAsync(DateTime month);
        Task<IEnumerable<TransDto>> GetFromMonthAndCategoryAsync(DateTime month, long categoryId);
        Task<IEnumerable<TransDto>> GetFromMonthAndCategoriesAsync(DateTime month, long[] categoryIds);

        Task<long> GetCountFromSpecificAccountAsync(long accountId);
        Task<long> GetCountFromSummaryAccountAsync();
    }
}