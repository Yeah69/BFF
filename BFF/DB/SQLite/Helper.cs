using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
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

                IEnumerable<Transaction> transtactions = cnn.GetAll<Transaction>();
                IEnumerable<Transaction> nullSumsTransactions = transtactions.Where(t => t.Sum == null);
                foreach (Transaction t in nullSumsTransactions)
                {
                    t.SubTransactions = SubTransaction.GetFromDb(t.Id);
                }
                IEnumerable<Income> incomes = cnn.GetAll<Income>();
                //todo: Income need "SubIncome"s or else the ParentId may collide with the SubTransactions Table (will be needed to adjust in Import? Maybe not, because Income is a special category there, therefore cannot be split)
                //IEnumerable<Income> nullSumIncomes = incomes.Where(i => i.Sum == null);
                //foreach (Income i in nullSumIncomes)
                //{
                //    SubTransaction.GetFromDb(i.Id);
                //}
                IEnumerable<Transfer> transfers = cnn.GetAll<Transfer>();

                list.AddRange(transtactions);
                list.AddRange(incomes);
                list.AddRange(transfers);

                ClearCache();

                //stopwatch.Stop();
                //TimeSpan ts = stopwatch.Elapsed;
                //string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
                //Console.WriteLine($"The elapsed time is {elapsedTime}");

                cnn.Close();
            }
            return list;
        }

        public static void ClearCache()
        {
            Account.ClearCache();
            Category.ClearCache();
            Payee.ClearCache();
        }

        #endregion

        #endregion
    }
}
