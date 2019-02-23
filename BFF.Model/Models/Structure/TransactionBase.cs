using System;
using BFF.Core.Helper;
using BFF.Model.Repositories;
using BFF.Persistence.Models;

namespace BFF.Model.Models.Structure
{
    public interface ITransactionBase : ITransBase
    {
        IAccount Account { get; set; }

        IPayee Payee { get; set; }
    }

    internal abstract class TransactionBase<T, TPersistence> : TransBase<T, TPersistence>, ITransactionBase 
        where T : class, ITransactionBase
        where TPersistence : class, IPersistenceModel
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
            TPersistence backingPersistenceModel,
            IRepository<T, TPersistence> repository,
            IRxSchedulerProvider rxSchedulerProvider,
            bool isInserted,
            IFlag flag,
            string checkNumber,
            DateTime date,
            IAccount account = null,
            IPayee payee = null,
            string memo = null,
            bool? cleared = null)
            : base(backingPersistenceModel, repository, rxSchedulerProvider, flag, checkNumber, date, isInserted, memo, cleared)
        {
            _account = account;
            _payee = payee;
        }
    }
}