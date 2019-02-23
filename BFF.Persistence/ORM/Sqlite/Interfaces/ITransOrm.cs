using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Models.Sql;

namespace BFF.Persistence.ORM.Sqlite.Interfaces
{
    public interface ITransOrm : IOncePerBackend
    {
        Task<IEnumerable<ITransSql>> GetPageFromSpecificAccountAsync(int offset, int pageSize, long accountId);
        Task<IEnumerable<ITransSql>> GetPageFromSummaryAccountAsync(int offset, int pageSize);
        Task<IEnumerable<ITransSql>> GetFromMonthAsync(DateTime month);
        Task<IEnumerable<ITransSql>> GetFromMonthAndCategoryAsync(DateTime month, long categoryId);
        Task<IEnumerable<ITransSql>> GetFromMonthAndCategoriesAsync(DateTime month, long[] categoryIds);

        Task<long> GetCountFromSpecificAccountAsync(long accountId);
        Task<long> GetCountFromSummaryAccountAsync();
    }
}