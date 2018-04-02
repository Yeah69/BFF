using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.Models.Native
{
    public interface IParentTransaction : ITransactionBase
    {
        ReadOnlyObservableCollection<ISubTransaction> SubTransactions { get; }

        void AddSubElement(ISubTransaction subTransaction);

        void RemoveSubElement(ISubTransaction subTransaction);
    }
    
    public class ParentTransaction : TransactionBase<IParentTransaction>, IParentTransaction
    {
        private readonly ObservableCollection<ISubTransaction> _subTransactions;
        
        public ParentTransaction(
            IRepository<IParentTransaction> repository,
            IEnumerable<ISubTransaction> subTransactions,
            DateTime date,
            long id = -1L,
            IFlag flag = null,
            string checkNumber = "",
            IAccount account = null,
            IPayee payee = null,
            string memo = "",
            bool? cleared = false)
            : base(repository, id, flag, checkNumber, date, account, payee, memo, cleared)
        {
            _subTransactions = new ObservableCollection<ISubTransaction>(subTransactions);
            SubTransactions = new ReadOnlyObservableCollection<ISubTransaction>(_subTransactions);
            SubTransactions.ObserveAddChanged().Subscribe(st => st.Parent = this);
            
            foreach (var subTransaction in SubTransactions)
            {
                subTransaction.Parent = this;
            }
        }

        public ReadOnlyObservableCollection<ISubTransaction> SubTransactions { get; }

        public void AddSubElement(ISubTransaction subTransaction)
        {
            subTransaction.Parent = this;
            _subTransactions.Add(subTransaction);
        }

        public void RemoveSubElement(ISubTransaction subTransaction)
        {
            _subTransactions.Remove(subTransaction);
        }
    }
}
