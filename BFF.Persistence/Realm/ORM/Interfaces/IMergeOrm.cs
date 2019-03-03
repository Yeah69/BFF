using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Realm.Persistence.Models;

namespace BFF.Persistence.Realm.ORM.Interfaces
{
    public interface IMergeOrm : IOncePerBackend
    {
        Task MergePayeeAsync(IPayeeRealm from, IPayeeRealm to);
        Task MergeFlagAsync(IFlagRealm from, IFlagRealm to);
        Task MergeCategoryAsync(ICategoryRealm from, ICategoryRealm to);
    }
}