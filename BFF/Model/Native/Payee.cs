using System.Collections.Generic;
using System.Linq;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native
{
    public class Payee : CommonProperties
    {

        public string Name { get; set; }
        
        public override string ToString()
        {
            return Name;
        }
        
        private static readonly Dictionary<string, Payee> Cache = new Dictionary<string, Payee>();

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

        public static void ClearCache()
        {
            Cache.Clear();
        }
    }
}
