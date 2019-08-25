using System.Threading.Tasks;

namespace BFF.Core.Persistence
{
    public interface IImportExport
    {
        Task ImportExportAsync(
            IImportingConfiguration importingConfiguration,
            IExportingConfiguration exportingConfiguration);
    }
}
