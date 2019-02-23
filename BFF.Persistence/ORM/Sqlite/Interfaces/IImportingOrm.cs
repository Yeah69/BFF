using System.Threading.Tasks;
using BFF.Persistence.Import;

namespace BFF.Persistence.ORM.Sqlite.Interfaces
{
    public interface IImportingOrm
    {
        Task PopulateDatabaseAsync(ISqliteYnab4CsvImportContainerData sqliteYnab4CsvImportContainer);
    }
}