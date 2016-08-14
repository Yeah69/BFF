﻿using System.Windows.Input;
using Dapper.Contrib.Extensions;

namespace BFF.MVVM.Models.Native.Structure
{
    /// <summary>
    /// Base class for all classes which can be shown in the TitDataGrid (TIT := Transaction Income Transfer)
    /// </summary>
    public abstract class TitLike : DataModelBase
    {
        private string _memo;
        private long _sum;

        /// <summary>
        /// A note to hint on the reasons of creating this Tit
        /// </summary>
        public string Memo
        {
            get { return _memo; }
            set
            {
                _memo = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The amount of money, which was payeed or recieved
        /// </summary>
        public virtual long Sum
        {
            get { return _sum; }
            set
            {
                _sum = value; 
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes the TitLike-parts of the object
        /// </summary>
        /// <param name="id">Identification number for the database</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="sum">The amount of money, which was payeed or recieved</param>
        protected TitLike(long id = -1L, string memo = null, long sum = 0L) : base(id)
        {
            _memo = memo ?? _memo;
            if (_sum == 0L || sum != 0L) _sum = sum;
        }

    }
}
