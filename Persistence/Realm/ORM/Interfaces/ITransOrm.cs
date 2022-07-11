using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Realm.Models.Persistence;

namespace BFF.Persistence.Realm.ORM.Interfaces
{
    internal interface ITransOrm : IScopeInstance
    {
        Task<IEnumerable<Trans>> GetPageFromSpecificAccountAsync(int offset, int pageSize, Account account);
        Task<IEnumerable<Trans>> GetPageFromSummaryAccountAsync(int offset, int pageSize);
        Task<IEnumerable<Trans>> GetFromMonthAsync(DateTime month);
        Task<IEnumerable<Trans>> GetFromMonthAndCategoryAsync(DateTime month, Category category);
        Task<IEnumerable<Trans>> GetFromMonthAndCategoriesAsync(DateTime month, Category[] categories);

        Task<long> GetCountFromSpecificAccountAsync(Account account);
        Task<long> GetCountFromSummaryAccountAsync();
    }
}