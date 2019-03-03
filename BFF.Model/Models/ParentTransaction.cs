using System;
using System.Collections.ObjectModel;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;

namespace BFF.Model.Models
{
    public interface IParentTransaction : ITransactionBase
    {
        ReadOnlyObservableCollection<ISubTransaction> SubTransactions { get; }

        void AddSubElement(ISubTransaction subTransaction);

        void RemoveSubElement(ISubTransaction subTransaction);
    }

    public abstract class ParentTransaction : TransactionBase, IParentTransaction
    {
        public ParentTransaction(
            IRxSchedulerProvider rxSchedulerProvider,
            DateTime date,
            IFlag flag,
            string checkNumber,
            IAccount account,
            IPayee payee,
            string memo,
            bool cleared)
            : base(rxSchedulerProvider, flag, checkNumber, date, account, payee, memo, cleared)
        {
        }

        public abstract ReadOnlyObservableCollection<ISubTransaction> SubTransactions { get; protected set; }

        public abstract void AddSubElement(ISubTransaction subTransaction);

        public abstract void RemoveSubElement(ISubTransaction subTransaction);
    }
}
