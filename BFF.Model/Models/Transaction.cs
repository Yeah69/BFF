using System;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.Model.Repositories;
using BFF.Persistence.Models;

namespace BFF.Model.Models
{
    public interface ITransaction : ITransactionBase, IHaveCategory
    {
        long Sum { get; set; }
    }

    internal class Transaction<TPersistence> : TransactionBase<ITransaction, TPersistence>, ITransaction
        where TPersistence : class, IPersistenceModel
    {
        private ICategoryBase _category;
        private long _sum;
        
        public Transaction(
            TPersistence backingPersistenceModel,
            IRepository<ITransaction, TPersistence> repository, 
            IRxSchedulerProvider rxSchedulerProvider,
            DateTime date,
            bool isInserted = false,
            IFlag flag = null,
            string checkNumber = "",
            IAccount account = null, 
            IPayee payee = null, 
            ICategoryBase category = null,
            string memo = "", 
            long sum = 0L, 
            bool? cleared = false)
            : base(backingPersistenceModel, repository, rxSchedulerProvider, isInserted, flag, checkNumber, date, account, payee, memo, cleared)
        {
            _category = category;
            _sum = sum;
        }
        
        public ICategoryBase Category
        {
            get => _category;
            set
            {
                if (value is null)
                {
                    OnPropertyChanged();
                    return;
                }
                if(_category == value) return;
                _category = value;
                UpdateAndNotify();
            }
        }
        
        public long Sum
        {
            get => _sum;
            set
            {
                if(_sum == value) return;
                _sum = value;
                UpdateAndNotify();
            }
        }
    }
}
