namespace BFF.DB.SQLite
{
    internal static class SqLiteQueries
    {
        /// <summary>
        /// First for digits are the release year, next two digits are the release counter, next three are the patch counter
        /// </summary>
        internal static readonly int DatabaseSchemaVersion = 201801001;

        internal static string SetDatabaseSchemaVersion = $@"PRAGMA user_version = {DatabaseSchemaVersion};";
    }
}
