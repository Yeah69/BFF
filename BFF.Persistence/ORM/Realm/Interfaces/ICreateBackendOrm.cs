using System.Threading.Tasks;

namespace BFF.Persistence.ORM.Realm.Interfaces
{
    public interface ICreateBackendOrm
    {
        Task CreateAsync();
    }
}