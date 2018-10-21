using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Models;

namespace BFF.Persistence.ORM.Interfaces
{
    public interface IMergeOrm : IOncePerBackend
    {
        Task MergePayeeAsync(IPayeeDto from, IPayeeDto to);
        Task MergeFlagAsync(IFlagDto from, IFlagDto to);
        Task MergeCategoryAsync(ICategoryDto from, ICategoryDto to);
    }
}