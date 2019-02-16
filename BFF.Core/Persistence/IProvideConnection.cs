using BFF.Core.IoC;

namespace BFF.Core.Persistence
{
    public interface IProvideConnection<out T> : IOncePerBackend
    {
        T Connection { get; }

        void Backup(string reason);
    }
}