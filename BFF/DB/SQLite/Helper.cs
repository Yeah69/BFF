using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static string CurrentDBName { get; set; }

        #endregion

        #region Static Methods

        public static string CurrentDBFileName()
        {
            return $"{CurrentDBName}.sqlite";
        }

        public static string CurrentDBConnectionString()
        {
            return $"Data Source={CurrentDBName}.sqlite;Version=3;";
        }

        #endregion

        #endregion
    }
}
