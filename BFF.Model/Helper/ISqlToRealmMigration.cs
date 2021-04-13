using System.Threading.Tasks;

namespace BFF.Model.Helper
{
    public interface ISqlToRealmMigration
    {
        Task JustDoIt();
    }
}