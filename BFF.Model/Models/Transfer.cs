using System;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.Model.Repositories;

namespace BFF.Model.Models
{
    public interface ITransfer : ITransBase
    {
        IAccount FromAccount { get; set; }
        
        IAccount ToAccount { get; set; }
        
        long Sum { get; set; }
    }

    internal class Transfer : TransBase<ITransfer>, ITransfer
    {
        private IAccount _fromAccount;
        private IAccount _toAccount;
        private long _sum;
        
        public IAccount FromAccount
        {
            get => _fromAccount;
            set
            {
                if(_fromAccount == value) return;
                if (value != null && _toAccount == value) // If value equals ToAccount, then the FromAccount and ToAccount switch values
                {
                    _toAccount = _fromAccount;
                    OnPropertyChanged(nameof(ToAccount));
                }
                _fromAccount = value;
                UpdateAndNotify();
            }
        }
        
        public IAccount ToAccount
        {
            get => _toAccount;
            set
            {
                if (_toAccount == value) return;
                if (value != null && _fromAccount == value) // If value equals ToAccount, then the FromAccount and ToAccount switch values
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
            IRepository<ITransfer> repository, 
            IRxSchedulerProvider rxSchedulerProvider,
            DateTime date,
            long id = -1L,
            IFlag flag = null,
            string checkNumber = "",
            IAccount fromAccount = null,
            IAccount toAccount = null,
            string memo = "",
            long sum = 0L,
            bool? cleared = false)
            : base(repository, rxSchedulerProvider, flag, checkNumber, date, id, memo, cleared)
        {
            _fromAccount = fromAccount;
            _toAccount = toAccount;
            _sum = sum;
        }
    }
}
