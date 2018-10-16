using System.Threading.Tasks;
using BFF.Core.IoCMarkerInterfaces;
using BFF.Persistence.Models;

namespace BFF.Persistence.ORM.Interfaces
{
    public interface IMergeOrm : IOncePerBackend
    {
        Task MergePayeeAsync(PayeeDto from, PayeeDto to);
        Task MergeFlagAsync(FlagDto from, FlagDto to);
        Task MergeCategoryAsync(CategoryDto from, CategoryDto to);
    }
}