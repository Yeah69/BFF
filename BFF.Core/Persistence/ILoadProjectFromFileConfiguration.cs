using BFF.Core.IoC;

namespace BFF.Core.Persistence
{
    public interface ILoadProjectFromFileConfiguration : IConfiguration
    {
        string Path { get; }

        BackendChoice BackendChoice { get; }
    }
}