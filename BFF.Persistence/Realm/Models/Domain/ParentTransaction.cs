using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;
using Reactive.Bindings.Extensions;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal sealed class ParentTransaction : Model.Models.ParentTransaction, IRealmModel<ITransRealm>
    {
        private readonly ICrudOrm<ITransRealm> _crudOrm;
        private bool _isInserted;

        private readonly ObservableCollection<ISubTransaction> _subTransactions;

        public ParentTransaction(
            ICrudOrm<ITransRealm> crudOrm,
            IRealmSubTransactionRepository subTransactionRepository,
            IRxSchedulerProvider rxSchedulerProvider,
            ITransRealm realmObject,
            bool isInserted,
            DateTime date, 
            IFlag flag,
            string checkNumber, 
            IAccount account, 
            IPayee payee,
            string memo, 
            bool cleared) : base(rxSchedulerProvider, date, flag, checkNumber, account, payee, memo, cleared)
        {
            RealmObject = realmObject;
            _crudOrm = crudOrm;
            _isInserted = isInserted;

            _subTransactions = new ObservableCollection<ISubTransaction>();
            SubTransactions = new ReadOnlyObservableCollection<ISubTransaction>(_subTransactions);

            subTransactionRepository
                .GetChildrenOfAsync(RealmObject)
                .ContinueWith(async t =>
                {
                    foreach (var subTransaction in await t.ConfigureAwait(false))
                    {
                        _subTransactions.Add(subTransaction);
                    }
                    SubTransactions.ObserveAddChanged().Subscribe(st => st.Parent = this);

                    foreach (var subTransaction in SubTransactions)
                    {
                        subTransaction.Parent = this;
                    }
                });
        }

        public ITransRealm RealmObject { get; }

        public override bool IsInserted => _isInserted;

        public override async Task InsertAsync()
        {
            _isInserted = await _crudOrm.CreateAsync(RealmObject).ConfigureAwait(false);
        }

        public override async Task DeleteAsync()
        {
            await _crudOrm.DeleteAsync(RealmObject).ConfigureAwait(false);
            _isInserted = false;
        }

        protected override async Task UpdateAsync()
        {
            await _crudOrm.UpdateAsync(RealmObject).ConfigureAwait(false);
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
