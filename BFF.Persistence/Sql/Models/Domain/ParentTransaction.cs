using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Sql.Models.Persistence;
using BFF.Persistence.Sql.ORM.Interfaces;
using BFF.Persistence.Sql.Repositories.ModelRepositories;
using Reactive.Bindings.Extensions;

namespace BFF.Persistence.Sql.Models.Domain
{
    internal sealed class ParentTransaction : Model.Models.ParentTransaction, ISqlModel
    {
        private readonly ICrudOrm<ITransSql> _crudOrm;

        private ObservableCollection<ISubTransaction> _subTransactions;

        public ParentTransaction(
            ICrudOrm<ITransSql> crudOrm,
            ISubTransactionRepository subTransactionRepository,
            IRxSchedulerProvider rxSchedulerProvider, 
            long id,
            DateTime date, 
            IFlag flag,
            string checkNumber, 
            IAccount account, 
            IPayee payee,
            string memo, 
            bool cleared) : base(rxSchedulerProvider, date, flag, checkNumber, account, payee, memo, cleared)
        {
            Id = id;
            _crudOrm = crudOrm;

            SubTransactions = new ReadOnlyObservableCollection<ISubTransaction>(new ObservableCollection<ISubTransaction>());

            subTransactionRepository
                .GetChildrenOfAsync(id)
                .ContinueWith(async t =>
                {
                    _subTransactions = new ObservableCollection<ISubTransaction>(await t.ConfigureAwait(false));
                    SubTransactions = new ReadOnlyObservableCollection<ISubTransaction>(_subTransactions);
                    SubTransactions.ObserveAddChanged().Subscribe(st => st.Parent = this);

                    foreach (var subTransaction in SubTransactions)
                    {
                        subTransaction.Parent = this;
                    }
                    OnPropertyChanged(nameof(SubTransactions));
                });
        }

        public long Id { get; private set; }

        public override bool IsInserted => Id > 0;

        public override async Task InsertAsync()
        {
            Id = await _crudOrm.CreateAsync(CreatePersistenceObject()).ConfigureAwait(false);
        }

        public override async Task DeleteAsync()
        {
            await _crudOrm.DeleteAsync(CreatePersistenceObject()).ConfigureAwait(false);
        }

        protected override async Task UpdateAsync()
        {
            await _crudOrm.UpdateAsync(CreatePersistenceObject()).ConfigureAwait(false);
        }

        private ITransSql CreatePersistenceObject()
        {
            if (!(Account is Account)
                && (Flag != null || !(Flag is Flag))
                && (Payee != null || !(Payee is Payee)))
                throw new ArgumentException("Cannot create persistence object if parts are from another backend");

            return new Trans
            {
                Id = Id,
                Date = Date,
                AccountId = ((Account)Account).Id,
                CategoryId = -69,
                PayeeId = (Payee as Payee)?.Id,
                FlagId = (Flag as Flag)?.Id,
                CheckNumber = CheckNumber,
                Memo = Memo,
                Sum = 0,
                Cleared = Cleared ? 1 : 0,
                Type = nameof(TransType.ParentTransaction)
            };
        }

        public override ReadOnlyObservableCollection<ISubTransaction> SubTransactions { get; protected set; }

        public override void AddSubElement(ISubTransaction subTransaction)
        {
            subTransaction.Parent = this;
            _subTransactions.Add(subTransaction);
        }

        public override void RemoveSubElement(ISubTransaction subTransaction)
        {
            _subTransactions.Remove(subTransaction);
        }
    }
}
