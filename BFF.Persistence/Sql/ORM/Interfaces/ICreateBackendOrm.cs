using System.Threading.Tasks;

namespace BFF.Persistence.Sql.ORM.Interfaces
{
    public interface ICreateBackendOrm
    {
        Task CreateAsync();
    }
}