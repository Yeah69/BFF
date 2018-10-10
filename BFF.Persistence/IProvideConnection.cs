using System.Data;
using BFF.Core.IoCMarkerInterfaces;

namespace BFF.Persistence
{
    public interface IProvideConnection : IOncePerBackend
    {
        IDbConnection Connection { get; }

        void Backup(string reason);
    }
}