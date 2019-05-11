using System.Threading.Tasks;

namespace BFF.Core.IoC
{
    public interface IImportContext
    {
        Task ImportAsync();
    }
}
