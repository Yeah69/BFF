using BFF.Model.Contexts;
using BFF.Model.ImportExport;

namespace BFF.Model.IoC
{
    public interface IContextFactory
    {
        bool ContextCanLoad { get; }
        bool ContextCanImport { get; }
        bool ContextCanExport { get; }
        bool CanCreate(IConfiguration configuration);
        IContext Create(IConfiguration configuration);
    }
}