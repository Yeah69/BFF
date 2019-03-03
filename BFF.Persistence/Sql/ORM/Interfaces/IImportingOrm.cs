using System.Threading.Tasks;
using BFF.Persistence.Import;

namespace BFF.Persistence.Sql.ORM.Interfaces
{
    public interface IImportingOrm
    {
        Task PopulateDatabaseAsync(ISqliteYnab4CsvImportContainerData sqliteYnab4CsvImportContainer);
    }
}