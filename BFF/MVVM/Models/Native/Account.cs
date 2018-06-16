using System;
using BFF.DB;
using BFF.Helper;
using BFF.MVVM.Models.Native.Structure;

namespace BFF.MVVM.Models.Native
{
    public interface IAccount : ICommonProperty
    {
        long StartingBalance { get; set; }

        DateTime StartingDate { get; set; }
    }
    
    public class Account : CommonProperty<IAccount>, IAccount
    {
        private readonly INotifyBudgetOverviewRelevantChange _notifyBudgetOverviewRelevantChange;
        private long _startingBalance;
        private DateTime _startingDate;
        
        public virtual long StartingBalance
        {
            get => _startingBalance;
            set
            {
                if(_startingBalance == value) return;
                _startingBalance = value;
                UpdateAndNotify()
                    .ContinueWith(_ => _notifyBudgetOverviewRelevantChange.Notify(StartingDate));
            }
        }

        public DateTime StartingDate
        {
            get => _startingDate;
            set
            {
                if (_startingDate == value) return;
                _startingDate = value;
                UpdateAndNotify()
                    .ContinueWith(_ => _notifyBudgetOverviewRelevantChange.Notify(StartingDate));
            }
        }
        
        public Account(
            IRepository<IAccount> repository,
            IRxSchedulerProvider rxSchedulerProvider,
            INotifyBudgetOverviewRelevantChange notifyBudgetOverviewRelevantChange,
            DateTime startingDate, 
            long id = -1L, 
            string name = "", 
            long startingBalance = 0L) 
            : base(repository, rxSchedulerProvider, name: name)
        {
            Id = id;
            _notifyBudgetOverviewRelevantChange = notifyBudgetOverviewRelevantChange;
            _startingBalance = startingBalance;
            _startingDate = startingDate;
        }
    }
}
