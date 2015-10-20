using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;
using static BFF.DB.SQLite.Helper;

namespace BFF.Model.Native
{
    class Account : DataModelBase
    {
        [Key]
        public override long Id { get; set; } = -1;

        public string Name { get; set; }

        public double StartingBalance { get; set; } = 0.0;

        public override string ToString()
        {
            return Name;
        }

        private static readonly Dictionary<string, Account> Cache = new Dictionary<string, Account>();
        private static readonly Dictionary<long, Account> DbCache = new Dictionary<long, Account>();

        [Write(false)]
        public static string CreateTableStatement => $@"CREATE TABLE [{nameof(Account)}s](
                        {nameof(Id)} INTEGER PRIMARY KEY,
                        {nameof(Name)} VARCHAR(100),
                        {nameof(StartingBalance)} FLOAT NOT NULL DEFAULT 0);";

        public static Account GetOrCreate(string name)
        {
            if (Cache.ContainsKey(name))
                return Cache[name];
            Account account = new Account {Name = name};
            Cache.Add(name, account);
            return account;
        }

        public static List<Account> GetAllCache()
        {
            return Cache.Values.ToList();
        }

        public static Account GetFromDb(long id)
        {
            if (DbCache.ContainsKey(id)) return DbCache[id];
            Account ret;
            using(var cnn = new SQLiteConnection(CurrentDbConnectionString()))
            {
                cnn.Open();

                ret = cnn.Get<Account>(id);

                cnn.Close();
            }
            DbCache.Add(id, ret);
            return ret;
        }

        public static void ClearCache()
        {
            DbCache.Clear();
        }
    }
}
