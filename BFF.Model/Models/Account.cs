using System;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.Model.Repositories;

namespace BFF.Model.Models
{
    public interface IAccount : ICommonProperty
    {
        long StartingBalance { get; set; }

        DateTime StartingDate { get; set; }
    }

    internal class Account : CommonProperty<IAccount>, IAccount
    {
        private long _startingBalance;
        private DateTime _startingDate;
        
        public virtual long StartingBalance
        {
            get => _startingBalance;
            set
            {
                if(_startingBalance == value) return;
                _startingBalance = value;
                UpdateAndNotify();
            }
        }

        public DateTime StartingDate
        {
            get => _startingDate;
            set
            {
                if (_startingDate == value) return;
                _startingDate = value;
                UpdateAndNotify();
            }
        }
        
        public Account(
            IRepository<IAccount> repository,
            IRxSchedulerProvider rxSchedulerProvider,
            DateTime startingDate, 
            long id = -1L, 
            string name = "", 
            long startingBalance = 0L) 
            : base(repository, rxSchedulerProvider, name: name)
        {
            Id = id;
            _startingBalance = startingBalance;
            _startingDate = startingDate;
        }
    }
}
