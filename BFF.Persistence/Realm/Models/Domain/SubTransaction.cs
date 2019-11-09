using System;
using System.Linq;
using System.Threading.Tasks;
using BFF.Core.Helper;
using BFF.Model.Models.Structure;
using BFF.Persistence.Realm.ORM;
using BFF.Persistence.Realm.ORM.Interfaces;

namespace BFF.Persistence.Realm.Models.Domain
{
    internal class SubTransaction : Model.Models.SubTransaction, IRealmModel<Persistence.SubTransaction>
    {
        private readonly IUpdateBudgetCache _updateBudgetCache;
        private readonly RealmObjectWrap<Persistence.SubTransaction> _realmObjectWrap;

        public SubTransaction(
            ICrudOrm<Persistence.SubTransaction> crudOrm,
            IUpdateBudgetCache updateBudgetCache,
            IRxSchedulerProvider rxSchedulerProvider,
            Persistence.SubTransaction realmObject,
            ICategoryBase category,
            string memo, 
            long sum) : base(rxSchedulerProvider, category, memo, sum)
        {
            _updateBudgetCache = updateBudgetCache;
            _realmObjectWrap = new RealmObjectWrap<Persistence.SubTransaction>(
                realmObject,
                realm =>
                {
                    var dbSetting = realm.All<Persistence.DbSetting>().First();
                    var id = dbSetting.NextSubTransactionId++;
                    realm.Add(dbSetting, true);
                    var ro = new Persistence.SubTransaction{ Id = id };
                    UpdateRealmObject(ro);
                    return ro;
                },
                UpdateRealmObject,
                crudOrm);
            
            void UpdateRealmObject(Persistence.SubTransaction ro)
            {
                ro.Parent = 
                    Parent is null
                        ? null
                        : (Parent as ParentTransaction)?.RealmObject
                          ?? throw new ArgumentException("Model objects from different backends shouldn't be mixed");
                ro.Category =
                    Category is null
                        ? null
                        : (Category as Category)?.RealmObject
                          ?? (Category as IncomeCategory)?.RealmObject
                          ?? throw new ArgumentException("Model objects from different backends shouldn't be mixed");
                ro.Memo = Memo;
                ro.Sum = Sum;
            }
        }

        public override bool IsInserted => _realmObjectWrap.IsInserted;

        public Persistence.SubTransaction RealmObject => _realmObjectWrap.RealmObject;

        public override async Task InsertAsync()
        {
            await _realmObjectWrap.InsertAsync().ConfigureAwait(false);
            if (_realmObjectWrap.RealmObject != null)
                await _updateBudgetCache.OnTransactionInsertOrDeleteAsync(
                    _realmObjectWrap.RealmObject.Category,
                    _realmObjectWrap.RealmObject.Parent.Date).ConfigureAwait(false);
        }

        public override async Task DeleteAsync()
        {
            var temp = _realmObjectWrap.RealmObject;
            await _realmObjectWrap.DeleteAsync().ConfigureAwait(false);
            if (temp != null)
                await _updateBudgetCache.OnTransactionInsertOrDeleteAsync(
                    temp.Category,
                    temp.Parent.Date).ConfigureAwait(false);

        }

        protected override async Task UpdateAsync()
        {
            var beforeCategory = _realmObjectWrap.RealmObject?.Category;
            var beforeDate = _realmObjectWrap.RealmObject?.Parent.Date;
            var beforeSum = _realmObjectWrap.RealmObject?.Sum;
            // The change already occured for the domain model but wasn't yet updated in the realm object
            var afterCategory = (Category as Category)?.RealmObject;
            var afterDate = new DateTimeOffset(Parent.Date, TimeSpan.Zero);
            var afterSum = Sum;
            await _realmObjectWrap.UpdateAsync().ConfigureAwait(false);
            if (beforeDate is null) return;
            await _updateBudgetCache
                .OnTransactionUpdateAsync(beforeCategory, beforeDate.Value, beforeSum.Value, afterCategory, afterDate, afterSum)
                .ConfigureAwait(false);
        }
    }
}
