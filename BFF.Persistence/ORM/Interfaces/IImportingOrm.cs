using System.Threading.Tasks;
using BFF.Persistence.Import;

namespace BFF.Persistence.ORM.Interfaces
{
    public interface IImportingOrm
    {
        Task PopulateDatabaseAsync(ImportLists importLists, ImportAssignments importAssignments);
    }
}