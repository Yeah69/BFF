
namespace BFF.DB.SQLite
{
    class Helper
    {
        #region Non-Static

        #region Properties



        #endregion

        #region Methods



        #endregion

        #endregion

        #region Static

        #region Static Variables

        public static string CurrentDbName { get; set; }

        #endregion

        #region Static Methods

        public static string CurrentDbFileName()
        {
            return $"{CurrentDbName}.sqlite";
        }

        public static string CurrentDbConnectionString()
        {
            return $"Data Source={CurrentDbName}.sqlite;Version=3;";
        }

        #endregion

        #endregion
    }
}
