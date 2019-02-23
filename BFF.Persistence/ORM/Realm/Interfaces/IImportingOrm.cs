using System.Threading.Tasks;
using BFF.Persistence.Import;

namespace BFF.Persistence.ORM.Realm.Interfaces
{
    public interface IImportingOrm
    {
        Task PopulateDatabaseAsync(IRealmYnab4CsvImportContainerData sqliteYnab4CsvImportContainer);
    }
}