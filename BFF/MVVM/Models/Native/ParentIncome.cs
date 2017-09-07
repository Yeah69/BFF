using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BFF.DB;
using BFF.MVVM.Models.Native.Structure;
using Reactive.Bindings.Extensions;

namespace BFF.MVVM.Models.Native
{
    public interface IParentIncome : IParentTransInc
    {
        ObservableCollection<ISubIncome> SubIncomes { get; }
    }

    /// <summary>
    /// An Income, which is split into several SubIncomes
    /// </summary>
    public class ParentIncome : ParentTransInc<IParentIncome>, IParentIncome
    {
        /// <summary>
        /// Initializes the object
        /// </summary>
        /// <param name="date">Marks when the Tit happened</param>
        /// <param name="account">The Account to which this belongs</param>
        /// <param name="payee">To whom was payed or who payed</param>
        /// <param name="memo">A note to hint on the reasons of creating this Tit</param>
        /// <param name="cleared">Gives the possibility to mark a Tit as processed or not</param>
        public ParentIncome(
            IRepository<IParentIncome> repository,
            IEnumerable<ISubIncome> subIncomes,
            long id,
            DateTime date,
            IAccount account = null,
            IPayee payee = null,
            string memo = null,
            bool? cleared = null)
            : base(repository, id, date, account, payee, memo, cleared)
        {
            SubIncomes = new ObservableCollection<ISubIncome>(subIncomes);
            SubIncomes.ObserveAddChanged().Subscribe(st => st.Parent = this);
            foreach (var subTransaction in SubIncomes)
            {
                subTransaction.Parent = this;
            }
        }

        public ObservableCollection<ISubIncome> SubIncomes { get; }
    }
}
