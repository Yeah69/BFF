using BFF.Model.Import;
using System.Threading.Tasks;

namespace BFF.Model.Contexts
{
    public interface IExportContext : IContext
    {
        Task Export(DtoImportContainer container);
    }
}