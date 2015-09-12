using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using BFF.Model.Native.Structure;
using Dapper;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native
{
    [Table(nameof(Payee))]
    class Payee : DataModelBase
    {
        #region Non-Static

        #region Properties

        [Write(false)]
        public override string CreateTableStatement => $@"CREATE TABLE [{nameof(Payee)}](
                        {nameof(ID)} INTEGER PRIMARY KEY,
                        {nameof(Name)} VARCHAR(100));";

        [Key]
        public override long ID { get; set; }
        
        public string Name { get; set; }

        #endregion

        #region Methods



        #endregion

        #endregion

        #region Static

        #region Static Variables

        private static readonly Dictionary<string, Payee> Cache = new Dictionary<string, Payee>();

        #endregion

        #region Static Methods

        public static Payee GetOrCreate(string name)
        {
            if (Cache.ContainsKey(name))
                return Cache[name];
            //todo: Find out about converting the ID
            Payee payee = new Payee { ID = 69, Name = name };
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
