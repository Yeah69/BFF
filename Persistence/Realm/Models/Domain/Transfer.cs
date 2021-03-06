﻿using System;
using System.Linq;
using System.Threading.Tasks;
using BFF.Model.Models;
using BFF.Persistence.Helper;
using BFF.Persistence.Realm.Models.Persistence;
using BFF.Persistence.Realm.ORM.Interfaces;
using MrMeeseeks.Extensions;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class Transfer : Model.Models.Transfer, IRealmModel<Trans>
    {
        private readonly RealmObjectWrap<Trans> _realmObjectWrap;

        public Transfer(
            // parameters
            Trans? realmObject,
            DateTime date,
            IFlag? flag, 
            string checkNumber, 
            IAccount? fromAccount,
            IAccount? toAccount,
            string memo, 
            long sum,
            bool cleared,
            
            // dependencies
            ICrudOrm<Trans> crudOrm) : base(date, flag, checkNumber, fromAccount, toAccount, memo, sum, cleared)
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
            
            void UpdateRealmObject(Trans ro)
            {
                ro.FromAccount =
                    FromAccount is null
                        ? null
                        : (FromAccount as Account)?.RealmObject
                          ?? throw new ArgumentException("Model objects from different backends shouldn't be mixed");
                ro.ToAccount =
                    ToAccount is null
                        ? null
                        : (ToAccount as Account)?.RealmObject
                          ?? throw new ArgumentException("Model objects from different backends shouldn't be mixed");
                ro.Date = new DateTimeOffset(Date.Year, Date.Month, Date.Day, Date.Hour, Date.Minute, Date.Second, Date.Millisecond, TimeSpan.Zero);
                ro.MonthIndex = ro.Date.ToMonthIndex();
                ro.CheckNumber = CheckNumber;
                ro.Flag =
                    Flag is null
                        ? null
                        : (Flag as Flag)?.RealmObject
                          ?? throw new ArgumentException("Model objects from different backends shouldn't be mixed");
                ro.Memo = Memo;
                ro.Sum = Sum;
                ro.Cleared = Cleared;
                ro.TypeIndex = (int) TransType.Transfer;

                ro.Account = null;
                ro.Payee = null;
                ro.Category = null;
            }
        }

        public override bool IsInserted => _realmObjectWrap.IsInserted;

        public Trans? RealmObject => _realmObjectWrap.RealmObject;

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
    }
}
