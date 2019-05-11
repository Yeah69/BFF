using System;
using System.Threading.Tasks;

namespace BFF.Core.IoC
{
    public interface ICreateProjectContext : IDisposable
    {
        Task CreateProjectAsync();
    }
}
