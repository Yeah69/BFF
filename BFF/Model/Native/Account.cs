﻿using System.Collections.Generic;
using System.Linq;
using BFF.Model.Native.Structure;
using Dapper.Contrib.Extensions;

namespace BFF.Model.Native
{
    public class Account : CommonProperties
    {
        //todo: DB updates
        [Key]
        public long Id { get; set; } = -1;

        public string Name { get; set; }

        public long StartingBalance { get; set; } = 0L;

        public override string ToString()
        {
            return Name;
        }

        private static readonly Dictionary<string, Account> Cache = new Dictionary<string, Account>();

        // todo: Refactor the GetOrCreate and GetAllCache into the Conversion/Import class
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
    }
}
