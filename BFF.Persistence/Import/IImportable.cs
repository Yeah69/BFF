using System.Threading.Tasks;

namespace BFF.Persistence.Import
{
    public interface IImportable
    {
        Task<string> Import();
    }
}
