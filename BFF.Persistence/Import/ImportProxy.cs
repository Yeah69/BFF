using BFF.Model.Models.Utility;

namespace BFF.Persistence.Import
{
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
