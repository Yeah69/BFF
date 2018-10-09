using System.Data;
using System.Data.Common;

namespace BFF.DB
{
    public interface IProvideConnection : IOncePerBackend
    {
        IDbConnection Connection { get; }

        void Backup(string reason);
    }
}