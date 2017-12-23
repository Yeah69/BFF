using System;
using BFF.DB;

namespace BFF.MVVM.Models.Native.Structure
{
    /// <summary>
    /// Enumerates all Types of Tits
    /// </summary>
    public enum TransType
    {
        Transaction = 1,
        Transfer = 2,
        ParentTransaction = 3
    }

    public interface ITransBase : ITransLike
    {
        string CheckNumber { get; set; }

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
    public abstract class TransBase<T> : TransLike<T>, ITransBase where T : class, ITransBase
    {
        private DateTime _date;
        private bool _cleared;
        private string _checkNumber;

        public string CheckNumber
        {
            get => _checkNumber;
            set
            {
                if (_checkNumber == value) return;
                _checkNumber = value;
                Update();
                OnPropertyChanged();
            }
        }

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
                Update();
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
                Update();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes the TransBase-parts of the object
        /// </summary>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="id">Identification number for the database</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        protected TransBase(IRepository<T> repository, 
                            string checkNumber,
                            DateTime date, 
                            long id, 
                            string memo, 
                            bool? cleared) : base(repository, id, memo)
        {
            _checkNumber = checkNumber;
            _date = date;
            _cleared = cleared ?? _cleared;
        }
    }
}
