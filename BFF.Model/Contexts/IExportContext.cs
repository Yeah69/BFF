using BFF.Model.Import;

namespace BFF.Model.Contexts
{
    public interface IExportContext
    {
        void Export(DtoImportContainer container);
    }
}