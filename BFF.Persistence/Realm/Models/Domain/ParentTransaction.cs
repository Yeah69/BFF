using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        private readonly ObservableCollection<ISubTransaction> _subTransactions;
        private readonly RealmObjectWrap<ITransRealm> _realmObjectWrap;

        public ParentTransaction(
            ICrudOrm<ITransRealm> crudOrm,
            IRealmSubTransactionRepository subTransactionRepository,
            IRxSchedulerProvider rxSchedulerProvider,
            ITransRealm realmObject,
            DateTime date, 
            IFlag flag,
            string checkNumber, 
            IAccount account, 
            IPayee payee,
            string memo, 
            bool cleared) : base(rxSchedulerProvider, date, flag, checkNumber, account, payee, memo, cleared)
        {
            _realmObjectWrap = new RealmObjectWrap<ITransRealm>(
                realmObject,
                realm =>
                {
                    var id = realm.All<Trans>().Count();
                    var ro = new Trans{ Id = id };
                    UpdateRealmObject(ro);
                    return ro;
                },
                UpdateRealmObject,
                crudOrm);

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
            
            void UpdateRealmObject(ITransRealm ro)
            {
                ro.Account =
                    Account is null
                        ? null
                        : (Account as Account)?.RealmObject
                          ?? throw new ArgumentException("Model objects from different backends shouldn't be mixed");
                ro.Date = Date;
                ro.Payee =
                    Payee is null
                        ? null
                        : (Payee as Payee)?.RealmObject
                          ?? throw new ArgumentException("Model objects from different backends shouldn't be mixed");
                ro.CheckNumber = CheckNumber;
                ro.Flag =
                    Flag is null
                        ? null
                        : (Flag as Flag)?.RealmObject
                          ?? throw new ArgumentException("Model objects from different backends shouldn't be mixed");
                ro.Memo = Memo;
                ro.Cleared = Cleared;
                ro.Type = TransType.ParentTransaction;

                ro.FromAccount = null;
                ro.ToAccount = null;
                ro.Category = null;
                ro.Sum = 0;
            }
        }

        public override bool IsInserted => _realmObjectWrap.IsInserted;

        public ITransRealm RealmObject => _realmObjectWrap.RealmObject;

        public override Task InsertAsync()
        {
            return _realmObjectWrap.InsertAsync();
        }

        public override Task DeleteAsync()
        {
            return _realmObjectWrap.DeleteAsync();
        }

        protected override Task UpdateAsync()
        {
            return _realmObjectWrap.UpdateAsync();
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
