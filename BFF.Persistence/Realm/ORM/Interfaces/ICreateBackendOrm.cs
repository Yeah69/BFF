using System.Threading.Tasks;

namespace BFF.Persistence.Realm.ORM.Interfaces
{
    public interface ICreateBackendOrm
    {
        Task CreateAsync();
    }
}