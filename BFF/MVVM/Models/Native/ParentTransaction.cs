using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.Models.Native
{
    public interface IParentTransaction : IParentTransInc
    {
        ObservableCollection<ISubTransaction> SubTransactions { get; }
    }

    /// <summary>
    /// A Transaction, which is split into several SubTransactions
    /// </summary>
    public class ParentTransaction : ParentTransInc<IParentTransaction>, IParentTransaction
    {
        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="account">The Account to which this belongs</param>
        /// <param name="payee">To whom was payeed or who payeed</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        public ParentTransaction(
            IRepository<IParentTransaction> repository,
            IEnumerable<ISubTransaction> subTransactions,
            long id,
            DateTime date,
            IAccount account = null,
            IPayee payee = null,
            string memo = null,
            bool? cleared = null)
            : base(repository, id, date, account, payee, memo, cleared)
        {
            SubTransactions = new ObservableCollection<ISubTransaction>(subTransactions);
            SubTransactions.ObserveAddChanged().Subscribe(st => st.Parent = this);
            foreach (var subTransaction in SubTransactions)
            {
                subTransaction.Parent = this;
            }
        }

        public ObservableCollection<ISubTransaction> SubTransactions { get; }
    }
}
