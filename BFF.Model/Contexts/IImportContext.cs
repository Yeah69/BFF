using BFF.Model.Import;
using System.Threading.Tasks;

namespace BFF.Model.Contexts
{
    public interface IImportContext : IContext
    {
        Task<DtoImportContainer> Import();
    }
}