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
        Transfer = 3,
        ParentTransaction = 4,
        ParentIncome = 5
    }

    public interface ITitBase : ITitLike
    {
        /// <summary>
        /// Marks when the Tit happened
        /// </summary>
        DateTime Date { get; set; }

        /// <summary>
        /// Gives the possibility to mark a Tit as processed or not
        /// </summary>
        bool Cleared { get; set; }
    }

    /// <summary>
    /// Base class for all Tit classes, which are not SubElements (TIT := Transaction Income Transfer)
    /// </summary>
    public abstract class TitBase : TitLike, ITitBase
    {
        private DateTime _date;
        private bool _cleared;

        /// <summary>
        /// Marks when the Tit happened
        /// </summary>
        public DateTime Date
        {
            get => _date;
            set
            {
                if(_date == value) return;
                _date = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gives the possibility to mark a Tit as processed or not
        /// </summary>
        public bool Cleared
        {
            get => _cleared;
            set
            {
                if(_cleared == value) return;
                _cleared = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes the TitBase-parts of the object
        /// </summary>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="id">Identification number for the database</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        protected TitBase(DateTime date, long id = -1L, string memo = null, bool? cleared = null) : base(id, memo)
        {
            _date = date;
            _cleared = cleared ?? _cleared;
        }
    }
}
