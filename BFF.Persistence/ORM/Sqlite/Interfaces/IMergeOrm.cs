using System.Threading.Tasks;
using BFF.Core.IoC;
using BFF.Persistence.Models.Sql;

namespace BFF.Persistence.ORM.Sqlite.Interfaces
{
    public interface IMergeOrm : IOncePerBackend
    {
        Task MergePayeeAsync(IPayeeSql from, IPayeeSql to);
        Task MergeFlagAsync(IFlagSql from, IFlagSql to);
        Task MergeCategoryAsync(ICategorySql from, ICategorySql to);
    }
}