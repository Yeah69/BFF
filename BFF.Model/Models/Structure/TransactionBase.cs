using System;
using BFF.Core.Helper;

namespace BFF.Model.Models.Structure
{
    public interface ITransactionBase : ITransBase
    {
        IAccount Account { get; set; }

        IPayee Payee { get; set; }
    }

    public abstract class TransactionBase : TransBase, ITransactionBase
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
            }
        }

        protected TransactionBase(
            IRxSchedulerProvider rxSchedulerProvider,
            IFlag flag,
            string checkNumber,
            DateTime date,
            IAccount account,
            IPayee payee,
            string memo,
            bool cleared)
            : base(rxSchedulerProvider, flag, checkNumber, date, memo, cleared)
        {
            _account = account;
            _payee = payee;
        }
    }
}