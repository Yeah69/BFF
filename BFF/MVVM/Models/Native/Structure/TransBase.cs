using System;
using BFF.DB;

namespace BFF.MVVM.Models.Native.Structure
{
    public enum TransType
    {
        Transaction = 1,
        Transfer = 2,
        ParentTransaction = 3
    }

    public interface ITransBase : ITransLike
    {
        IFlag Flag { get; set; }

        string CheckNumber { get; set; }
        
        DateTime Date { get; set; }
        
        bool Cleared { get; set; }
    }
    
    public abstract class TransBase<T> : TransLike<T>, ITransBase where T : class, ITransBase
    {
        private DateTime _date;
        private bool _cleared;
        private string _checkNumber;
        private IFlag _flag;

        public IFlag Flag
        {
            get => _flag;
            set
            {
                if (_flag == value) return;
                _flag = value;
                UpdateAndNotify();
                OnPropertyChanged();
            }
        }

        public string CheckNumber
        {
            get => _checkNumber;
            set
            {
                if (_checkNumber == value) return;
                _checkNumber = value;
                UpdateAndNotify();
                OnPropertyChanged();
            }
        }
        
        public DateTime Date
        {
            get => _date;
            set
            {
                if (_date == value) return;
                _date = value;
                UpdateAndNotify();
                OnPropertyChanged();
            }
        }
        
        public bool Cleared
        {
            get => _cleared;
            set
            {
                if (_cleared == value) return;
                _cleared = value;
                UpdateAndNotify();
                OnPropertyChanged();
            }
        }
        
        protected TransBase(
            IRepository<T> repository,
            IFlag flag,
            string checkNumber,
            DateTime date,
            long id,
            string memo,
            bool? cleared) : base(repository, id, memo)
        {
            _flag = flag;
            _checkNumber = checkNumber;
            _date = date;
            _cleared = cleared ?? _cleared;
        }
    }
}