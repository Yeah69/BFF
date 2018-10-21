using BFF.Persistence.Import;

namespace BFF.Model.Models.Utility
{
    public interface IImportProxy
    {
        void Import();
    }

    internal class ImportProxy : IImportProxy
    {
        private readonly IImportable _importable;

        public ImportProxy(IImportable importable)
        {
            _importable = importable;
        }

        public void Import()
        {
            _importable.Import();
        }
    }
}
