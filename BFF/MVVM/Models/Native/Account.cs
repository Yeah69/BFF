using System;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface IAccount : ICommonProperty
    {
        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        long StartingBalance { get; set; }
    }

    /// <summary>
    /// Tits can be added to an Account
    /// </summary>
    public class Account : CommonProperty<Account>, IAccount
    {
        private long _startingBalance;

        /// <summary>
        /// Starting balance of the Account
        /// </summary>
        public virtual long StartingBalance
        {
            get => _startingBalance;
            set
            {
                if(_startingBalance == value) return;
                _startingBalance = value;
                Update();
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="id">This objects Id</param>
        /// <param name="name">Name of the Account</param>
        /// <param name="startingBalance">Starting balance of the Account</param>
        public Account(IRepository<Account> repository, 
                       long id = -1L, 
                       string name = null, 
                       long startingBalance = 0L) 
            : base(repository, name)
        {
            Id = id;
            _startingBalance = startingBalance;
        }
    }
}
