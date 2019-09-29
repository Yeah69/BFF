using System.Threading.Tasks;
using BFF.Persistence.Import;

namespace BFF.Persistence.Realm.ORM.Interfaces
{
    public interface IExportingOrm
    {
        Task PopulateDatabaseAsync(IRealmExportContainerData exportContainer);
    }
}