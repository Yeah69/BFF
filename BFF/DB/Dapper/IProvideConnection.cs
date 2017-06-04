using System.Data.Common;

namespace BFF.DB.Dapper.ModelRepositories
{
    public interface IProvideConnection
    {
        DbConnection Connection { get; }    
    }
}