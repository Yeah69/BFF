using System.Threading.Tasks;
using BFF.Persistence.Import;

namespace BFF.Persistence.Realm.ORM.Interfaces
{
    internal interface IExportingOrm
    {
        Task PopulateDatabaseAsync(IRealmExportContainerData exportContainer);
    }
}