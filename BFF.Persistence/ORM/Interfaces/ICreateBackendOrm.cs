using System.Threading.Tasks;

namespace BFF.Persistence.ORM.Interfaces
{
    public interface ICreateBackendOrm
    {
        Task CreateAsync();
    }
}