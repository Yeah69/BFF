using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using BFF.Model.Native.Structure;
using Dapper;

namespace BFF.Model.Native
{
    class Payee : DataModelBase
    {
        #region Non-Static

        #region Properties

        [PrimaryKey]
        public int ID { get; set; }

        [DataField]
        public string Name { get; set; }

        #endregion

        #region Methods

        protected override string GetDelimitedCreateTableList(string delimiter)
        {
            List<string> list = new List<string>{"ID INTEGER PRIMARY KEY AUTOINCREMENT",
                "Name VARCHAR(100)"
            };
            return string.Join(delimiter, list);
        }

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
