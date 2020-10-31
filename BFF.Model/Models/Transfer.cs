using System;
using BFF.Model.Models.Structure;

namespace BFF.Model.Models
{
    public interface ITransfer : ITransBase
    {
        IAccount? FromAccount { get; set; }
        
        IAccount? ToAccount { get; set; }
        
        long Sum { get; set; }
    }

    public  abstract class Transfer : TransBase, ITransfer
    {
        private IAccount? _fromAccount;
        private IAccount? _toAccount;
        private long _sum;
        
        public IAccount? FromAccount
        {
            get => _fromAccount;
            set
            {
                if(_fromAccount == value) return;
                if (value != null && (_toAccount == value || _toAccount is null)) // If value equals ToAccount, then the FromAccount and ToAccount switch values
                {
                    _toAccount = _fromAccount;
                    OnPropertyChanged(nameof(ToAccount));
                }
                _fromAccount = value;
                UpdateAndNotify();
            }
        }
        
        public IAccount? ToAccount
        {
            get => _toAccount;
            set
            {
                if (_toAccount == value) return;
                if (value != null && (_fromAccount == value || _fromAccount is null)) // If value equals ToAccount, then the FromAccount and ToAccount switch values
                {
                    _fromAccount = _toAccount;
                    OnPropertyChanged(nameof(FromAccount));
                }
                _toAccount = value;
                UpdateAndNotify();
            }
        }
        
        public long Sum
        {
            get => _sum;
            set
            {
                if(_sum == value) return;
                _sum = Math.Abs(value);
                UpdateAndNotify();
            }
        }

        public Transfer(
            DateTime date,
            IFlag? flag,
            string checkNumber,
            IAccount? fromAccount,
            IAccount? toAccount,
            string memo,
            long sum,
            bool cleared)
            : base(flag, checkNumber, date, memo, cleared)
        {
            _fromAccount = fromAccount;
            _toAccount = toAccount;
            _sum = sum;
        }
    }
}
