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

    /// <summary>
    /// A Transaction, which is split into several SubTransactions
    /// </summary>
    public class ParentTransaction : TransactionBase<IParentTransaction>, IParentTransaction
    {
        private readonly ObservableCollection<ISubTransaction> _subTransactions;

        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="account">The Account to which this belongs</param>
        /// <param name="payee">To whom was payed or who payed</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        public ParentTransaction(
            IRepository<IParentTransaction> repository,
            IEnumerable<ISubTransaction> subTransactions,
            long id,
            string checkNumber,
            DateTime date,
            IAccount account = null,
            IPayee payee = null,
            string memo = null,
            bool? cleared = null)
            : base(repository, id, checkNumber, date, account, payee, memo, cleared)
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
