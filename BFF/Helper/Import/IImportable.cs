using System.Threading.Tasks;

namespace BFF.Helper.Import
{
    public interface IImportable
    {
        string SavePath { get; set; }

        Task<string> Import();
    }
}
