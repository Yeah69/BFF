using BFF.Persistence.Import;

namespace BFF.Persistence.Contexts
{
    public interface IImportContext
    {
        IImportable Importable { get; }
    }
}