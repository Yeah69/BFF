using System.Collections.Generic;
using System.Linq;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native
{
    class Payee : DataModelBase
    {
        #region Non-Static

        #region Properties

        [Key]
        public override long Id { get; set; } = -1;

        public string Name { get; set; }

        #endregion

        #region Methods



        #endregion

        #endregion

        #region Static

        #region Static Variables

        private static readonly Dictionary<string, Payee> Cache = new Dictionary<string, Payee>();
        
        public static string CreateTableStatement => $@"CREATE TABLE [{nameof(Payee)}s](
                        {nameof(Id)} INTEGER PRIMARY KEY,
                        {nameof(Name)} VARCHAR(100));";

        #endregion

        #region Static Methods

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

        #endregion

        #endregion
    }
}
