using System;

namespace BFF.Model.Models.Structure
{
    public interface ITransBase : ITransLike
    {
        IFlag? Flag { get; set; }

        string CheckNumber { get; set; }
        
        DateTime Date { get; set; }
        
        bool Cleared { get; set; }
    }

    public abstract class TransBase : TransLike, ITransBase
    {
        private DateTime _date;
        private bool _cleared;
        private string _checkNumber;
        private IFlag? _flag;

        public IFlag? Flag
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
            IFlag? flag,
            string checkNumber,
            DateTime date,
            string memo,
            bool cleared) : base(memo)
        {
            _flag = flag;
            _checkNumber = checkNumber;
            _date = date;
            _cleared = cleared;
        }
    }
}