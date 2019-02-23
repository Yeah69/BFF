using System;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.Model.Repositories;
using BFF.Persistence.Models;

namespace BFF.Model.Models
{
    public interface IAccount : ICommonProperty
    {
        long StartingBalance { get; set; }

        DateTime StartingDate { get; set; }
    }

    internal class Account<TPersistence> : CommonProperty<IAccount, TPersistence>, IAccount
        where TPersistence : class, IPersistenceModel
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
            TPersistence backingPersistenceModel,
            IRepository<IAccount, TPersistence> repository,
            IRxSchedulerProvider rxSchedulerProvider,
            DateTime startingDate, 
            bool isInserted = false, 
            string name = "", 
            long startingBalance = 0L) 
            : base(backingPersistenceModel, repository, rxSchedulerProvider, name: name, isInserted: isInserted)
        {
            _startingBalance = startingBalance;
            _startingDate = startingDate;
        }
    }
}
