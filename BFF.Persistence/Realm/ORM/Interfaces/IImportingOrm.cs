using System.Threading.Tasks;
using BFF.Persistence.Import;

namespace BFF.Persistence.Realm.ORM.Interfaces
{
    public interface IImportingOrm
    {
        Task PopulateDatabaseAsync(IRealmYnab4CsvImportContainerData sqliteYnab4CsvImportContainer);
    }
}