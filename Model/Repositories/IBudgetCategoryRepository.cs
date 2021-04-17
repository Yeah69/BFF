using System;
using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Model.Models;

namespace BFF.Model.Repositories
{
    public interface IBudgetCategoryRepository : IDisposable, IOncePerBackend
    {
        Task<IBudgetCategory> FindAsync(ICategory category);
    }
}