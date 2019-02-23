using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.Model.Repositories;
using BFF.Persistence.Models;
using Reactive.Bindings.Extensions;

namespace BFF.Model.Models
{
    public interface IParentTransaction : ITransactionBase
    {
        ReadOnlyObservableCollection<ISubTransaction> SubTransactions { get; }

        void AddSubElement(ISubTransaction subTransaction);

        void RemoveSubElement(ISubTransaction subTransaction);
    }

    internal class ParentTransaction<TPersistence> : TransactionBase<IParentTransaction, TPersistence>, IParentTransaction
        where TPersistence : class, IPersistenceModel
    {
        private readonly ObservableCollection<ISubTransaction> _subTransactions;
        
        public ParentTransaction(
            TPersistence backingPersistenceModel,
            IRepository<IParentTransaction, TPersistence> repository,
            IRxSchedulerProvider rxSchedulerProvider,
            IEnumerable<ISubTransaction> subTransactions,
            DateTime date,
            bool isInserted = false,
            IFlag flag = null,
            string checkNumber = "",
            IAccount account = null,
            IPayee payee = null,
            string memo = "",
            bool? cleared = false)
            : base(backingPersistenceModel, repository, rxSchedulerProvider, isInserted, flag, checkNumber, date, account, payee, memo, cleared)
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
