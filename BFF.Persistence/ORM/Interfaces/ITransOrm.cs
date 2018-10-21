using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Models;

namespace BFF.Persistence.ORM.Interfaces
{
    public interface ITransOrm : IOncePerBackend
    {
        Task<IEnumerable<ITransDto>> GetPageFromSpecificAccountAsync(int offset, int pageSize, long accountId);
        Task<IEnumerable<ITransDto>> GetPageFromSummaryAccountAsync(int offset, int pageSize);
        Task<IEnumerable<ITransDto>> GetFromMonthAsync(DateTime month);
        Task<IEnumerable<ITransDto>> GetFromMonthAndCategoryAsync(DateTime month, long categoryId);
        Task<IEnumerable<ITransDto>> GetFromMonthAndCategoriesAsync(DateTime month, long[] categoryIds);

        Task<long> GetCountFromSpecificAccountAsync(long accountId);
        Task<long> GetCountFromSummaryAccountAsync();
    }
}