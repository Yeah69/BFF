using System.Collections.Generic;
using System.Linq;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native
{
    class Account : DataModelBase
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

        private static readonly Dictionary<string, Account> Cache = new Dictionary<string, Account>();
        
        public static string CreateTableStatement => $@"CREATE TABLE [{nameof(Account)}s](
                        {nameof(Id)} INTEGER PRIMARY KEY,
                        {nameof(Name)} VARCHAR(100));";

        #endregion

        #region Static Methods

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

        #endregion

        #endregion
    }
}
