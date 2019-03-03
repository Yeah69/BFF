using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;

namespace BFF.Model.Models
{
    public interface IAccount : ICommonProperty
    {
        long StartingBalance { get; set; }

        DateTime StartingDate { get; set; }
        Task<long?> GetClearedBalanceAsync();

        Task<long?> GetClearedBalanceUntilNowAsync();

        Task<long?> GetUnclearedBalanceAsync();

        Task<long?> GetUnclearedBalanceUntilNowAsync();

        Task<IEnumerable<ITransBase>> GetTransPageAsync(int offset, int pageSize);

        Task<long> GetTransCountAsync();
    }

    public abstract class Account : CommonProperty, IAccount
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

        public abstract Task<long?> GetClearedBalanceAsync();
        public abstract Task<long?> GetClearedBalanceUntilNowAsync();
        public abstract Task<long?> GetUnclearedBalanceAsync();
        public abstract Task<long?> GetUnclearedBalanceUntilNowAsync();
        public abstract Task<IEnumerable<ITransBase>> GetTransPageAsync(int offset, int pageSize);
        public abstract Task<long> GetTransCountAsync();

        public Account(
            IRxSchedulerProvider rxSchedulerProvider,
            DateTime startingDate, 
            string name, 
            long startingBalance) 
            : base(rxSchedulerProvider, name)
        {
            _startingBalance = startingBalance;
            _startingDate = startingDate;
        }
    }
}
