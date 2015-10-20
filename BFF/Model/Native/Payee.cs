using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;
using static BFF.DB.SQLite.Helper;

namespace BFF.Model.Native
{
    class Payee : DataModelBase
    {
        [Key]
        public override long Id { get; set; } = -1;

        public string Name { get; set; }
        
        public override string ToString()
        {
            return Name;
        }
        
        private static readonly Dictionary<string, Payee> Cache = new Dictionary<string, Payee>();
        private static readonly Dictionary<long, Payee> DbCache = new Dictionary<long, Payee>();

        [Write(false)]
        public static string CreateTableStatement => $@"CREATE TABLE [{nameof(Payee)}s](
                        {nameof(Id)} INTEGER PRIMARY KEY,
                        {nameof(Name)} VARCHAR(100));";

        public static Payee GetOrCreate(string name)
        {
            if (Cache.ContainsKey(name))
                return Cache[name];
            Payee payee = new Payee { Name = name };
            Cache.Add(name, payee);
            return payee;
        }

        public static List<Payee> GetAllCache()
        {
            return Cache.Values.ToList();
        }

        public static Payee GetFromDb(long id)
        {
            if (DbCache.ContainsKey(id)) return DbCache[id];
            Payee ret;
            using (var cnn = new SQLiteConnection(CurrentDbConnectionString()))
            {
                cnn.Open();

                ret = cnn.Get<Payee>(id);

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
