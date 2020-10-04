using System;
using BFF.Model.Models.Structure;

namespace BFF.Model.Models
{
    public interface ITransaction : ITransactionBase, IHaveCategory
    {
        long Sum { get; set; }
    }

    public abstract class Transaction : TransactionBase, ITransaction
    {
        private ICategoryBase _category;
        private long _sum;
        
        public Transaction(
            DateTime date,
            IFlag flag,
            string checkNumber,
            IAccount account, 
            IPayee payee, 
            ICategoryBase category,
            string memo, 
            long sum, 
            bool cleared)
            : base(flag, checkNumber, date, account, payee, memo, cleared)
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
