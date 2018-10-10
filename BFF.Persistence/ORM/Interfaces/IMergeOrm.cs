using System.Threading.Tasks;
using BFF.Core.IoCMarkerInterfaces;
using BFF.Persistence.Models;

namespace BFF.Persistence.ORM.Interfaces
{
    public interface IMergeOrm : IOncePerBackend
    {
        Task MergePayeeAsync(Payee from, Payee to);
        Task MergeFlagAsync(Flag from, Flag to);
        Task MergeCategoryAsync(Category from, Category to);
    }
}