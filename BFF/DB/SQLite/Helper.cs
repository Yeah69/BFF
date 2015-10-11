using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using BFF.Model.Native;
using Dapper.Contrib.Extensions;

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
            return $"Data Source={CurrentDbName}.sqlite;Version=3;foreign keys=true;";
        }

        public static List<Transaction> GetAllTransactions()
        {
            CurrentDbName = "testDatabase";
            List<Transaction> list;
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString()))
            {
                cnn.Open();

                //Stopwatch stopwatch = new Stopwatch();
                //stopwatch.Start();

                list = cnn.GetAll<Transaction>().ToList();

                //stopwatch.Stop();
                //TimeSpan ts = stopwatch.Elapsed;
                //string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
                //Console.WriteLine($"The elapsed time is {elapsedTime}");

                cnn.Close();
            }
            return list;
        }

        #endregion

        #endregion
    }
}
