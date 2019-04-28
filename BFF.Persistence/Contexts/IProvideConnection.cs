using BFF.Core.IoC;

namespace BFF.Persistence.Contexts
{
    public interface IProvideConnection<out T> : IOncePerBackend
    {
        T Connection { get; }

        void Backup(string reason);
    }
}