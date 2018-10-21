using System.Data;
using BFF.Core.IoC;

namespace BFF.Core.Persistence
{
    public interface IProvideConnection : IOncePerBackend
    {
        IDbConnection Connection { get; }

        void Backup(string reason);
    }
}