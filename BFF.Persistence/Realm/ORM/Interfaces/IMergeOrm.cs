using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Realm.Models.Persistence;

namespace BFF.Persistence.Realm.ORM.Interfaces
{
    internal interface IMergeOrm : IOncePerBackend
    {
        Task MergePayeeAsync(Payee from, Payee to);
        Task MergeFlagAsync(Flag from, Flag to);
        Task MergeCategoryAsync(Category from, Category to);
    }
}