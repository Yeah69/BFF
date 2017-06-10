using BFF.MVVM.Models.Native;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.DB.SQLite
{
    static class SqLiteQueries
    {
        internal static readonly int DatabaseSchemaVersion = 2;

        internal static string SetDatabaseSchemaVersion = $@"PRAGMA user_version = {DatabaseSchemaVersion};";
    }
}
