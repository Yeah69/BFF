using BFF.Model.Import;
using System;
using System.Threading.Tasks;

namespace BFF.Model.IoC
{
    public interface IProjectContext : IDisposable
    {
        Task CreateProject();

        Task CreateProject(DtoImportContainer container);

        Task<DtoImportContainer> ExtractProject();
    }
}
