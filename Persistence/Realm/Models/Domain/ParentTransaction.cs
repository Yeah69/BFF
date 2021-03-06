﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BFF.Model.Models;
using BFF.Persistence.Helper;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using BFF.Persistence.Realm.Repositories.ModelRepositories;
using MrMeeseeks.Extensions;
using Reactive.Bindings.Extensions;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal sealed class ParentTransaction : Model.Models.ParentTransaction, IRealmModel<Trans>
    {
        private readonly ObservableCollection<ISubTransaction> _subTransactions;
        private readonly RealmObjectWrap<Trans> _realmObjectWrap;

        public ParentTransaction(
            // parameters
            Trans? realmObject,
            DateTime date, 
            IFlag? flag,
            string checkNumber, 
            IAccount? account, 
            IPayee? payee,
            string memo, 
            bool cleared,
            
            // dependencies
            ICrudOrm<Trans> crudOrm,
            IRealmSubTransactionRepository subTransactionRepository) : base(date, flag, checkNumber, account, payee, memo, cleared)
        {
            _realmObjectWrap = new RealmObjectWrap<Trans>(
                realmObject,
                realm =>
                {
                    var dbSetting = realm.All<Persistence.DbSetting>().First();
                    var id = dbSetting.NextTransId++;
                    realm.Add(dbSetting, true);
                    var ro = new Trans{ Id = id };
                    UpdateRealmObject(ro);
                    return ro;
                },
                UpdateRealmObject,
                crudOrm);

            _subTransactions = new ObservableCollection<ISubTransaction>();
            SubTransactions = new ReadOnlyObservableCollection<ISubTransaction>(_subTransactions);

            if (realmObject is not null)
                subTransactionRepository
                    .GetChildrenOfAsync(RealmObject ?? throw new NullReferenceException("Shouldn't be null at that point"))
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
            
            void UpdateRealmObject(Trans ro)
            {
                ro.Account =
                    Account is null
                        ? null
                        : (Account as Account)?.RealmObject
                          ?? throw new ArgumentException("Model objects from different backends shouldn't be mixed");
                ro.Date = new DateTimeOffset(Date.Year, Date.Month, Date.Day, Date.Hour, Date.Minute, Date.Second, Date.Millisecond, TimeSpan.Zero);
                ro.MonthIndex = ro.Date.ToMonthIndex();
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
                ro.TypeIndex = (int) TransType.ParentTransaction;

                ro.FromAccount = null;
                ro.ToAccount = null;
                ro.Category = null;
                ro.Sum = 0;
            }
        }

        public override bool IsInserted => _realmObjectWrap.IsInserted;

        public Trans? RealmObject => _realmObjectWrap.RealmObject;

        public override Task InsertAsync()
        {
            return _realmObjectWrap.InsertAsync();
        }

        public override async Task DeleteAsync()
        {
            foreach (var subTransaction in SubTransactions)
            {
                await subTransaction.DeleteAsync().ConfigureAwait(false);
            }

            await _realmObjectWrap.DeleteAsync().ConfigureAwait(false);
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
