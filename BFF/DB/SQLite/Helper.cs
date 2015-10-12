using System.Collections.Generic;
using System.Data.SQLite;
using BFF.Model.Native;
using BFF.Model.Native.Structure;
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

        public static List<ITransactionLike> GetAllTransactions()
        {
            CurrentDbName = "testDatabase";
            List<ITransactionLike> list = new List<ITransactionLike>();
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString()))
            {
                cnn.Open();

                //Stopwatch stopwatch = new Stopwatch();
                //stopwatch.Start();

                list.AddRange(cnn.GetAll<Transaction>());
                list.AddRange(cnn.GetAll<Income>());
                list.AddRange(cnn.GetAll<Transfer>());

                //todo: Clear Cache

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
