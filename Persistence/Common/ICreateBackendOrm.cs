using System.Threading.Tasks;

namespace BFF.Persistence.Common
{
    public interface ICreateBackendOrm
    {
        Task CreateAsync();
    }
}