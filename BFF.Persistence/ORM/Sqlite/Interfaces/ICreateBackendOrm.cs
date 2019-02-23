using System.Threading.Tasks;

namespace BFF.Persistence.ORM.Sqlite.Interfaces
{
    public interface ICreateBackendOrm
    {
        Task CreateAsync();
    }
}