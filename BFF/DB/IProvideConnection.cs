using System.Data.Common;

namespace BFF.DB
{
    public interface IProvideConnection
    {
        DbConnection Connection { get; }    
    }
}