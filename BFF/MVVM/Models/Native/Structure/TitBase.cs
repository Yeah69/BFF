using System;

namespace BFF.MVVM.Models.Native.Structure
{
    /// <summary>
    /// Enumerates all Types of Tits
    /// </summary>
    public enum TitType
    {
        Transaction = 1,
        Income = 2,
        Transfer = 3
    }

    /// <summary>
    /// Base class for all Tit classes, which are not SubElements (TIT := Transaction Income Transfer)
    /// </summary>
    public abstract class TitBase : TitLike
    {
        private DateTime _date;
        private bool _cleared;

        /// <summary>
        /// Marks when the Tit happened
        /// </summary>
        public DateTime Date
        {
            get { return _date; }
            set
            {
                _date = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gives the possibility to mark a Tit as processed or not
        /// </summary>
        public bool Cleared
        {
            get { return _cleared; }
            set
            {
                _cleared = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes the TitBase-parts of the object
        /// </summary>
        /// <param name="id">Identification number for the database</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="sum">The amount of money, which was payeed or recieved</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        /// <param name="date">Marks when the Tit happened</param>
        protected TitBase(DateTime date, long id = -1L, string memo = null, long sum = 0L, bool? cleared = null) : base(id, memo, sum)
        {
            ConstrDbLock = true;

            _date = date;
            _cleared = cleared ?? _cleared;
            
            ConstrDbLock = false;
        }
    }
}
