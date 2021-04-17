using System;
using System.Threading.Tasks;

namespace BFF.Model.IoC
{
    public interface IProjectContext : IDisposable
    {
        Task CreateProject();
    }
}
