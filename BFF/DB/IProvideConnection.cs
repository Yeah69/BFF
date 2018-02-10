using System.Data.Common;

namespace BFF.DB
{
    public interface IProvideConnection : IOncePerBackend
    {
        DbConnection Connection { get; }

        void Backup(string reason);
    }
}