﻿using System;
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
    public class Account : CommonProperty, IAccount
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

        #region Overrides of ExteriorCrudBase

        public override void Insert(IBffOrm orm)
        {
            orm?.Insert(this);
        }

        public override void Update(IBffOrm orm)
        {
            orm?.Update(this);
        }

        public override void Delete(IBffOrm orm)
        {
            orm?.Delete(this);
        }

        #endregion
    }
}
