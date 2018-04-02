using System;
using BFF.DB;

namespace BFF.MVVM.Models.Native.Structure
{
    public interface ITransactionBase : ITransBase
    {
        IAccount Account { get; set; }
        
        IPayee Payee { get; set; }
    }
    
    public abstract class TransactionBase<T> : TransBase<T>, ITransactionBase where T : class, ITransactionBase
    {
        private IAccount _account;
        private IPayee _payee;
        
        public IAccount Account
        {
            get => _account;
            set
            {
                if (_account == value) return;
                _account = value;
                UpdateAndNotify();
                OnPropertyChanged();
            }
        }
        
        public IPayee Payee
        {
            get => _payee;
            set
            {
                if (_payee == value) return;
                _payee = value;
                UpdateAndNotify();
                OnPropertyChanged();
            }
        }
        
        protected TransactionBase(
            IRepository<T> repository,
            long id,
            IFlag flag,
            string checkNumber,
            DateTime date,
            IAccount account = null,
            IPayee payee = null,
            string memo = null,
            bool? cleared = null)
            : base(repository, flag, checkNumber, date, id, memo, cleared)
        {
            _account = account;
            _payee = payee;
        }
    }
}