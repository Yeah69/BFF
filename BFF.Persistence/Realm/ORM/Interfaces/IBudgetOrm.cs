using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Realm.Models.Persistence;

namespace BFF.Persistence.Realm.ORM.Interfaces
{
    internal interface IBudgetOrm : IOncePerBackend
    {
        Task<BudgetBlock> FindAsync(int year);
        Task<IReadOnlyList<(BudgetEntry Entry, DateTime Month, long Budget, long Outflow, long Balance)>> FindAsync(int year, Category category);
    }
}