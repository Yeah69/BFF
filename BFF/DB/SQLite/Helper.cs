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

        public static string CurrentDataBaseName;

        #endregion

        #region Static Methods

        public static string CurrentFileName()
        {
            return string.Format("{0}.sqlite", CurrentDataBaseName);
        }

        public static string CurrentConnectionString()
        {
            return string.Format("Data Source={0}.sqlite;Version=3;", CurrentDataBaseName);
        }

        #endregion

        #endregion
    }
}
