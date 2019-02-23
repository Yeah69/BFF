using System;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.Model.Repositories;
using BFF.Persistence.Models;

namespace BFF.Model.Models
{
    public interface ITransfer : ITransBase
    {
        IAccount FromAccount { get; set; }
        
        IAccount ToAccount { get; set; }
        
        long Sum { get; set; }
    }

    internal class Transfer<TPersistence> : TransBase<ITransfer, TPersistence>, ITransfer
        where TPersistence : class, IPersistenceModel
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
                if (value != null && (_toAccount == value || _toAccount == null)) // If value equals ToAccount, then the FromAccount and ToAccount switch values
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
                if (value != null && (_fromAccount == value || _fromAccount == null)) // If value equals ToAccount, then the FromAccount and ToAccount switch values
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
            TPersistence backingPersistenceModel,
            IRepository<ITransfer, TPersistence> repository, 
            IRxSchedulerProvider rxSchedulerProvider,
            DateTime date,
            bool isInserted = false,
            IFlag flag = null,
            string checkNumber = "",
            IAccount fromAccount = null,
            IAccount toAccount = null,
            string memo = "",
            long sum = 0L,
            bool? cleared = false)
            : base(backingPersistenceModel, repository, rxSchedulerProvider, flag, checkNumber, date, isInserted, memo, cleared)
        {
            _fromAccount = fromAccount;
            _toAccount = toAccount;
            _sum = sum;
        }
    }
}
