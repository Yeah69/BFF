using System;

namespace BFF.Model.Native.Structure
{
    public enum TitType
    {
        Transaction = 1,
        Income = 2,
        Transfer = 3
    }

    /// <summary>
    /// Base class for all Tit classes, which are not SubElements
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
                Update();
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
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes the TitBase-parts of the object
        /// </summary>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        protected TitBase(string memo = null, bool? cleared = null) : base(memo)
        {
            ConstrDbLock = true;
            _cleared = cleared ?? _cleared;
            ConstrDbLock = false;
        }
    }
}
