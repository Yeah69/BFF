using System;
using BFF.Core;
using BFF.DB;

namespace BFF.MVVM.Models.Native.Structure
{
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
            }
        }
        
        public DateTime Date
        {
            get => _date;
            set
            {
                if (_date == value) return;
                var previousDate = _date;
                _date = value;
                UpdateAndNotify();
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
            }
        }
        
        protected TransBase(
            IRepository<T> repository, 
            IRxSchedulerProvider rxSchedulerProvider,
            IFlag flag,
            string checkNumber,
            DateTime date,
            long id,
            string memo,
            bool? cleared) : base(repository, rxSchedulerProvider, id, memo)
        {
            _flag = flag;
            _checkNumber = checkNumber;
            _date = date;
            _cleared = cleared ?? _cleared;
        }
    }
}