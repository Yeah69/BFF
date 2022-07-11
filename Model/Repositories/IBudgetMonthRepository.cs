using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Model.Models;

namespace BFF.Model.Repositories
{
    public interface IBudgetMonthRepository : IDisposable, IScopeInstance
    {
        Task<IList<IBudgetMonth>> FindAsync(int year);

        Task<long> GetAvailableToBudgetOfCurrentMonth();
    }
}