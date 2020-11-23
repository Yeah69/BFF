using BFF.Model.Contexts;
using BFF.Model.Import;

namespace BFF.Persistence.Sql
{
    public class SqliteContext : IExportContext
    {
        public void Export(DtoImportContainer container)
        {
            thow new System.NotImplementedException();
        }
    }
}