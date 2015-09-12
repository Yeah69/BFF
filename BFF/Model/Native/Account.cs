using System.Collections.Generic;
using System.Linq;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native
{
    [Table(nameof(Account))]
    class Account : DataModelBase
    {
        #region Non-Static

        #region Properties

        [Write(false)]
        public override string CreateTableStatement => $@"CREATE TABLE [{nameof(Account)}](
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
