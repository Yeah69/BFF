using System;
using System.Collections.Generic;
using System.Linq;
using BFF.Model.Native.Structure;

namespace BFF.Model.Native
{
    /// <summary>
    /// Tits can be added to an Account
    /// </summary>
    public class Account : CommonProperty
    {
        private long _startingBalance;

        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public virtual long StartingBalance
        {
            get { return _startingBalance; }
            set
            {
                _startingBalance = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Representing String
        /// </summary>
        /// <returns>Just the Name-property</returns>
        public override string ToString()
        {
            return Name;
        }
        
        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="id">This objects Id</param>
        /// <param name="name">Name of the Account</param>
        /// <param name="startingBalance">Starting balance of the Account</param>
        public Account(long id = -1L, string name = null, long startingBalance = 0L) : base(name)
        {
            Id = id;
            _startingBalance = startingBalance;
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

        public static void ClearCache()
        {
            Cache.Clear();
        }

        #region Overrides of DataModelBase

        protected override void InsertToDb()
        {
            throw new NotImplementedException();
        }

        protected override void UpdateToDb()
        {
            throw new NotImplementedException();
        }

        protected override void DeleteFromDb()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
