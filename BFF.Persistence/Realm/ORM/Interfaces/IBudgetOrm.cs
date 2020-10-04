using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Common;
using BFF.Persistence.Realm.Models.Persistence;

namespace BFF.Persistence.Realm.ORM.Interfaces
{
    internal interface IBudgetOrm : IOncePerBackend
    {
        Task<BudgetBlock> FindAsync(int year);
        Task<IReadOnlyList<(BudgetEntry Entry, BudgetEntryData Data)>> FindAsync(int year, Category category);

        Task<long> GetAverageBudgetOfLastMonths(int currentMonthIndex, Category category, int monthCount);
        Task<long> GetAverageOutflowOfLastMonths(int currentMonthIndex, Category category, int monthCount);
        Task SetEmptyBudgetEntriesToAvgBudget(int monthIndex, int monthCount);
        Task SetEmptyBudgetEntriesToAvgOutflow(int monthIndex, int monthCount);
        Task SetEmptyBudgetEntriesToBalanceZero(int monthIndex);
        Task SetAllBudgetEntriesToZero(int monthIndex);
    }
}