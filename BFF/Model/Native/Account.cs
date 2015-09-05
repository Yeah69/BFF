using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Windows.Media;
using BFF.Model.Native.Structure;

namespace BFF.Model.Native
{
    class Account : DataModelBase
    {
        #region Non-Static
        
        #region Properties

        [PrimaryKey]
        public int ID { get; set; }

        [DataField]
        public string Name { get; set; }

        [DataField]
        public double RunningBalance { get; set; }

        #endregion

        #region Methods

        protected override string GetDelimitedCreateTableList(string delimiter)
        {
            List<string> list = new List<string>{"ID INTEGER PRIMARY KEY AUTOINCREMENT",
                "Name VARCHAR(100)",
                "RunningBalance FLOAT"
            };
            return string.Join(delimiter, list);
        }

        #endregion

        #endregion

        #region Static

        #region Static Variables

        private static readonly Dictionary<string, Account> Cache = new Dictionary<string, Account>();

        #endregion

        #region Static Methods

        public static Account GetOrCreate(string name)
        {
            if (Cache.ContainsKey(name))
                return Cache[name];
            //todo: Find out about converting the ID
            //RunningBalance has to be calculated later on
            Account account = new Account {ID = 69, Name = name};
            Cache.Add(name, account);
            return account;
        }

        public static List<Account> GetAllCache()
        {
            return Cache.Values.ToList();
        }

        #endregion

        #endregion
    }
}
